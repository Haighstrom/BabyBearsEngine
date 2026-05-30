using System.Threading;
using System.Threading.Tasks;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Lifecycle state of a <see cref="LoadingScreenWorld"/>.
/// </summary>
public enum LoadingScreenState
{
    /// <summary>The world has been constructed but <see cref="LoadingScreenWorld.Load"/> has not yet started the background task.</summary>
    Pending = 0,

    /// <summary>The background load task is in flight.</summary>
    Loading = 1,

    /// <summary>The load task finished successfully. The world will switch to the next world on the next update tick.</summary>
    Completed = 2,
}

/// <summary>
/// A <see cref="World"/> shown while assets (or any other work) are loaded asynchronously, then
/// automatically hands off to a target world once the load completes.
/// <para>
/// The load delegate runs on a background <see cref="Task"/> and reports progress (0..1) via an
/// <see cref="IProgress{T}"/>. The default visual is a centred <see cref="ProgressBar"/> tracking
/// the reported progress; the bar is exposed via <see cref="Bar"/> for repositioning or
/// restyling, and additional children (background image, text label, animated cog, etc.) can be
/// added with <see cref="World.Add(IAddable)"/>.
/// </para>
/// <para>
/// Exception handling: if the load delegate throws, the exception is re-thrown on the main
/// thread from <see cref="Update"/> and propagates up to <c>GameLauncher.Run</c>'s fatal-error
/// handler. There is no in-band recovery channel — game code that wants to recover from
/// specific failure modes (missing asset, malformed file, etc.) must <c>try/catch</c> inside
/// the load delegate itself.
/// </para>
/// </summary>
public class LoadingScreenWorld : World
{
    private const float DefaultBarHeight = 30f;
    private const float DefaultBarWidth = 400f;
    // Y position as a fraction of window height — slightly below centre, like a typical splash screen.
    private const float DefaultBarYFraction = 0.7f;

    private readonly Func<IProgress<float>, CancellationToken, Task>? _asyncLoadWork;
    private readonly Action<IProgress<float>, CancellationToken>? _syncLoadWorkWithGL;
    private readonly Func<IWorld> _nextWorldFactory;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly AtomicFloatProgress _progress = new();
    private readonly IWorldSwitcher? _worldSwitcherOverride;

    private ProgressBar _bar;
    private Task? _loadTask;
    // Sync object created by the worker thread at the end of loadAssets and waited on by the
    // main thread before handing off to the next world. Both contexts share sync objects
    // (they're shareable across shared contexts), and only the main-thread wait gives the
    // driver an opportunity to make worker-side texture uploads visible to the main context.
    private long _workerFence = 0;
    // The shared GL context whose lifetime spans the worker task. Created on the main thread
    // (window creation is main-thread-only), used on the worker, then disposed on the main
    // thread (GLFW windows can only be disposed on the main thread).
    private ILoadingGLContext? _workerGLContext = null;
    private LoadingScreenState _state = LoadingScreenState.Pending;
    private bool _handoffDone = false;

    /// <summary>
    /// Creates a loading screen world that runs <paramref name="loadWork"/> on a background
    /// thread and switches to the world produced by <paramref name="nextWorldFactory"/> when it
    /// completes successfully. The default progress bar visual is used.
    /// </summary>
    /// <param name="loadWork">The async work to perform. Receives an <see cref="IProgress{T}"/> for reporting progress in [0, 1] and a <see cref="CancellationToken"/> that fires when this world is unloaded.</param>
    /// <param name="nextWorldFactory">Factory for the world to switch to once loading completes. Invoked on the main thread.</param>
    public LoadingScreenWorld(
        Func<IProgress<float>, CancellationToken, Task> loadWork,
        Func<IWorld> nextWorldFactory)
        : this(loadWork, nextWorldFactory, ProgressBarTheme.Default)
    {
    }

    /// <summary>
    /// Creates a loading screen world with a custom <see cref="ProgressBarTheme"/> for the
    /// default centred progress bar.
    /// </summary>
    /// <param name="loadWork">The async work to perform. Receives an <see cref="IProgress{T}"/> for reporting progress in [0, 1] and a <see cref="CancellationToken"/> that fires when this world is unloaded.</param>
    /// <param name="nextWorldFactory">Factory for the world to switch to once loading completes. Invoked on the main thread.</param>
    /// <param name="barTheme">Visual styling for the default progress bar.</param>
    public LoadingScreenWorld(
        Func<IProgress<float>, CancellationToken, Task> loadWork,
        Func<IWorld> nextWorldFactory,
        ProgressBarTheme barTheme)
        : this(loadWork, nextWorldFactory, barTheme, worldSwitcherOverride: null)
    {
    }

    /// <summary>
    /// Creates a loading screen world that runs <paramref name="loadAssets"/> on a dedicated
    /// background thread that holds a shared OpenGL context, so the delegate can construct GL
    /// resources (<see cref="Worlds.Graphics.Textures.CreateFromFile"/>, font atlases, etc.)
    /// directly without dispatching back to the main thread. Once loading completes, the world
    /// switches to <paramref name="nextWorldFactory"/>'s output.
    /// </summary>
    /// <param name="loadAssets">
    /// The synchronous load work. Receives an <see cref="IProgress{T}"/> for reporting progress
    /// in [0, 1] and a <see cref="CancellationToken"/> that fires when this world is unloaded.
    /// The delegate runs on a background thread with a shared GL context current, so GL resource
    /// constructors can be called directly. Must be fully synchronous — any <c>await</c> can move
    /// execution off the loading thread, away from the GL context.
    /// </param>
    /// <param name="nextWorldFactory">Factory for the world to switch to once loading completes. Invoked on the main thread.</param>
    /// <param name="barTheme">Visual styling for the default progress bar.</param>
    public LoadingScreenWorld(
        Action<IProgress<float>, CancellationToken> loadAssets,
        Func<IWorld> nextWorldFactory,
        ProgressBarTheme barTheme)
        : this(
            asyncLoadWork: null,
            syncLoadWorkWithGL: loadAssets ?? throw new ArgumentNullException(nameof(loadAssets)),
            nextWorldFactory,
            barTheme,
            worldSwitcherOverride: null)
    {
    }

    /// <summary>
    /// Creates a loading screen world running <paramref name="loadAssets"/> with the default
    /// <see cref="ProgressBarTheme"/>.
    /// </summary>
    public LoadingScreenWorld(
        Action<IProgress<float>, CancellationToken> loadAssets,
        Func<IWorld> nextWorldFactory)
        : this(loadAssets, nextWorldFactory, ProgressBarTheme.Default)
    {
    }

    // Internal ctor used by tests to inject a fake world switcher without going through Engine /
    // EngineConfiguration. The public ctors funnel into this one.
    internal LoadingScreenWorld(
        Func<IProgress<float>, CancellationToken, Task> loadWork,
        Func<IWorld> nextWorldFactory,
        ProgressBarTheme barTheme,
        IWorldSwitcher? worldSwitcherOverride)
        : this(
            asyncLoadWork: loadWork ?? throw new ArgumentNullException(nameof(loadWork)),
            syncLoadWorkWithGL: null,
            nextWorldFactory,
            barTheme,
            worldSwitcherOverride)
    {
    }

    private LoadingScreenWorld(
        Func<IProgress<float>, CancellationToken, Task>? asyncLoadWork,
        Action<IProgress<float>, CancellationToken>? syncLoadWorkWithGL,
        Func<IWorld> nextWorldFactory,
        ProgressBarTheme barTheme,
        IWorldSwitcher? worldSwitcherOverride)
    {
        ArgumentNullException.ThrowIfNull(nextWorldFactory);
        ArgumentNullException.ThrowIfNull(barTheme);

        _asyncLoadWork = asyncLoadWork;
        _syncLoadWorkWithGL = syncLoadWorkWithGL;
        _nextWorldFactory = nextWorldFactory;
        _worldSwitcherOverride = worldSwitcherOverride;

        float windowWidth = Window.Width;
        float windowHeight = Window.Height;
        Rect barRect = new(
            (windowWidth - DefaultBarWidth) / 2f,
            windowHeight * DefaultBarYFraction,
            DefaultBarWidth,
            DefaultBarHeight);

        _bar = new ProgressBar(barRect, barTheme);
        Add(_bar);
    }

    /// <summary>
    /// The default progress bar that tracks reported progress. Exposed so callers can reposition,
    /// resize, or restyle it after construction, or replace it via <see cref="World.Remove"/> +
    /// <see cref="World.Add(IAddable)"/> if they want a fully custom visual.
    /// </summary>
    public ProgressBar Bar => _bar;

    /// <summary>The most recent progress value reported by the load delegate, in [0, 1].</summary>
    public float Progress => _progress.Value;

    /// <summary>The world's lifecycle state — see <see cref="LoadingScreenState"/>.</summary>
    public LoadingScreenState State => _state;

    /// <summary>
    /// Raised on the main thread when the load delegate completes successfully, immediately
    /// before the world is switched. Use this hook for any main-thread, post-load work that must
    /// happen before the next world's constructor runs (e.g. promoting CPU-side image data to GL
    /// textures, finalising state derived from the loaded assets).
    /// </summary>
    public event Action? LoadCompleted;

    /// <inheritdoc/>
    /// <remarks>Kicks off the background load task. Idempotent — calling <see cref="Load"/> a second time on the same instance has no effect.</remarks>
    public override void Load()
    {
        base.Load();

        if (_state != LoadingScreenState.Pending)
        {
            return;
        }

        _state = LoadingScreenState.Loading;
        CancellationToken token = _cancellation.Token;

        if (_syncLoadWorkWithGL is not null)
        {
            // Create the shared GL context here on the engine thread (NativeWindow creation is
            // main-thread-only on Windows). The worker thread then claims it via MakeCurrent.
            // The whole sync delegate runs on one Task.Run thread pool thread — no await means
            // no continuation thread switches, so the GL context stays current for the entire load.
            _workerGLContext = EngineConfiguration.GLLoadingContextFactory.CreateSharedContext();
            ILoadingGLContext glCtx = _workerGLContext;
            Action<IProgress<float>, CancellationToken> syncWork = _syncLoadWorkWithGL;
            _loadTask = Task.Run(() =>
            {
                try
                {
                    glCtx.MakeCurrentOnThisThread();
                    syncWork(_progress, token);
                    // Insert a fence at the end of the worker's command stream. The main thread
                    // will wait on this fence before using any loaded resources — without that
                    // wait the driver can leave shared textures in a not-yet-visible state and
                    // the first main-thread sample returns zeros (a black frame).
                    IntPtr fence = OpenTK.Graphics.OpenGL4.GL.FenceSync(
                        OpenTK.Graphics.OpenGL4.SyncCondition.SyncGpuCommandsComplete,
                        OpenTK.Graphics.OpenGL4.WaitSyncFlags.None);
                    OpenTK.Graphics.OpenGL4.GL.Flush();
                    Interlocked.Exchange(ref _workerFence, fence.ToInt64());
                }
                finally
                {
                    // Release the context from this thread; the main thread will Dispose the
                    // underlying GLFW window after observing task completion (GLFW only allows
                    // window disposal on the main thread).
                    glCtx.ReleaseFromWorkerThread();
                }
            }, token);
        }
        else
        {
            _loadTask = Task.Run(() => _asyncLoadWork!(_progress, token), token);
        }
    }

    /// <inheritdoc/>
    /// <remarks>Cancels the background load task (the token passed to the load delegate fires here) before tearing the world down.</remarks>
    public override void Unload()
    {
        if (!_cancellation.IsCancellationRequested)
        {
            _cancellation.Cancel();
        }

        base.Unload();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Mirrors the latest reported progress onto <see cref="Bar"/>, then — if the background task
    /// has finished — raises <see cref="LoadCompleted"/> and triggers the world switch via
    /// <see cref="Engine.ChangeWorld(Func{IWorld})"/>. If the load delegate threw, the exception
    /// is re-thrown here on the main thread (propagating up to <c>GameLauncher.Run</c>'s fatal
    /// handler); game code that wants to recover from a specific failure should
    /// <c>try/catch</c> inside the load delegate itself rather than relying on a callback.
    /// </remarks>
    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        // Progress is reported from the load thread; mirror the latest value onto the bar each
        // frame on the main thread so the visual stays in sync without needing a sync context.
        _bar.AmountFilled = _progress.Value;

        if (_handoffDone || _loadTask is null || !_loadTask.IsCompleted)
        {
            return;
        }

        if (_loadTask.IsFaulted)
        {
            // Re-throw the worker's exception on the main thread so it propagates up to the
            // engine's fatal-error handler. We dispose the GL context first so the GLFW window
            // isn't leaked, but otherwise let the exception bubble — the framework deliberately
            // doesn't try to recover, because it can't tell programmer errors apart from
            // recoverable I/O failures. Game code that wants recovery should try/catch inside
            // the load delegate.
            _handoffDone = true;
            DisposeWorkerGLContext();
            Exception inner = UnwrapAggregate(_loadTask.Exception) ?? new InvalidOperationException("LoadingScreenWorld load task faulted with no exception attached.");
            throw inner;
        }

        if (_loadTask.IsCanceled)
        {
            // The world was unloaded while loading — nothing more to do. State stays Loading
            // because the world is going away anyway; we don't fire any event here.
            _handoffDone = true;
            DisposeWorkerGLContext();
            return;
        }

        if (_syncLoadWorkWithGL is not null)
        {
            // Wait on the worker's end-of-stream fence so all its commands are flushed to the
            // GPU before we proceed. Fences are shareable across shared contexts.
            long fence = Interlocked.Read(ref _workerFence);
            if (fence != 0)
            {
                OpenTK.Graphics.OpenGL4.GL.ClientWaitSync(
                    new IntPtr(fence),
                    OpenTK.Graphics.OpenGL4.ClientWaitSyncFlags.SyncFlushCommandsBit,
                    timeout: 5_000_000_000L);
                OpenTK.Graphics.OpenGL4.GL.DeleteSync(new IntPtr(fence));
            }

            // Invalidate the bind cache so the next render-path BindTexture call definitely
            // hits glBindTexture in the main context — the glBindTexture from a different
            // context is what tells the driver to refresh its cached view of the shared
            // texture. Without this the cache may skip the bind (handle "already current"),
            // the driver serves stale state, and the first sample produces a black frame.
            OpenGLHelper.ResetCache();

            // Dispose the worker's GLFW window on the main thread (GLFW only allows window
            // disposal on the main thread). The worker already released the context via
            // ReleaseFromWorkerThread.
            DisposeWorkerGLContext();
        }

        // Successful completion. Snap the bar to full so a one-shot load with no intermediate
        // progress reports still looks "finished" for the frame before the world switches.
        _progress.Report(1f);
        _bar.AmountFilled = 1f;
        _state = LoadingScreenState.Completed;
        _handoffDone = true;

        LoadCompleted?.Invoke();

        IWorldSwitcher switcher = _worldSwitcherOverride ?? EngineConfiguration.WorldSwitcher;
        switcher.RequestWorldChange(_nextWorldFactory);
    }

    private void DisposeWorkerGLContext()
    {
        if (_workerGLContext is null)
        {
            return;
        }

        _workerGLContext.Dispose();
        _workerGLContext = null;
    }

    private static Exception? UnwrapAggregate(AggregateException? aggregate)
    {
        if (aggregate is null)
        {
            return null;
        }

        return aggregate.InnerExceptions.Count == 1 ? aggregate.InnerException : aggregate;
    }

    /// <summary>
    /// Thread-safe <see cref="IProgress{T}"/> sink for a single <c>float</c> in [0, 1]. The load
    /// thread writes via <see cref="Report"/>; the main thread reads via <see cref="Value"/>.
    /// Implemented with <see cref="Interlocked"/> on the bit pattern so no synchronisation
    /// context or lock is required.
    /// </summary>
    private sealed class AtomicFloatProgress : IProgress<float>
    {
        private int _bits = 0;

        public float Value => BitConverter.Int32BitsToSingle(Volatile.Read(ref _bits));

        public void Report(float value) => Volatile.Write(ref _bits, BitConverter.SingleToInt32Bits(value));
    }
}

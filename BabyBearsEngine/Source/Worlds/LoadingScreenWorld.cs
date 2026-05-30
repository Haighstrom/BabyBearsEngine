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
/// One unit of loading work. The framework runs each step in order on a background thread with
/// a shared OpenGL context current, so the <paramref name="Work"/> delegate can call texture/
/// shader/buffer constructors directly. The <paramref name="Name"/> is exposed via
/// <see cref="LoadingScreenWorld.CurrentStepName"/> for diagnostics or UI display.
/// </summary>
public sealed record LoadStep(string Name, Action Work);

/// <summary>
/// Abstract base for a "splash + progress bar" world that loads assets in the background and
/// then hands off to the next world. Subclass it, override <see cref="AssetsToLoad"/> with the
/// list of work, and override <see cref="NextWorld"/> with what comes next.
///
/// <example><code>
/// internal sealed class GameLoadingWorld : LoadingScreenWorld
/// {
///     private readonly Func&lt;World&gt; _menuFactory;
///     private ITexture _bear = null!;
///     private ITexture _sky  = null!;
///
///     public GameLoadingWorld(Func&lt;World&gt; menuFactory) { _menuFactory = menuFactory; }
///
///     protected override IReadOnlyList&lt;LoadStep&gt; AssetsToLoad =&gt; [
///         new("Loading bear", () =&gt; _bear = Textures.CreateFromFile("Assets/bear.png")),
///         new("Loading sky",  () =&gt; _sky  = Textures.CreateFromFile("Assets/sky.png")),
///     ];
///
///     protected override IWorld NextWorld() =&gt; new MainGameWorld(_menuFactory, _bear, _sky);
/// }
/// </code></example>
///
/// Each step runs on a dedicated worker thread with a shared GL context — GL resource
/// constructors work directly inside the <c>Work</c> delegate. Progress is reported
/// automatically: <c>0/N</c> at the start, advancing by <c>1/N</c> after each step completes.
/// The current step's <see cref="LoadStep.Name"/> is exposed via <see cref="CurrentStepName"/>
/// for UI display.
///
/// Exception handling: if any step's <c>Work</c> delegate throws, the exception propagates to
/// the main thread from <see cref="Update"/> and bubbles up to <c>GameLauncher.Run</c>'s
/// fatal-error handler. Subclasses that want graceful recovery from specific failures should
/// <c>try/catch</c> inside the failing step's delegate.
/// </summary>
public abstract class LoadingScreenWorld : World
{
    private const float DefaultBarHeight = 30f;
    private const float DefaultBarWidth = 400f;
    // Y position as a fraction of window height — slightly below centre, like a typical splash screen.
    private const float DefaultBarYFraction = 0.7f;

    private readonly CancellationTokenSource _cancellation = new();
    private readonly AtomicFloatProgress _progress = new();
    private readonly AtomicStepName _currentStepName = new();
    private readonly IWorldSwitcher? _worldSwitcherOverride;

    private ProgressBar _bar;
    private Task? _loadTask;
    // Sync object created by the worker thread at the end of loading and waited on by the main
    // thread before the world handoff. Fences are shareable across shared contexts, and the
    // main-thread wait is what gives the driver an opportunity to make worker-side texture
    // uploads visible to the main context.
    private long _workerFence = 0;
    // The shared GL context whose lifetime spans the worker task. Created on the main thread
    // (window creation is main-thread-only on Windows), used on the worker, then disposed on
    // the main thread (GLFW windows can only be disposed on the main thread).
    private ILoadingGLContext? _workerGLContext = null;
    private LoadingScreenState _state = LoadingScreenState.Pending;
    private bool _handoffDone = false;

    /// <param name="barTheme">Optional visual styling for the default progress bar. Defaults to <see cref="ProgressBarTheme.Default"/>.</param>
    protected LoadingScreenWorld(ProgressBarTheme? barTheme = null)
        : this(barTheme, worldSwitcherOverride: null)
    {
    }

    // Internal ctor with a test seam — lets unit tests inject a fake world switcher.
    internal LoadingScreenWorld(ProgressBarTheme? barTheme, IWorldSwitcher? worldSwitcherOverride)
    {
        barTheme ??= ProgressBarTheme.Default;
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
    /// Override to declare the loading steps. Each <see cref="LoadStep.Work"/> delegate runs on
    /// a background thread with a shared GL context current — call texture/shader/buffer
    /// constructors directly inside it. The framework reports progress automatically based on
    /// how many steps have completed.
    /// </summary>
    protected abstract IReadOnlyList<LoadStep> AssetsToLoad { get; }

    /// <summary>
    /// Override to return the world to switch to once all steps have completed. Invoked on the
    /// main thread after the last step's work has finished and any loaded GL resources are
    /// fully visible to the main context.
    /// </summary>
    protected abstract IWorld NextWorld();

    /// <summary>
    /// The default progress bar that tracks reported progress. Exposed so subclasses can
    /// reposition, resize, or restyle it after construction, or replace it via
    /// <see cref="World.Remove"/> + <see cref="World.Add(IAddable)"/> if they want a fully
    /// custom visual.
    /// </summary>
    public ProgressBar Bar => _bar;

    /// <summary>Current progress in [0, 1] — updated as each step completes.</summary>
    public float Progress => _progress.Value;

    /// <summary>The world's lifecycle state — see <see cref="LoadingScreenState"/>.</summary>
    public LoadingScreenState State => _state;

    /// <summary>
    /// The name of the step currently executing on the worker thread, or null between steps or
    /// before loading starts. Subclasses can poll this in <see cref="Update"/> to mirror it onto
    /// a label below the progress bar.
    /// </summary>
    public string? CurrentStepName => _currentStepName.Value;

    /// <summary>
    /// Raised on the main thread when loading completes successfully, immediately before the
    /// world is switched. Use this hook for any main-thread, post-load work that must happen
    /// before <see cref="NextWorld"/> is invoked.
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

        // Snapshot the step list once on the main thread. The lambdas inside the steps are
        // constructed here but invoked on the worker.
        IReadOnlyList<LoadStep> steps = AssetsToLoad ?? [];
        CancellationToken token = _cancellation.Token;

        // Create the shared GL context here on the engine thread (NativeWindow creation is
        // main-thread-only on Windows). The worker thread then claims it via MakeCurrent.
        // The whole loading sequence runs on one Task.Run thread pool thread — no awaits
        // means no continuation thread switches, so the GL context stays current throughout.
        _workerGLContext = EngineConfiguration.GLLoadingContextFactory.CreateSharedContext();
        ILoadingGLContext glCtx = _workerGLContext;

        _loadTask = Task.Run(() =>
        {
            try
            {
                glCtx.MakeCurrentOnThisThread();

                for (int i = 0; i < steps.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    LoadStep step = steps[i];
                    _currentStepName.Value = step.Name;
                    step.Work();
                    _progress.Report((float)(i + 1) / steps.Count);
                }

                _currentStepName.Value = null;

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

    /// <inheritdoc/>
    /// <remarks>Cancels the background load task (the token passed to step work fires here) before tearing the world down.</remarks>
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
    /// Mirrors the latest reported progress onto <see cref="Bar"/>, then — if the background
    /// task has finished — raises <see cref="LoadCompleted"/> and triggers the world switch via
    /// <see cref="NextWorld"/>. If any step's work threw, the exception is re-thrown here on the
    /// main thread (propagating to <c>GameLauncher.Run</c>'s fatal handler).
    /// </remarks>
    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        _bar.AmountFilled = _progress.Value;

        if (_handoffDone || _loadTask is null || !_loadTask.IsCompleted)
        {
            return;
        }

        if (_loadTask.IsFaulted)
        {
            // Re-throw the worker's exception on the main thread so it propagates up to the
            // engine's fatal-error handler. Dispose the GL context first so the GLFW window
            // isn't leaked, but otherwise let the exception bubble — the framework deliberately
            // doesn't try to recover, because it can't tell programmer errors apart from
            // recoverable I/O failures. Subclasses that want recovery should try/catch inside
            // the failing step's delegate.
            _handoffDone = true;
            DisposeWorkerGLContext();
            Exception inner = UnwrapAggregate(_loadTask.Exception) ?? new InvalidOperationException("LoadingScreenWorld load task faulted with no exception attached.");
            throw inner;
        }

        if (_loadTask.IsCanceled)
        {
            // The world was unloaded while loading — nothing more to do.
            _handoffDone = true;
            DisposeWorkerGLContext();
            return;
        }

        // Wait on the worker's end-of-stream fence so all its commands are flushed to the GPU
        // before we proceed. Fences are shareable across shared contexts.
        long fence = Interlocked.Read(ref _workerFence);
        if (fence != 0)
        {
            OpenTK.Graphics.OpenGL4.GL.ClientWaitSync(
                new IntPtr(fence),
                OpenTK.Graphics.OpenGL4.ClientWaitSyncFlags.SyncFlushCommandsBit,
                timeout: 5_000_000_000L);
            OpenTK.Graphics.OpenGL4.GL.DeleteSync(new IntPtr(fence));
        }

        // Invalidate the bind cache so the next render-path BindTexture call definitely hits
        // glBindTexture in the main context — that re-bind from a different context is what
        // tells the driver to refresh its cached view of the shared texture.
        OpenGLHelper.ResetCache();

        DisposeWorkerGLContext();

        // Snap to full and complete.
        _progress.Report(1f);
        _bar.AmountFilled = 1f;
        _state = LoadingScreenState.Completed;
        _handoffDone = true;

        LoadCompleted?.Invoke();

        IWorldSwitcher switcher = _worldSwitcherOverride ?? EngineConfiguration.WorldSwitcher;
        switcher.RequestWorldChange(NextWorld);
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
    /// </summary>
    private sealed class AtomicFloatProgress : IProgress<float>
    {
        private int _bits = 0;

        public float Value => BitConverter.Int32BitsToSingle(Volatile.Read(ref _bits));

        public void Report(float value) => Volatile.Write(ref _bits, BitConverter.SingleToInt32Bits(value));
    }

    /// <summary>Thread-safe holder for the currently-executing step's name.</summary>
    private sealed class AtomicStepName
    {
        private string? _value = null;

        public string? Value
        {
            get => Volatile.Read(ref _value);
            set => Volatile.Write(ref _value, value);
        }
    }
}

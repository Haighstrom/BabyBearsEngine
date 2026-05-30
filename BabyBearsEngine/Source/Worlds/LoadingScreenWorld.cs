using System;
using System.Threading;
using System.Threading.Tasks;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Abstract base for a world that runs <see cref="LoadAssets"/> on a background thread with a
/// shared OpenGL context current — so the load body can call texture/shader/buffer constructors
/// directly. The base handles the threading, the shared-context setup, the cross-context GL
/// synchronisation when loading finishes, and re-throwing any exception as fatal on the main
/// thread. That is all it does.
///
/// <para>The base deliberately does NOT include any UI. Subclasses are responsible for whatever
/// they want to show during loading — a progress bar, an hourglass animation, a list of message
/// lines, anything — by adding their own children. Subclasses also decide what to do when
/// loading completes (switch worlds, wait for a "press any key" prompt, fade out, etc.) by
/// overriding <see cref="OnLoadCompleted"/>.</para>
///
/// <example><code>
/// internal sealed class GameLoadingWorld : LoadingScreenWorld
/// {
///     private readonly Func&lt;World&gt; _next;
///     private ITexture _bear = null!;
///
///     public GameLoadingWorld(Func&lt;World&gt; next)
///     {
///         _next = next;
///         Add(new ProgressBar(...));  // or any other UI
///     }
///
///     // Runs on the worker thread; GL constructors work directly here.
///     protected override void LoadAssets()
///     {
///         _bear = Textures.CreateFromFile("Assets/bear.png");
///     }
///
///     // Runs on the main thread once loading is finished.
///     protected override void OnLoadCompleted()
///     {
///         Engine.ChangeWorld(() =&gt; new GameWorld(_bear));
///     }
/// }
/// </code></example>
///
/// <para>Threading note: any state mutated by <see cref="LoadAssets"/> on the worker that the
/// main thread also reads (typically the subclass's progress counters and UI mirror strings)
/// must be made thread-safe by the subclass — e.g. via <c>Volatile</c>/<c>Interlocked</c> or a
/// lock. The base intentionally takes no opinion on how progress is tracked or displayed.</para>
/// </summary>
public abstract class LoadingScreenWorld : World
{
    private readonly CancellationTokenSource _cancellation = new();
    private Task? _loadTask;
    // Sync object created by the worker thread at the end of LoadAssets and waited on by the
    // main thread before OnLoadCompleted fires. Fences are shareable across shared contexts,
    // and the main-thread wait gives the driver an opportunity to make worker-side texture
    // uploads visible to this context.
    private long _workerFence = 0;
    // The shared GL context whose lifetime spans the worker task. Created on the main thread
    // (window creation is main-thread-only on Windows), used on the worker, then disposed on
    // the main thread (GLFW windows can only be disposed on the main thread).
    private ILoadingGLContext? _workerGLContext = null;
    private bool _completedHandled = false;

    /// <summary>
    /// True once <see cref="LoadAssets"/> has finished and the cross-context GL sync has run.
    /// The framework also calls <see cref="OnLoadCompleted"/> at the moment this flips from
    /// false to true — most subclasses won't need to poll this directly.
    /// </summary>
    public bool IsLoadComplete { get; private set; } = false;

    /// <summary>
    /// True after this world is unloaded. Check inside long-running loops in
    /// <see cref="LoadAssets"/> to stop early so the worker doesn't keep running after the
    /// world is gone.
    /// </summary>
    protected bool LoadingCancelled => _cancellation.IsCancellationRequested;

    /// <summary>
    /// Override to do the loading work. Runs on a background thread with a shared OpenGL
    /// context current — call texture/shader/buffer constructors directly. Throw to fail the
    /// load — the exception will be re-thrown as fatal on the main thread.
    /// </summary>
    protected abstract void LoadAssets();

    /// <summary>
    /// Called on the main thread once <see cref="LoadAssets"/> has completed successfully and
    /// any GL state has been synchronised. Default does nothing — override to switch worlds
    /// (<c>Engine.ChangeWorld(...)</c>), show a "press any key" prompt, or whatever the loading
    /// screen's exit behaviour should be.
    /// </summary>
    protected virtual void OnLoadCompleted() { }

    /// <inheritdoc/>
    /// <remarks>Spawns the worker task that runs <see cref="LoadAssets"/>. Idempotent.</remarks>
    public override void Load()
    {
        base.Load();

        if (_loadTask is not null)
        {
            return;
        }

        // Create the shared GL context here on the engine thread (NativeWindow creation is
        // main-thread-only on Windows). The worker thread then claims it via MakeCurrent.
        // The whole load runs on one Task.Run thread pool thread — no awaits means no
        // continuation thread switches, so the GL context stays current throughout.
        _workerGLContext = EngineConfiguration.GLLoadingContextFactory.CreateSharedContext();
        ILoadingGLContext glCtx = _workerGLContext;

        _loadTask = Task.Run(() =>
        {
            try
            {
                glCtx.MakeCurrentOnThisThread();

                LoadAssets();

                // Insert a fence at the end of the worker's command stream. The main thread
                // will wait on this fence before flipping IsLoadComplete — without that wait
                // the driver can leave shared textures in a not-yet-visible state and the
                // first main-thread sample returns zeros (a black frame).
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
        });
    }

    /// <inheritdoc/>
    /// <remarks>Signals cancellation so long-running <see cref="LoadAssets"/> loops can bail out via <see cref="LoadingCancelled"/>.</remarks>
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
    /// Polls the worker task. When it completes successfully, runs the cross-context GL sync,
    /// disposes the worker window (main-thread-only), then flips <see cref="IsLoadComplete"/>
    /// to true and invokes <see cref="OnLoadCompleted"/>. If <see cref="LoadAssets"/> threw,
    /// the exception is re-thrown here so it propagates to <c>GameLauncher.Run</c>'s fatal
    /// handler.
    /// </remarks>
    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        if (_completedHandled || _loadTask is null || !_loadTask.IsCompleted)
        {
            return;
        }

        if (_loadTask.IsFaulted)
        {
            _completedHandled = true;
            DisposeWorkerGLContext();
            Exception inner = UnwrapAggregate(_loadTask.Exception) ?? new InvalidOperationException("LoadingScreenWorld load task faulted with no exception attached.");
            throw inner;
        }

        if (_loadTask.IsCanceled)
        {
            _completedHandled = true;
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

        _completedHandled = true;
        IsLoadComplete = true;

        OnLoadCompleted();
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
}

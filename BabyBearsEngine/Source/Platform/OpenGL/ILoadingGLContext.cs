namespace BabyBearsEngine.OpenGL;

/// <summary>
/// A short-lived OpenGL context that shares resources (textures, buffers, programs) with the
/// engine's main context, so a background thread can construct GL resources that the main
/// thread then sees and renders. Produced by <see cref="IGLLoadingContextFactory"/>; consumed
/// by <see cref="Worlds.LoadingScreenWorld"/>'s sync overload.
///
/// Lifecycle is two-phase because the underlying GLFW window can only be disposed on the main
/// thread, but the GL work happens on a worker:
///   1. Main thread: <see cref="IGLLoadingContextFactory.CreateSharedContext"/>.
///   2. Worker thread: <see cref="MakeCurrentOnThisThread"/>, do GL work, then <see cref="ReleaseFromWorkerThread"/>.
///   3. Main thread: <see cref="IDisposable.Dispose"/> to tear down the underlying window.
/// </summary>
internal interface ILoadingGLContext : IDisposable
{
    /// <summary>
    /// Makes this GL context current on the calling thread and registers the thread as a
    /// valid GL thread (so <see cref="GLThread.Ensure"/> guards in resource constructors
    /// don't fire). Call exactly once on the worker thread that will do the GL work, before
    /// any GL resource constructors.
    /// </summary>
    void MakeCurrentOnThisThread();

    /// <summary>
    /// Releases the context from the worker thread and unregisters the thread. Must be called
    /// from the same worker thread that called <see cref="MakeCurrentOnThisThread"/>. Does NOT
    /// destroy the underlying window — call <see cref="IDisposable.Dispose"/> on the main
    /// thread afterwards.
    /// </summary>
    void ReleaseFromWorkerThread();
}

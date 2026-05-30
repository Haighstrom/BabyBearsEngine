namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Factory for shared GL contexts used by background loading. Registered by the platform
/// adapter (OpenTK) into <see cref="EngineConfiguration"/>; consumed internally by
/// <see cref="Worlds.LoadingScreenWorld"/>'s sync overload.
/// </summary>
internal interface IGLLoadingContextFactory
{
    /// <summary>
    /// Creates a fresh GL context that shares resources with the main engine context.
    /// Must be called on the engine main thread (window creation is main-thread-only on
    /// Windows). The returned <see cref="ILoadingGLContext"/> can then be made current on
    /// any single worker thread for the duration of background GL work.
    /// </summary>
    ILoadingGLContext CreateSharedContext();
}

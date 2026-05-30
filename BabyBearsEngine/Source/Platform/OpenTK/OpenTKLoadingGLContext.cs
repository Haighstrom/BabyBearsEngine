using BabyBearsEngine.OpenGL;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

/// <summary>
/// <see cref="ILoadingGLContext"/> backed by an invisible OpenTK <see cref="NativeWindow"/>
/// whose context was created with <see cref="NativeWindowSettings.SharedContext"/> pointing at
/// the engine's main context. Resources created via this context are shared with the main
/// context, so a TextureGraphic constructed on the loading thread is drawable from the main
/// thread once loading completes.
/// </summary>
internal sealed class OpenTKLoadingGLContext(NativeWindow window) : ILoadingGLContext
{
    private bool _disposed = false;

    public void MakeCurrentOnThisThread()
    {
        window.Context.MakeCurrent();
        GLThread.Register();
    }

    public void ReleaseFromWorkerThread()
    {
        GLThread.Unregister();
        // Releasing the context can throw if it was never made current on this thread (e.g. the
        // worker bailed out before MakeCurrentOnThisThread); swallow that, the window dispose
        // on the main thread will still clean up.
        try
        {
            window.Context.MakeNoneCurrent();
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // GLFW windows can only be disposed on the main thread. This Dispose must be called
        // from the engine main thread; the worker thread should have already called
        // ReleaseFromWorkerThread to give up the context.
        window.Dispose();
        _disposed = true;
    }
}

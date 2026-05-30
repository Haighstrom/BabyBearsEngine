using BabyBearsEngine.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

/// <summary>
/// OpenTK implementation of <see cref="IGLLoadingContextFactory"/>. Creates an invisible
/// 1×1 <see cref="NativeWindow"/> with a context that shares resources with the engine's
/// main context. The new context's API/version/profile must match the main context exactly,
/// otherwise GLFW will create a context that doesn't actually share resources even though
/// the share parameter was passed. The new context is released from the engine thread so the
/// worker thread can claim it.
/// </summary>
internal sealed class OpenTKGLLoadingContextFactory(GameWindow mainWindow) : IGLLoadingContextFactory
{
    public ILoadingGLContext CreateSharedContext()
    {
        GLThread.Ensure();

        var settings = new NativeWindowSettings
        {
            StartVisible = false,
            ClientSize = new Vector2i(1, 1),
            SharedContext = mainWindow.Context,
            API = mainWindow.API,
            APIVersion = mainWindow.APIVersion,
            Profile = mainWindow.Profile,
            Flags = mainWindow.Flags,
        };

        var window = new NativeWindow(settings);

        // Release the new context from the engine thread so the worker thread can MakeCurrent
        // it without OpenGL complaining that it's owned by another thread. Restore the main
        // context as current on this thread (NativeWindow ctor leaves its own context current).
        window.Context.MakeNoneCurrent();
        mainWindow.MakeCurrent();

        return new OpenTKLoadingGLContext(window);
    }
}

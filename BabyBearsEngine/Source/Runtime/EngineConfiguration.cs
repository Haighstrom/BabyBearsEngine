using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine;

// Process-wide service registry. Setters and Reset() exist as a test seam — production code
// initialises this once via GameLauncher; tests can substitute individual services and must
// call Reset() in teardown to avoid state bleeding between cases.
internal static class EngineConfiguration
{
    private const string AlreadyInitialisedMessage = "Game services already initialised.";
    private const string NotInitialisedMessage = "The platform context has not been initialized. Please call Initialise() before accessing the platform context.";
    private const string ScreenCaptureNotEnabledMessage = "Screen capture is not enabled. Set ApplicationSettings.DiagnosticsSettings.CaptureFrames = true to enable.";

    private static IGPUResourceDeletionService s_gpuResourceDeletionService = new DefaultGPUResourceDeletionService();
    private static IKeyboard? s_keyboard = null;
    private static IMouse? s_mouse = null;
    private static IScreenCapture? s_screenCapture = null;
    private static ITextureFactory s_textureFactory = new DefaultTextureFactory();
    private static IWindow? s_window = null;
    private static IWorldSwitcher? s_worldSwitcher = null;

    public static IGPUResourceDeletionService GPUResourceDeletionService
    {
        get => s_gpuResourceDeletionService;
        set => s_gpuResourceDeletionService = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IKeyboard KeyboardService
    {
        get => s_keyboard ?? throw new InvalidOperationException(NotInitialisedMessage);
        set => s_keyboard = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IMouse MouseService
    {
        get => s_mouse ?? throw new InvalidOperationException(NotInitialisedMessage);
        set => s_mouse = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IScreenCapture ScreenCaptureService
    {
        get => s_screenCapture ?? throw new InvalidOperationException(ScreenCaptureNotEnabledMessage);
        set => s_screenCapture = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static ITextureFactory TextureFactory
    {
        get => s_textureFactory;
        set => s_textureFactory = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IWindow WindowService
    {
        get => s_window ?? throw new InvalidOperationException(NotInitialisedMessage);
        set => s_window = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IWorldSwitcher WorldSwitcher
    {
        get => s_worldSwitcher ?? throw new InvalidOperationException(NotInitialisedMessage);
        set => s_worldSwitcher = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static void Initialise(IWindow window, IKeyboard keyboard, IMouse mouse, IWorldSwitcher worldSwitcher, IScreenCapture? screenCapture)
    {
        ArgumentNullException.ThrowIfNull(window, nameof(window));
        ArgumentNullException.ThrowIfNull(keyboard, nameof(keyboard));
        ArgumentNullException.ThrowIfNull(mouse, nameof(mouse));
        ArgumentNullException.ThrowIfNull(worldSwitcher, nameof(worldSwitcher));

        if (s_window is not null || s_keyboard is not null || s_mouse is not null || s_worldSwitcher is not null || s_screenCapture is not null)
        {
            throw new InvalidOperationException(AlreadyInitialisedMessage);
        }

        s_window = window;
        s_keyboard = keyboard;
        s_mouse = mouse;
        s_worldSwitcher = worldSwitcher;
        s_screenCapture = screenCapture;
    }

    public static void Reset()
    {
        s_gpuResourceDeletionService = new DefaultGPUResourceDeletionService();
        s_keyboard = null;
        s_mouse = null;
        s_screenCapture = null;
        s_textureFactory = new DefaultTextureFactory();
        s_window = null;
        s_worldSwitcher = null;
    }
}

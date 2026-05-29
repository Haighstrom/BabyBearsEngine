using BabyBearsEngine.AudioSystem;
using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine;

// Process-wide service registry. Setters and Reset() exist as a test seam — production code
// initialises this once via GameLauncher; tests can substitute individual services and must
// call Reset() in teardown to avoid state bleeding between cases.
internal static class EngineConfiguration
{
    private const string AlreadyInitialisedMessage = "Game services already initialised.";
    private const string NotInitialisedMessage = "The platform context has not been initialized. Please call Initialise() before accessing the platform context.";
    private const string ScreenCaptureNotEnabledMessage = "Screen capture is not enabled. Set ApplicationSettings.DiagnosticsSettings.CaptureFrames = true to enable.";

    // One atlas generator per backend, so a FontDefinition can pin either built-in (or a custom
    // one) independently of the engine-wide default. Resolved per font in FontTextureCache.
    private static readonly Dictionary<TextRenderer, IFontAtlasGenerator> s_atlasGenerators = [];
    private static TextRenderer s_defaultTextRenderer = TextRenderer.Gdi;

    private static IAudio? s_audio = null;
    private static IEngineInfo? s_engineInfo = null;
    private static IGPUResourceDeletionService? s_gpuResourceDeletionService = null;
    private static IKeyboard? s_keyboard = null;
    private static IMouse? s_mouse = null;
    private static IScreenCapture? s_screenCapture = null;
    private static ITextureFactory? s_textureFactory = null;
    private static IWindow? s_window = null;
    private static IWorldSwitcher? s_worldSwitcher = null;

    /// <summary>
    /// Default MSAA sample count used by <see cref="Worlds.Camera"/> and <see cref="Worlds.UICamera"/> when no
    /// explicit sample count is provided. Set by <see cref="GameLauncher"/> from
    /// <see cref="ApplicationSettings.DefaultCameraMsaa"/> before the engine starts.
    /// </summary>
    public static MsaaSamples DefaultCameraMsaa { get; set; } = MsaaSamples.Disabled;

    /// <summary>
    /// The backend used to build atlases for fonts that don't pin one via
    /// <see cref="Worlds.Graphics.Text.FontDefinition.Renderer"/>. Set by <see cref="GameLauncher"/>
    /// from <see cref="ApplicationSettings.TextSettings"/>. Changing it drops cached atlases that
    /// resolved through the previous default.
    /// </summary>
    public static TextRenderer DefaultTextRenderer
    {
        get => s_defaultTextRenderer;
        set
        {
            s_defaultTextRenderer = value;
            FontTextureCache.InvalidateCache();
        }
    }

    /// <summary>
    /// The atlas generator registered under the current <see cref="DefaultTextRenderer"/>. The
    /// setter is a test seam: it replaces the default backend's generator and clears the cache.
    /// Production code registers generators via <see cref="RegisterAtlasGenerator"/> during
    /// <see cref="Initialise"/>.
    /// </summary>
    public static IFontAtlasGenerator AtlasGenerator
    {
        get => GetAtlasGenerator(s_defaultTextRenderer);
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            RegisterAtlasGenerator(s_defaultTextRenderer, value);
        }
    }

    public static IAudio AudioService
    {
        get => s_audio ?? throw new InvalidOperationException(NotInitialisedMessage);
        set => s_audio = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IEngineInfo EngineInfo
    {
        get => s_engineInfo ?? throw new InvalidOperationException(NotInitialisedMessage);
        set => s_engineInfo = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IGPUResourceDeletionService GPUResourceDeletionService
    {
        get => s_gpuResourceDeletionService ?? throw new InvalidOperationException(NotInitialisedMessage);
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
        get => s_textureFactory ?? throw new InvalidOperationException(NotInitialisedMessage);
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

    /// <summary>
    /// Returns the atlas generator registered for <paramref name="renderer"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no generator is registered for the
    /// requested backend — i.e. the engine has not been initialised.</exception>
    public static IFontAtlasGenerator GetAtlasGenerator(TextRenderer renderer)
    {
        if (s_atlasGenerators.TryGetValue(renderer, out IFontAtlasGenerator? generator))
        {
            return generator;
        }

        throw new InvalidOperationException(NotInitialisedMessage);
    }

    public static void Initialise(
        IWindow window,
        IKeyboard keyboard,
        IMouse mouse,
        IWorldSwitcher worldSwitcher,
        IEngineInfo engineInfo,
        IAudio audio,
        IScreenCapture? screenCapture,
        IGPUResourceDeletionService? gpuResourceDeletion = null,
        ITextureFactory? textureFactory = null,
        IFontAtlasGenerator? atlasGenerator = null)
    {
        ArgumentNullException.ThrowIfNull(window, nameof(window));
        ArgumentNullException.ThrowIfNull(keyboard, nameof(keyboard));
        ArgumentNullException.ThrowIfNull(mouse, nameof(mouse));
        ArgumentNullException.ThrowIfNull(worldSwitcher, nameof(worldSwitcher));
        ArgumentNullException.ThrowIfNull(engineInfo, nameof(engineInfo));
        ArgumentNullException.ThrowIfNull(audio, nameof(audio));

        if (s_window is not null
            || s_keyboard is not null
            || s_mouse is not null
            || s_worldSwitcher is not null
            || s_engineInfo is not null
            || s_audio is not null
            || s_screenCapture is not null
            || s_gpuResourceDeletionService is not null
            || s_textureFactory is not null
            || s_atlasGenerators.Count > 0)
        {
            throw new InvalidOperationException(AlreadyInitialisedMessage);
        }

        s_window = window;
        s_keyboard = keyboard;
        s_mouse = mouse;
        s_worldSwitcher = worldSwitcher;
        s_engineInfo = engineInfo;
        s_audio = audio;
        s_screenCapture = screenCapture;
        s_gpuResourceDeletionService = gpuResourceDeletion ?? new DefaultGPUResourceDeletionService();
        s_textureFactory = textureFactory ?? new DefaultTextureFactory();

        // Register every built-in backend so any FontDefinition can pin one regardless of the
        // chosen default. A caller-supplied generator overrides the built-in for the default
        // backend (a test/customisation seam).
        s_atlasGenerators[TextRenderer.Gdi] = new GdiFontAtlasGenerator();
        s_atlasGenerators[TextRenderer.Sdf] = new SdfFontAtlasGenerator();
        s_atlasGenerators[TextRenderer.FreeType] = new FreeTypeFontAtlasGenerator();

        if (atlasGenerator is not null)
        {
            s_atlasGenerators[s_defaultTextRenderer] = atlasGenerator;
        }
    }

    /// <summary>
    /// Registers (or replaces) the atlas generator for a backend and drops cached atlases, since
    /// they reference the previous generator's GL texture and shader.
    /// </summary>
    public static void RegisterAtlasGenerator(TextRenderer renderer, IFontAtlasGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        s_atlasGenerators[renderer] = generator;
        FontTextureCache.InvalidateCache();
    }

    public static void Reset()
    {
        DefaultCameraMsaa = MsaaSamples.Disabled;
        s_atlasGenerators.Clear();
        s_defaultTextRenderer = TextRenderer.Gdi;
        s_audio = null;
        s_engineInfo = null;
        s_gpuResourceDeletionService = null;
        s_keyboard = null;
        s_mouse = null;
        s_screenCapture = null;
        s_textureFactory = null;
        s_window = null;
        s_worldSwitcher = null;
        GpuCapabilities.Reset();
        FontTextureCache.InvalidateCache();
    }
}

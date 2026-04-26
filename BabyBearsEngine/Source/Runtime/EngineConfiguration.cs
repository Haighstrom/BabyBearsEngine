using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Runtime;

public static class EngineConfiguration
{
    private const string NotInitialisedMessage = "The platform context has not been initialized. Please call Initialise() before accessing the platform context.";
    private const string AlreadyInitialisedMessage = "Game services already initialised.";

    private static ITextureFactory s_textureFactory = new DefaultTextureFactory();
    private static IGPUResourceDeletionService s_gpuResourceDeletionService = new DefaultGPUResourceDeletionService();
    private static IPlatformContext? s_backend;

    private static IPlatformContext Backend
    {
        get
        {
            if (s_backend == null)
            {
                throw new InvalidOperationException(NotInitialisedMessage);
            }
            return s_backend;
        }
    }

    public static void Initialise(IPlatformContext platformContext)
    {
        ArgumentNullException.ThrowIfNull(platformContext, nameof(platformContext));

        if (s_backend is not null)
        {
            throw new InvalidOperationException(AlreadyInitialisedMessage);
        }

        s_backend = platformContext;
    }

    public static ITextureFactory TextureFactory
    {
        get => s_textureFactory;
        set => s_textureFactory = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IGPUResourceDeletionService GPUResourceDeletionService
    {
        get => s_gpuResourceDeletionService;
        set => s_gpuResourceDeletionService = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IWindow WindowService => Backend.Window;

    public static IKeyboard KeyboardService => Backend.Keyboard;

    public static IMouse MouseService => Backend.Mouse;

    public static IWorldSwitcher WorldSwitcher => Backend.WorldSwitcher;
}

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Runtime;

//Static Service Locator
internal static class RuntimeServices
{
    private const string NotInitialisedMessage = "Game services have not been initialised.";
    private const string AlreadyInitialisedMessage = "Game services already initialised.";

    private static IPlatformContext? s_backend;

    private static IPlatformContext Backend => Volatile.Read(ref s_backend) ?? throw new InvalidOperationException(NotInitialisedMessage);

    public static void Initialise(IPlatformContext service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (Interlocked.CompareExchange(ref s_backend, service, null) is not null)
        {
            throw new InvalidOperationException(AlreadyInitialisedMessage);
        }
    }

    public static IWindow WindowService => Backend.Window;

    public static IKeyboard KeyboardService => Backend.Keyboard;

    public static IMouse MouseService => Backend.Mouse;

    public static bool TryGetGPUResourceDeletionService([NotNullWhen(true)] out IGPUResourceDeletionService? gpuResourceDeletionService)
    {
        var backend = Volatile.Read(ref s_backend);

        if (backend is null)
        {
            gpuResourceDeletionService = null;
            return false;
        }

        gpuResourceDeletionService = backend.GPUResourceDeletionService;
        return true;
    }
}

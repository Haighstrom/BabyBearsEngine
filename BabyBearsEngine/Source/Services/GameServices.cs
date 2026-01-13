using System.Diagnostics.CodeAnalysis;
using System.Threading;
using BabyBearsEngine.Source.Graphics.MemoryManagement;

namespace BabyBearsEngine.Source.Services;

internal static class GameServices
{
    private const string NotInitialisedMessage = "Game services have not been initialised.";
    private const string AlreadyInitialisedMessage = "Game services already initialised.";

    private static IGameServices? s_backend;

    private static IGameServices Backend => Volatile.Read(ref s_backend) ?? throw new InvalidOperationException(NotInitialisedMessage);

    public static void Initialise(IGameServices service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (Interlocked.CompareExchange(ref s_backend, service, null) is not null)
        {
            throw new InvalidOperationException(AlreadyInitialisedMessage);
        }
    }

    public static IWindowService WindowService => Backend.WindowService;

    public static IKeyboardService KeyboardService => Backend.KeyboardService;

    public static IMouseService MouseService => Backend.MouseService;

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

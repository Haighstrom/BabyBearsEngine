using BabyBearsEngine.Source.Graphics.MemoryManagement;

namespace BabyBearsEngine.Source.Services;

internal static class GameServices
{
    private static IGameServices? s_service;

    private static IGameServices Services => s_service ?? throw new InvalidOperationException("Game services have not been initialised.");

    public static void Initialise(IGameServices services)
    {
        if (s_service is not null)
        {
            throw new InvalidOperationException("Game services already initialised.");
        }

        s_service = services;
    }

    public static IWindowService WindowService => Services.WindowService;
    public static IKeyboardService KeyboardService => Services.KeyboardService;
    public static IMouseService MouseService => Services.MouseService;
    public static IGPUMemoryService GPUMemoryService => Services.GPUMemoryService;
}

using BabyBearsEngine.OpenGL;
using BabyBearsEngine.PowerUsers;

namespace BabyBearsEngine.Source.Runtime;

public static class EngineConfiguration
{
    private static ITextureFactory s_textureFactory = new DefaultTextureFactory();
    private static IWorldSwitcher s_worldSwitcher = new DefaultWorldSwitcher();

    public static ITextureFactory TextureFactory
    {
        get => s_textureFactory;
        set => s_textureFactory = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IWorldSwitcher WorldSwitcher
    {
        get => s_worldSwitcher;
        set => s_worldSwitcher = value ?? throw new ArgumentNullException(nameof(value));
    }
}

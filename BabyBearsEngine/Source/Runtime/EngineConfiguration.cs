using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Source.Runtime;

public static class EngineConfiguration
{
    private static ITextureFactory s_textureFactory = new DefaultTextureFactory();

    public static ITextureFactory TextureFactory
    {
        get => s_textureFactory;
        set => s_textureFactory = value ?? throw new ArgumentNullException(nameof(value));
    }
}

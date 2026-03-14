using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Runtime;

namespace BabyBearsEngine.Worlds.Graphics;

public static class Textures
{
    private static ITextureFactory Implementation => EngineConfiguration.TextureFactory;

    public static ITexture FromImageFile(string filePath) => Implementation.CreateTextureFromImageFile(filePath);
}

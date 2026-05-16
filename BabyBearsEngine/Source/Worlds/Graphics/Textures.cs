using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics;

public static class Textures
{
    private static ITextureFactory Implementation => EngineConfiguration.TextureFactory;

    public static ITexture CreateFromFile(string filePath) => Implementation.CreateTextureFromImageFile(filePath);

    public static ITexture GenTexture(Colour[,] pixels) => Implementation.GenTexture(pixels);
}

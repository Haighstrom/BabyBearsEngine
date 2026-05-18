using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics;

public static class Textures
{
    private static ITextureFactory Implementation => EngineConfiguration.TextureFactory;

    /// <summary>Loads an image file as a single <see cref="ITexture"/>.</summary>
    public static ITexture CreateFromFile(string filePath) => Implementation.CreateTextureFromImageFile(filePath);

    /// <summary>
    /// Loads an image file as an <see cref="ISpriteTexture"/> split into a regular grid of frames.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <param name="columns">Number of frame columns in the sprite sheet.</param>
    /// <param name="rows">Number of frame rows in the sprite sheet.</param>
    public static ISpriteTexture CreateSpriteFromFile(string filePath, int columns, int rows) =>
        Implementation.CreateSpriteTextureFromImageFile(filePath, columns, rows);

    /// <summary>
    /// Generates a rectangular texture with a solid <paramref name="fill"/> and a border of
    /// <paramref name="borderThickness"/> pixels in <paramref name="border"/>.
    /// </summary>
    public static ITexture GenBorderedRectangle(int width, int height, int borderThickness, Colour fill, Colour border) =>
        Implementation.GenBorderedRectangle(width, height, borderThickness, fill, border);

    /// <summary>Generates a texture from a 2D array of pixel colours.</summary>
    public static ITexture GenTexture(Colour[,] pixels) => Implementation.GenTexture(pixels);
}

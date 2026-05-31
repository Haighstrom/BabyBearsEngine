using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics;

public static class Textures
{
    private static ITextureFactory Implementation => EngineConfiguration.TextureFactory;

    /// <summary>Loads an image file as a single <see cref="ITexture"/>.</summary>
    /// <param name="linearFilter">True for bilinear filtering (smooth); false for nearest-neighbour (pixel-art sharp). Defaults to true.</param>
    public static ITexture CreateFromFile(string filePath, bool linearFilter = true) => Implementation.CreateTextureFromImageFile(filePath, linearFilter);

    /// <summary>
    /// Loads an image file as an <see cref="ISpriteTexture"/> split into a regular grid of frames.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <param name="rows">Number of frame rows in the sprite sheet.</param>
    /// <param name="columns">Number of frame columns in the sprite sheet.</param>
    /// <param name="linearFilter">True for bilinear filtering (smooth); false for nearest-neighbour (pixel-art sharp). Defaults to false.</param>
    public static ISpriteTexture CreateSpriteFromFile(string filePath, int rows, int columns, bool linearFilter = false) =>
        Implementation.CreateSpriteTextureFromImageFile(filePath, rows, columns, linearFilter);

    /// <summary>
    /// Creates a rectangular texture with a solid <paramref name="fill"/> and a border of
    /// <paramref name="borderThickness"/> pixels in <paramref name="border"/>.
    /// </summary>
    public static ITexture CreateBorderedRectangle(int width, int height, int borderThickness, Colour fill, Colour border) =>
        Implementation.CreateBorderedRectangle(width, height, borderThickness, fill, border);

    /// <summary>Creates a texture from a 2D array of pixel colours.</summary>
    public static ITexture CreateTexture(Colour[,] pixels, bool linearFilter = true) =>
        Implementation.CreateTexture(pixels, linearFilter);

    /// <summary>
    /// Creates a texture from a tightly-packed RGBA8 byte buffer (4 bytes per pixel, row-major).
    /// The buffer is premultiplied internally before upload; the caller's array is not modified.
    /// </summary>
    public static ITexture CreateTexture(byte[] rgbaData, int width, int height, bool linearFilter = true) =>
        Implementation.CreateTexture(rgbaData, width, height, linearFilter);

    /// <summary>
    /// Creates a single-channel (R8) texture from a tightly-packed byte buffer (1 byte per
    /// texel, row-major). Used for coverage and SDF atlases that store one value per texel;
    /// no premultiplication is applied since R8 has no alpha channel.
    /// </summary>
    public static ITexture CreateR8Texture(byte[] r8Data, int width, int height, bool linearFilter = true) =>
        Implementation.CreateR8Texture(r8Data, width, height, linearFilter);
}

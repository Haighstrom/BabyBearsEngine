namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Factory for creating GPU textures from various sources — image files, in-memory pixel
/// buffers, or procedurally generated colours. All multi-channel paths premultiply alpha
/// before upload so the engine's alpha-blending shaders sample them without correction; the
/// R8 path has no alpha and is not premultiplied.
/// </summary>
public interface ITextureFactory
{
    /// <summary>
    /// Loads an image file from disk as a single texture. Format is decoded by the active
    /// image loader (PNG, JPEG, BMP, etc.). Alpha is premultiplied before upload, and mipmaps
    /// are generated when <paramref name="linearFilter"/> is true.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <param name="linearFilter">True for bilinear filtering with mipmaps (smooth); false for nearest-neighbour (pixel-art sharp). Defaults to true.</param>
    ITexture CreateTextureFromImageFile(string filePath, bool linearFilter = true);

    /// <summary>
    /// Loads an image file from disk as a sprite sheet split into a regular grid of frames.
    /// Each frame is padded with a one-pixel edge-mirror border to prevent linear-sampling
    /// bleed between adjacent frames. Alpha is premultiplied; mipmaps generated when
    /// <paramref name="linearFilter"/> is true.
    /// </summary>
    /// <param name="filePath">Path to the sprite sheet image file.</param>
    /// <param name="rows">Number of frame rows in the sheet.</param>
    /// <param name="columns">Number of frame columns in the sheet.</param>
    /// <param name="linearFilter">True for bilinear filtering with mipmaps; false for nearest-neighbour (pixel-art sharp). Defaults to false.</param>
    ISpriteTexture CreateSpriteTextureFromImageFile(string filePath, int rows, int columns, bool linearFilter = false);

    /// <summary>
    /// Creates a rectangular texture with a solid fill and a coloured border inside it.
    /// </summary>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="borderThickness">Border width in pixels (drawn inside the rectangle on all four sides).</param>
    /// <param name="fillColour">Colour of the interior region.</param>
    /// <param name="borderColour">Colour of the border ring.</param>
    ITexture CreateBorderedRectangle(int width, int height, int borderThickness, Colour fillColour, Colour borderColour);

    /// <summary>
    /// Creates a texture from a 2D array of pixel colours. Alpha is premultiplied before upload.
    /// </summary>
    /// <param name="pixels">Pixel data indexed as <c>pixels[x, y]</c>; width = <c>GetLength(0)</c>, height = <c>GetLength(1)</c>.</param>
    /// <param name="linearFilter">True for bilinear filtering (smooth); false for nearest-neighbour (pixel-art sharp). Defaults to true.</param>
    ITexture CreateTexture(Colour[,] pixels, bool linearFilter = true);

    /// <summary>
    /// Creates a texture from a tightly-packed RGBA8 byte buffer (4 bytes per pixel,
    /// row-major, no padding). Alpha is premultiplied internally before upload; the caller's
    /// array is not modified.
    /// </summary>
    /// <param name="rgbaData">RGBA8 pixel data, length = <c>width * height * 4</c>.</param>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="linearFilter">True for bilinear filtering (smooth); false for nearest-neighbour (pixel-art sharp). Defaults to true.</param>
    /// <exception cref="ArgumentException">If <paramref name="rgbaData"/>'s length is not exactly <c>width * height * 4</c>.</exception>
    ITexture CreateTexture(byte[] rgbaData, int width, int height, bool linearFilter = true);

    /// <summary>
    /// Creates a single-channel (R8) texture from a tightly-packed byte buffer (1 byte per
    /// texel, row-major, no padding). Used for coverage and SDF atlases that store one value
    /// per texel; no premultiplication is applied since R8 has no alpha channel.
    /// </summary>
    /// <param name="r8Data">Single-channel pixel data, length = <c>width * height</c>.</param>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="linearFilter">True for bilinear filtering (smooth); false for nearest-neighbour (sharp). Defaults to true — SDF reconstruction relies on bilinear sampling between texels.</param>
    /// <exception cref="ArgumentException">If <paramref name="r8Data"/>'s length is not exactly <c>width * height</c>.</exception>
    ITexture CreateR8Texture(byte[] r8Data, int width, int height, bool linearFilter = true);
}

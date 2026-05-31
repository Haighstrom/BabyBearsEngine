using BabyBearsEngine.Platform.ImageLoading;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

internal sealed class DefaultTextureFactory() : ITextureFactory
{
    private const int SpritePadding = 2;

    /// <inheritdoc />
    public ISpriteTexture CreateSpriteTextureFromImageFile(string filePath, int rows, int columns, bool linearFilter = false)
    {
        GLThread.Ensure();
        int handle = GL.GenTexture();
        OpenGLHelper.BindTexture(handle);

        Rgba8ImageData orig = ImageLoader.LoadAsRgba8(filePath);
        Rgba8Tools.PremultiplyAlphaInPlace(orig.Data);
        Rgba8ImageData padded = CreatePaddedSpriteSheet(orig, columns, rows, SpritePadding);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, padded.Width, padded.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, padded.Data);

        var minFilter = linearFilter ? TextureMinFilter.Linear : TextureMinFilter.Nearest;
        var magFilter = linearFilter ? TextureMagFilter.Linear : TextureMagFilter.Nearest;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        if (linearFilter)
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        Texture texture = new(handle, padded.Width, padded.Height);
        return new SpriteTexture(texture, columns, rows, SpritePadding);
    }

    /// <inheritdoc />
    public ITexture CreateTextureFromImageFile(string filePath, bool linearFilter = true)
    {
        GLThread.Ensure();
        int handle = GL.GenTexture();
        OpenGLHelper.BindTexture(handle);

        Rgba8ImageData imageData = ImageLoader.LoadAsRgba8(filePath);
        Rgba8Tools.PremultiplyAlphaInPlace(imageData.Data);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, imageData.Width, imageData.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageData.Data);

        var minFilter = linearFilter ? TextureMinFilter.Linear : TextureMinFilter.Nearest;
        var magFilter = linearFilter ? TextureMagFilter.Linear : TextureMagFilter.Nearest;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        if (linearFilter)
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        return new Texture(handle, imageData.Width, imageData.Height);
    }

    /// <inheritdoc />
    public ITexture CreateBorderedRectangle(int width, int height, int borderThickness, Colour fillColour, Colour borderColour)
    {
        var pixels = new Colour[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = x < borderThickness || x >= width - borderThickness
                             || y < borderThickness || y >= height - borderThickness
                    ? borderColour
                    : fillColour;
            }
        }

        return CreateTexture(pixels);
    }

    /// <inheritdoc />
    public ITexture CreateR8Texture(byte[] r8Data, int width, int height, bool linearFilter = true)
    {
        ArgumentNullException.ThrowIfNull(r8Data);
        int expectedLength = width * height;
        if (r8Data.Length != expectedLength)
        {
            throw new ArgumentException(
                $"r8Data length ({r8Data.Length}) does not match width*height ({expectedLength}).",
                nameof(r8Data));
        }

        GLThread.Ensure();
        Texture t = new(
            handle: GL.GenTexture(),
            width: width,
            height: height);

        OpenGLHelper.BindTexture(t.Handle);

        // R8 rows are one byte per texel, so they aren't 4-byte aligned; relax the unpack
        // alignment for the upload, then restore the default so other texture paths are unaffected.
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, r8Data);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

        var minFilter = linearFilter ? TextureMinFilter.Linear : TextureMinFilter.Nearest;
        var magFilter = linearFilter ? TextureMagFilter.Linear : TextureMagFilter.Nearest;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return t;
    }

    /// <inheritdoc />
    public ITexture CreateTexture(Colour[,] pixels, bool linearFilter = true)
    {
        int width = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        byte[] data = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Colour c = pixels[x, y];
                int offset = (y * width + x) * 4;
                data[offset] = c.R;
                data[offset + 1] = c.G;
                data[offset + 2] = c.B;
                data[offset + 3] = c.A;
            }
        }

        // We just built the buffer ourselves, so premultiply in place — no caller-visible mutation.
        Rgba8Tools.PremultiplyAlphaInPlace(data);
        return UploadRgbaPremultiplied(data, width, height, linearFilter);
    }

    /// <inheritdoc />
    public ITexture CreateTexture(byte[] rgbaData, int width, int height, bool linearFilter = true)
    {
        ArgumentNullException.ThrowIfNull(rgbaData);
        int expectedLength = width * height * 4;
        if (rgbaData.Length != expectedLength)
        {
            throw new ArgumentException(
                $"rgbaData length ({rgbaData.Length}) does not match width*height*4 ({expectedLength}).",
                nameof(rgbaData));
        }

        // Clone before premultiplying so the caller's buffer is not mutated.
        byte[] data = (byte[])rgbaData.Clone();
        Rgba8Tools.PremultiplyAlphaInPlace(data);
        return UploadRgbaPremultiplied(data, width, height, linearFilter);
    }

    private static ITexture UploadRgbaPremultiplied(byte[] data, int width, int height, bool linearFilter)
    {
        GLThread.Ensure();
        int handle = GL.GenTexture();
        OpenGLHelper.BindTexture(handle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

        var minFilter = linearFilter ? TextureMinFilter.Linear : TextureMinFilter.Nearest;
        var magFilter = linearFilter ? TextureMagFilter.Linear : TextureMagFilter.Nearest;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return new Texture(handle, width, height);
    }

    private static Rgba8ImageData CreatePaddedSpriteSheet(Rgba8ImageData orig, int columns, int rows, int padding)
    {
        int frameW = orig.Width / columns;
        int frameH = orig.Height / rows;
        int newW = orig.Width + (columns + 1) * padding;
        int newH = orig.Height + (rows + 1) * padding;
        byte[] newData = new byte[newW * newH * 4]; // all zeros = transparent

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                int srcX = col * frameW;
                int srcY = row * frameH;
                int dstX = padding + col * (frameW + padding);
                int dstY = padding + row * (frameH + padding);

                for (int y = 0; y < frameH; y++)
                {
                    for (int x = 0; x < frameW; x++)
                    {
                        CopyPixel(orig.Data, orig.Width, srcX + x, srcY + y, newData, newW, dstX + x, dstY + y);
                    }
                }

                // Mirror the outermost pixel row/column of each frame into the adjacent padding pixel.
                // This prevents a dark or transparent fringe at frame edges when GL_LINEAR filtering blends
                // into the padding region; any sample that overshoots by <1 texel gets the correct colour.
                for (int y = 0; y < frameH; y++)
                {
                    CopyPixel(orig.Data, orig.Width, srcX, srcY + y, newData, newW, dstX - 1, dstY + y);
                    CopyPixel(orig.Data, orig.Width, srcX + frameW - 1, srcY + y, newData, newW, dstX + frameW, dstY + y);
                }

                for (int x = 0; x < frameW; x++)
                {
                    CopyPixel(orig.Data, orig.Width, srcX + x, srcY, newData, newW, dstX + x, dstY - 1);
                    CopyPixel(orig.Data, orig.Width, srcX + x, srcY + frameH - 1, newData, newW, dstX + x, dstY + frameH);
                }
            }
        }

        return new Rgba8ImageData(newW, newH, newData);
    }

    private static void CopyPixel(byte[] src, int srcW, int srcX, int srcY, byte[] dst, int dstW, int dstX, int dstY)
    {
        int srcOff = (srcY * srcW + srcX) * 4;
        int dstOff = (dstY * dstW + dstX) * 4;
        dst[dstOff] = src[srcOff];
        dst[dstOff + 1] = src[srcOff + 1];
        dst[dstOff + 2] = src[srcOff + 2];
        dst[dstOff + 3] = src[srcOff + 3];
    }
}

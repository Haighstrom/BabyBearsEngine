using BabyBearsEngine.Platform.ImageLoading;

namespace BabyBearsEngine.OpenGL;

internal sealed class DefaultTextureFactory() : ITextureFactory
{
    public ITexture CreateTextureFromImageFile(string filePath)
    {
        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);

        var imageData = ImageLoader.LoadAsRgba8(filePath);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, imageData.Width, imageData.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageData.Data);

        // First, we set the min and mag filter. These are used for when the texture is scaled down and up, respectively.
        // Here, we use Linear for both. This means that OpenGL will try to blend pixels, meaning that textures scaled too far will look blurred.
        // You could also use (amongst other options) Nearest, which just grabs the nearest pixel, which makes the texture look pixelated if scaled too far.
        // NOTE: The default settings for both of these are LinearMipmap. If you leave these as default but don't generate mipmaps,
        // your image will fail to render at all (usually resulting in pure black instead).
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
        // We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // Next, generate mipmaps.
        // Mipmaps are smaller copies of the texture, scaled down. Each mipmap level is half the size of the previous one
        // Generated mipmaps go all the way down to just one pixel.
        // OpenGL will automatically switch between mipmaps when an object gets sufficiently far away.
        // This prevents moir� effects, as well as saving on texture bandwidth.
        // Here you can see and read about the mori� effect https://en.wikipedia.org/wiki/Moir%C3%A9_pattern
        // Here is an example of mips in action https://en.wikipedia.org/wiki/File:Mipmap_Aliasing_Comparison.png
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        var texture = new Texture(handle, imageData.Width, imageData.Height);

        return texture;
    }

    public ITexture GenBorderedRectangle(int width, int height, int borderThickness, Colour fillColour, Colour borderColour)
    {
        Colour[,] pixels = new Colour[width, height];

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

        return GenTexture(pixels);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public ITexture GenTexture(System.Drawing.Bitmap bmp)//, TEXPARAMETER_VALUE minMagFilter = TEXPARAMETER_VALUE.GL_NEAREST)
    {
        bmp = BitmapTools.PremultiplyAlpha(bmp);

        Texture t = new(
            handle: GL.GenTexture(), 
            width: bmp.Width, 
            height: bmp.Height);

        GL.BindTexture(TextureTarget.Texture2D, t.Handle);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        var bmpd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpd.Width, bmpd.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpd.Scan0);

        bmp.UnlockBits(bmpd);

        return t;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public ITexture GenTexture(Colour[,] pixels)
    {
        int width = pixels.GetLength(0);
        int height = pixels.GetLength(1);

        System.Drawing.Bitmap bmp = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var bmpd = bmp.LockBits(
            new System.Drawing.Rectangle(0, 0, width, height),
            System.Drawing.Imaging.ImageLockMode.WriteOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        unsafe
        {
            byte* ptr = (byte*)bmpd.Scan0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Colour c = pixels[x, y];
                    byte* p = ptr + y * bmpd.Stride + x * 4;
                    p[0] = c.B; // Format32bppArgb stores as BGRA
                    p[1] = c.G;
                    p[2] = c.R;
                    p[3] = c.A;
                }
            }
        }

        bmp.UnlockBits(bmpd);
        return GenTexture(bmp);
    }
}

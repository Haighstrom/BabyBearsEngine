using System.IO;
using System.Numerics;
using BabyBearsEngine.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StbTrueTypeSharp;
using static StbTrueTypeSharp.StbTrueType;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

internal sealed class FontTexture
{
    public static Texture GetStbFontTexture(string fontPath, string text)
    {
        byte[] fontData = File.ReadAllBytes(fontPath);

        var fontInfo = StbTrueType.CreateFont(fontData, 0) 
            ?? throw new Exception("Failed to load font.");

        int bitmapWidth = 512;
        int bitmapHeight = 128;
        int lineHeight = 64;

        byte[] bitmap = new byte[bitmapWidth * bitmapHeight];

        // calculate font scaling
        float scale = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, lineHeight);

        int x = 0;
        int ascent, descent, lineGap;

        // Render the character 'A' to the bitmap
        unsafe
        {
            StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);
            ascent = (int)(ascent * scale);
            descent = (int)(descent * scale);

            fixed (byte* bmpPtr = bitmap)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    /* get bounding box for character (may be offset to account for chars that dip above or below the line */
                    int c_x1, c_y1, c_x2, c_y2;
                    StbTrueType.stbtt_GetCodepointBitmapBox(fontInfo, text[i], scale, scale, &c_x1, &c_y1, &c_x2, &c_y2);

                    /* compute y (different characters have different heights */
                    int y = ascent + c_y1;

                    int byteOffset = x + (y * bitmapWidth);
                    StbTrueType.stbtt_MakeCodepointBitmap(fontInfo, bmpPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, bitmapWidth, scale, scale, text[i]);

                    /* how wide is this character */
                    int ax;
                    StbTrueType.stbtt_GetCodepointHMetrics(fontInfo, text[i], &ax, default);
                    x += (int)(ax * scale);

                    if (i + 1 < text.Length)
                    {
                        /* add kerning */
                        int kern;
                        kern = StbTrueType.stbtt_GetCodepointKernAdvance(fontInfo, text[i], text[i + 1]);
                        x += (int)(kern * scale);
                    }
                }
            }
        }

        using (var image = Image<A8>.LoadPixelData<A8>(bitmap, bitmapWidth, bitmapHeight))
        {
            image.SaveAsPng("font_char_A.png");
        }

        // Upload the bitmap to OpenGL as a texture
        var textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, bitmapWidth, bitmapHeight, 0, PixelFormat.Red, PixelType.UnsignedByte, bitmap);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

        //Set texture parameters
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);


        GL.BindTexture(TextureTarget.Texture2D, 0);

        return new Texture(textureId, bitmapWidth, bitmapHeight);
    }

    //public Bitmap GenerateCharacters(int fontSize, string fontName, out Size charSize)
    //{
    //    var characters = new List<Bitmap>();
    //    using (var font = new Font(fontName, fontSize))
    //    {
    //        for (int i = 0; i < Characters.Length; i++)
    //        {
    //            var charBmp = GenerateCharacter(font, Characters[i]);
    //            characters.Add(charBmp);
    //        }
    //        charSize = new Size(characters.Max(x => x.Width), characters.Max(x => x.Height));
    //        var charMap = new Bitmap(charSize.Width * characters.Count, charSize.Height);
    //        using (var gfx = Graphics.FromImage(charMap))
    //        {
    //            gfx.FillRectangle(Brushes.Black, 0, 0, charMap.Width, charMap.Height);
    //            for (int i = 0; i < characters.Count; i++)
    //            {
    //                var c = characters[i];
    //                gfx.DrawImageUnscaled(c, i * charSize.Width, 0);

    //                c.Dispose();
    //            }
    //        }
    //        return charMap;
    //    }
    //}

    //private Bitmap GenerateCharacter(Font font, char c)
    //{
    //    var size = GetSize(font, c);
    //    var bmp = new Bitmap((int)size.Width, (int)size.Height);
    //    using (var gfx = Graphics.FromImage(bmp))
    //    {
    //        gfx.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
    //        gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
    //    }
    //    return bmp;
    //}

    private static Vector2 GetSize(stbtt_fontinfo fontInfo, char c)
    {
        float scale = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, 64);

        int c_x1, c_y1, c_x2, c_y2;

        unsafe
        {
            /* get bounding box for character (may be offset to account for chars that dip above or below the line */
            StbTrueType.stbtt_GetCodepointBitmapBox(fontInfo, c, scale, scale, &c_x1, &c_y1, &c_x2, &c_y2);
        }

        return new Vector2(c_x2 - c_x1, c_y2 - c_y1);
    }
}

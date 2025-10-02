using System;
using System.IO;
using BabyBearsEngine.Source.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;
using StbTrueTypeSharp;

namespace BabyBearsEngine.Source.Graphics.Text;

internal class FontTexture
{
    private const string Characters = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789µ§½!""#¤%&/()=?^*@£€${[]}\~¨'-_.:,;<>|°©®±¥";
    private const int GlyphSize = 64; // Adjust based on font size

    public static Texture GetFontTexture(string fontPath)
    {
        byte[] fontData = File.ReadAllBytes(fontPath);
        var font = StbTrueType.CreateFont(fontData, 0);

        // Create a bitmap for a single character (e.g., 'A')
        int width = GlyphSize, height = GlyphSize;
        byte[] bitmap = new byte[width * height];

        // Scale factor based on desired pixel height
        float scaleY = StbTrueType.stbtt_ScaleForPixelHeight(font, height);

        // Render the character 'A' to the bitmap
        unsafe
        {
            fixed (byte* bmpPtr = bitmap)
            {
                StbTrueType.stbtt_MakeCodepointBitmap(font, bmpPtr, GlyphSize, GlyphSize, 128, scaleY, scaleY, 'A');
            }
        };

        // Upload the bitmap to OpenGL as a texture
        var textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rg8, width, height, 0, PixelFormat.Rg, PixelType.UnsignedByte, bitmap);

        // Set texture parameters
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        GL.BindTexture(TextureTarget.Texture2D, 0);

        return new Texture(textureId, width, height);
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
    //private SizeF GetSize(Font font, char c)
    //{
    //    using (var bmp = new Bitmap(512, 512))
    //    {
    //        using (var gfx = Graphics.FromImage(bmp))
    //        {
    //            return gfx.MeasureString(c.ToString(), font);
    //        }
    //    }
    //}
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;

internal sealed class CharacterBitmapGenerator : ICharacterBitmapGenerator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public Bitmap GenerateCharacterBitmap(char c, Font font, bool antiAliased)
    {
        Bitmap image = new((int)font.Size * 3, (int)font.Size * 3, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var g = System.Drawing.Graphics.FromImage(image);
        g.TextRenderingHint = antiAliased ? TextRenderingHint.AntiAlias : TextRenderingHint.AntiAliasGridFit;
        g.SmoothingMode = antiAliased ? SmoothingMode.AntiAlias : SmoothingMode.Default;

        var stringFormat = new StringFormat(StringFormat.GenericTypographic);

        g.DrawString(c.ToString(), font, new SolidBrush(Color.White), 0, 0, stringFormat);

        var (x, y, width, height) = image.NonZeroAlphaRegion();

        if (width == 0 || height == 0)
        {
            //if (c != ' ')
            //    Log.Warning($"Character '{c}' is alleged to have size (W:{r.W},H:{r.H}) in font {FontName}, size {FontSize}. Reverting to trying character 't'.");

            g.DrawString("t", font, new SolidBrush(Color.White), 0, 0, stringFormat);

            (x, y, width, height) = image.NonZeroAlphaRegion();

            if (width == 0 || height == 0)
                throw new InvalidOperationException($"HFont.cs/GenerateCharacterBitmap: Font {font.Name}, Size {font.Size}, character 't' is giving size (W:{width},H:{height})");
        }

        image = new Bitmap(x + width, y + height, System.Drawing.Imaging.PixelFormat.Format32bppArgb); //include alpha at left/top of image so positioning is preserved
        g = System.Drawing.Graphics.FromImage(image);
        g.TextRenderingHint = antiAliased ? TextRenderingHint.AntiAlias : TextRenderingHint.AntiAliasGridFit;
        g.SmoothingMode = antiAliased ? SmoothingMode.AntiAlias : SmoothingMode.Default;
        g.DrawString(c.ToString(), font, new SolidBrush(Color.White), 0, 0, stringFormat);

        return image;
    }
}

using System.Drawing;
using System.Drawing.Text;

namespace BabyBearsEngine.Worlds.Graphics.Text;

internal sealed class CharacterBitmapGenerator : ICharacterBitmapGenerator
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public Bitmap GenerateCharacterBitmap(char c, Font font)
    {
        Bitmap image = new((int)font.Size * 3, (int)font.Size * 3, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var g = System.Drawing.Graphics.FromImage(image);
        // Grid-fit (hint) the glyph so stems snap to whole pixels — keeps small text crisp rather
        // than blurring it across partial-coverage columns. Must match the second pass below.
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var stringFormat = new StringFormat(StringFormat.GenericTypographic);

        g.DrawString(c.ToString(), font, new SolidBrush(Color.White), 0, 0, stringFormat);

        var (x, y, width, height) = image.NonZeroAlphaRegion();

        if (width == 0 || height == 0)
        {
            g.DrawString("t", font, new SolidBrush(Color.White), 0, 0, stringFormat);

            (x, y, width, height) = image.NonZeroAlphaRegion();

            if (width == 0 || height == 0)
            {
                throw new InvalidOperationException($"HFont.cs/GenerateCharacterBitmap: Font {font.Name}, Size {font.Size}, character 't' is giving size (W:{width},H:{height})");
            }
        }

        image = new Bitmap(x + width, y + height, System.Drawing.Imaging.PixelFormat.Format32bppArgb); //include alpha at left/top of image so positioning is preserved
        g = System.Drawing.Graphics.FromImage(image);
        // Grid-fit hinting must match the measuring pass above so the cropped glyph matches what
        // NonZeroAlphaRegion measured.
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.DrawString(c.ToString(), font, new SolidBrush(Color.White), 0, 0, stringFormat);

        return image;
    }
}

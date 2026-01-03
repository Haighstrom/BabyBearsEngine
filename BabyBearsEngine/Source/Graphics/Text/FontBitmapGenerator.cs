using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics.Text;

internal sealed class FontBitmapGenerator() : IFontBitmapGenerator
{
    private readonly CharacterBitmapGenerator _charBitmapGenerator = new();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public GeneratedFontStruct GenerateCharSpritesheetAndPositions(Font font, string charsToLoad, bool antiAliased, int charactersPerRow)
    {
        int widestChar = 0;
        int highestChar = 0;

        var characterBMPs = new List<Bitmap>();
        //find the biggest character and create the spritesheet based on this size
        foreach (char c in charsToLoad)
        {
            var b = _charBitmapGenerator.GenerateCharacterBitmap(c, font, antiAliased);

            if (b.Width > widestChar)
                widestChar = b.Width;
            
            if (b.Height > highestChar)
                highestChar = b.Height;
            
            characterBMPs.Add(b);
        }

        int spriteSheetWidth = charactersPerRow * widestChar;
        int spriteSheetHeight = (int)Math.Ceiling((float)characterBMPs.Count / charactersPerRow) * highestChar;

        Bitmap characterSS = new(spriteSheetWidth, spriteSheetHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var g = System.Drawing.Graphics.FromImage(characterSS);
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.TextRenderingHint = antiAliased ? TextRenderingHint.AntiAlias : TextRenderingHint.AntiAliasGridFit;

        int j = 0, posX, posY;

        Dictionary<char, Box2i> charPositions = [];
        Dictionary<char, Box2> charPositionsNormalised = [];

        foreach (char c in charsToLoad)
        {
            posX = j % charactersPerRow * widestChar;
            posY = j / charactersPerRow * highestChar;

            var charRect = new Box2i(posX, posY, posX + characterBMPs[j].Width, posY + highestChar);
            charPositions.Add(c, charRect);
            //charRect = charRect.ScaleAround(spriteSheetWidth, spriteSheetHeight, 0, 0);
            var charRectNormalised = new Box2(charRect.Min.X / (float)spriteSheetWidth, charRect.Min.Y / (float)spriteSheetHeight, charRect.Min.X / (float)spriteSheetWidth + charRect.Size.X / (float)spriteSheetWidth, charRect.Min.Y / (float)spriteSheetHeight + charRect.Size.Y / (float)spriteSheetHeight);
            charPositionsNormalised.Add(c, charRectNormalised);

            using (var ia = new ImageAttributes())
            {
                ia.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(characterBMPs[j], posX, posY);
            }

            j++;
        }

        foreach (var b in characterBMPs)
        {
            b.Dispose();
        }

        characterSS.Save("fontAsBitmap.png");

        return new GeneratedFontStruct(
            CharacterSS: characterSS,
            WidestChar: widestChar,
            HighestChar: highestChar,
            CharPositions: charPositions,
            CharPositionsNormalised: charPositionsNormalised);
    }
}

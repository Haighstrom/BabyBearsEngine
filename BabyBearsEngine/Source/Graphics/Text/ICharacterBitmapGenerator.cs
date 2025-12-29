using System.Drawing;

namespace BabyBearsEngine.Source.Graphics.Text;
internal interface ICharacterBitmapGenerator
{
    Bitmap GenerateCharacterBitmap(char c, Font font, bool antiAliased);
}
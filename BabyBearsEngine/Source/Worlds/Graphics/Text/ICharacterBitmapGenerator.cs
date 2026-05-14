using System.Drawing;

namespace BabyBearsEngine.Worlds.Graphics.Text;
internal interface ICharacterBitmapGenerator
{
    Bitmap GenerateCharacterBitmap(char c, Font font, bool antiAliased);
}
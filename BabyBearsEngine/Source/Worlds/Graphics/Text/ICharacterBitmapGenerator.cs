using System.Drawing;

namespace BabyBearsEngine.Rendering.Graphics.Text;
internal interface ICharacterBitmapGenerator
{
    Bitmap GenerateCharacterBitmap(char c, Font font, bool antiAliased);
}
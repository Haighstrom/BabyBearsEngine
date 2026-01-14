using System.Drawing;

namespace BabyBearsEngine.Source.Rendering.Graphics.Text;
internal interface ICharacterBitmapGenerator
{
    Bitmap GenerateCharacterBitmap(char c, Font font, bool antiAliased);
}
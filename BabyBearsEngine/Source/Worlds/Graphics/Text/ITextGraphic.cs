using BabyBearsEngine.Graphics;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Rendering.Graphics.Text;

public interface ITextGraphic : IRenderable
{
    Color4 Colour { get; set; }

    string Text { get; set; }
}

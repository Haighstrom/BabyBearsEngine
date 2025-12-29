using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics.Text;

public interface ITextGraphic : IRenderable
{
    Color4 Colour { get; set; }

    string Text { get; set; }
}

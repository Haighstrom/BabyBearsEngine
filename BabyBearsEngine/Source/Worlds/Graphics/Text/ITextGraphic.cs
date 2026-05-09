using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Rendering.Graphics.Text;

public interface ITextGraphic : IRenderable
{
    Colour Colour { get; set; }

    string Text { get; set; }
}

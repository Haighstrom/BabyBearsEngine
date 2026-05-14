using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public interface ITextGraphic : IRenderable
{
    Colour Colour { get; set; }

    string Text { get; set; }
}

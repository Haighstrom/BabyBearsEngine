using BabyBearsEngine.Source.Worlds;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.UI;

public class Button : IEntity
{
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;
    private readonly Color4 _colour;

    public Button(int x, int y, int width, int height, Color4 colour)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _colour = colour;

       // var image = new Image()
    }

    public void Render()
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        throw new NotImplementedException(
);
    }
}

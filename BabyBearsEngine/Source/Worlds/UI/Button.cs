using BabyBearsEngine.Graphics;
using BabyBearsEngine.Input;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Rendering.Graphics.Text;

namespace BabyBearsEngine.Worlds.UI;

public class Button(int x, int y, int width, int height, Colour colour, string textToDisplay = "") : IEntity
{
    private bool _disposed;
    private readonly ColouredRectangle _graphic = new(colour, x, y, width, height);
    private readonly TextImage _textImage = new(new FontDefinition("Times New Roman", 16), textToDisplay, Colour.Black, x, y, width, height);

    public void Update(double elapsed)
    {
        if (Mouse.LeftPressed && Mouse.ClientX >= x && Mouse.ClientX < x + width && Mouse.ClientY >= y && Mouse.ClientY < y + height)
        {
            OnClicked();
        }
    }

    public virtual void OnClicked()
    {
    }

    public void Render(Matrix3 projection)
    {
        _graphic.Render(projection);
        _textImage.Render(projection);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _graphic.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Button()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

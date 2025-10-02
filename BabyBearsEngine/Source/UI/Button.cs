using BabyBearsEngine.Source.Core;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.UI;

public class Button(int x, int y, int width, int height, Color4 colour) : IEntity
{
    private bool _disposed;
    private readonly ColouredRectangle _graphic = new(colour, x, y, width, height);

    public void Update()
    {
        if (Mouse.LeftPressed && Mouse.ClientX >= x && Mouse.ClientX < x + width && Mouse.ClientY >= y && Mouse.ClientY < y + height)
        {
            OnClicked();
        }
    }

    public virtual void OnClicked()
    {
        System.Console.WriteLine("Button clicked!");
    }

    public void Render()
    {
        _graphic.Render();
    }

    #region IDisposable
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
    #endregion
}

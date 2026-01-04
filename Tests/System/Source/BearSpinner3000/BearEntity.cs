using System;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Tools;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Tests.System.Source.BearSpinner3000;

internal class BearEntity(int startX, int startY) : IEntity
{
    private bool _disposed;
    private readonly Image _graphic = new("Assets/SpinnableBear.png", startX, startY, 60, 80) 
    { 
        Colour = ColourTools.RandSystemColour(), 
        Angle = Randomisation.Rand(360) 
    };
    private readonly float _rotateSpeed = 10 * Randomisation.RandF(-10, 10);
    private readonly float _swaySpeed = Randomisation.RandF(-4, 4);
    private readonly float _alphaShift = Randomisation.RandF(0, 100);
    private readonly float _alphaSpeed = Randomisation.RandF(0, 5);
    private readonly float _xSway = Randomisation.Rand(0, 500);
    private readonly float _ySway = Randomisation.Rand(0, 500);
    private double _totalElapsed = 0;

    public void Render()
    {
        _graphic.Render();
    }

    public void Update()
    {
        var elapsed = 1 / 60f;

        _totalElapsed += elapsed;
        _graphic.Alpha = (1 + (float)Math.Sin(_alphaShift + _alphaSpeed * _totalElapsed)) / 2;
        _graphic.Angle += _rotateSpeed * (float)elapsed;

        _graphic.X = startX + _xSway * (float)Math.Sin(_swaySpeed * _totalElapsed);
        _graphic.Y = startY + _ySway * (float)Math.Cos(_swaySpeed * _totalElapsed);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Bear()
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

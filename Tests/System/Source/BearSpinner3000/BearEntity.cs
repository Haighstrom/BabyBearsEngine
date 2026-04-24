using System;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System.Source.BearSpinner3000;

internal class BearEntity(int startX, int startY) : AddableBase, IEntity
{
    private readonly Image _graphic = new(Textures.CreateFromFile("Assets/SpinnableBear.png"), startX, startY, 60, 80) 
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

    public bool Active { get; set; } = true;
    public bool Visible { get; set; } = true;

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _graphic.Render(ref projection, ref modelView);
    }

    public void Update(double elapsed)
    {
        _totalElapsed += elapsed;
        _graphic.Alpha = (1 + (float)Math.Sin(_alphaShift + _alphaSpeed * _totalElapsed)) / 2;
        _graphic.Angle += _rotateSpeed * (float)elapsed;

        _graphic.X = startX + _xSway * (float)Math.Sin(_swaySpeed * _totalElapsed);
        _graphic.Y = startY + _ySway * (float)Math.Cos(_swaySpeed * _totalElapsed);
    }
}

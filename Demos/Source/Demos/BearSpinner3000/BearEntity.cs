using System;

namespace BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;

internal class BearEntity : Entity
{
    private readonly float _startX;
    private readonly float _startY;
    private readonly TextureGraphic _graphic;
    private readonly float _rotateSpeed = 10 * Randomisation.RandF(-10, 10);
    private readonly float _swaySpeed = Randomisation.RandF(-4, 4);
    private readonly float _alphaShift = Randomisation.RandF(0, 100);
    private readonly float _alphaSpeed = Randomisation.RandF(0, 5);
    private readonly float _xSway = Randomisation.Rand(0, 500);
    private readonly float _ySway = Randomisation.Rand(0, 500);
    private double _totalElapsed = 0;

    public BearEntity(float startX, float startY)
        : base(startX, startY, 60, 80)
    {
        _startX = startX;
        _startY = startY;
        _graphic = new TextureGraphic(Textures.CreateFromFile("Assets/SpinnableBear.png"), 0, 0, 60, 80)
        {
            Colour = ColourTools.RandSystemColour(),
            Angle = Randomisation.Rand(360),
        };
        Add(_graphic);
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);
        _totalElapsed += elapsed;
        _graphic.Alpha = (1 + (float)Math.Sin(_alphaShift + _alphaSpeed * _totalElapsed)) / 2;
        _graphic.Angle += _rotateSpeed * (float)elapsed;

        X = _startX + _xSway * (float)Math.Sin(_swaySpeed * _totalElapsed);
        Y = _startY + _ySway * (float)Math.Cos(_swaySpeed * _totalElapsed);
    }
}

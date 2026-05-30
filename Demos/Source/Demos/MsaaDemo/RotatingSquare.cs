namespace BabyBearsEngine.Demos.Source.Demos.MsaaDemo;

internal class RotatingSquare : Entity
{
    private const float Size = 220f;
    private const float RotateSpeed = 12f;

    private readonly ColourGraphic _graphic;

    public RotatingSquare(float centreX, float centreY, Colour colour)
        : base(centreX - Size / 2f, centreY - Size / 2f, Size, Size)
    {
        _graphic = new ColourGraphic(colour, 0, 0, Size, Size);
        Add(_graphic);
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);
        _graphic.Angle = (_graphic.Angle + RotateSpeed * (float)elapsed) % 360f;
    }
}

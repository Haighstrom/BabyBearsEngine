using System;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;

internal class BearTarget : Entity
{
    private const double LifeSpan = 3.0;
    private const double FadeStartAt = 1.0;

    private readonly Image _image;
    private double _lifeRemaining = LifeSpan;
    private bool _done;

    public event EventHandler? BearClicked;
    public event EventHandler? BearExpired;

    public BearTarget(int x, int y) : base(x, y, 60, 60, clickable: true)
    {
        _image = new Image(Textures.CreateFromFile("Assets/SpinnableBear.png"), 0, 0, 60, 60);
        Add(_image);
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        if (_done)
        {
            return;
        }

        _lifeRemaining -= elapsed;

        if (_lifeRemaining <= FadeStartAt)
        {
            _image.Alpha = (float)Math.Max(0, _lifeRemaining / FadeStartAt);
        }

        if (_lifeRemaining <= 0)
        {
            _done = true;
            BearExpired?.Invoke(this, EventArgs.Empty);
            Remove();
        }
    }

    protected override void OnLeftReleased()
    {
        if (_done)
        {
            return;
        }

        _done = true;
        BearClicked?.Invoke(this, EventArgs.Empty);
        Remove();
    }
}

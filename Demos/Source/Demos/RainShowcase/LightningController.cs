using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Demos.Source.Demos.RainShowcase;

/// <summary>
/// Drives the full-screen lightning flash overlay used by <see cref="RainShowcaseWorld"/>. Each
/// flash fades in over <see cref="RiseSeconds"/> and out over <see cref="FallSeconds"/>; when
/// <see cref="AutoFlash"/> is true the controller also schedules its own flashes at random
/// intervals to keep the storm feeling alive.
/// </summary>
internal sealed class LightningController(ColourGraphic overlay, Random random) : UpdateableBase
{
    private const float RiseSeconds = 0.06f;
    private const float FallSeconds = 0.55f;
    private const float MinIntervalSeconds = 4f;
    private const float MaxIntervalSeconds = 11f;

    private float _flashTime = 0f;
    private float _flashDuration = 0f;
    private float _autoTimer = 0f;
    private bool _autoFlash = false;

    public bool AutoFlash
    {
        get => _autoFlash;
        set
        {
            _autoFlash = value;
            if (value)
            {
                _autoTimer = NextAutoInterval();
            }
        }
    }

    public void Trigger()
    {
        _flashTime = 0f;
        _flashDuration = RiseSeconds + FallSeconds;
    }

    public override void Update(double elapsed)
    {
        float dt = (float)elapsed;

        if (_flashDuration > 0f)
        {
            _flashTime += dt;
            if (_flashTime >= _flashDuration)
            {
                _flashDuration = 0f;
                overlay.Colour = new Colour(255, 255, 255, 0);
            }
            else
            {
                float alpha = _flashTime < RiseSeconds
                    ? _flashTime / RiseSeconds
                    : 1f - (_flashTime - RiseSeconds) / FallSeconds;
                byte a = (byte)Math.Round(Math.Clamp(alpha, 0f, 1f) * 200f);
                overlay.Colour = new Colour(255, 255, 255, a);
            }
        }

        if (_autoFlash)
        {
            _autoTimer -= dt;
            if (_autoTimer <= 0f)
            {
                Trigger();
                _autoTimer = NextAutoInterval();
            }
        }
    }

    private float NextAutoInterval()
    {
        return MinIntervalSeconds + (MaxIntervalSeconds - MinIntervalSeconds) * (float)random.NextDouble();
    }
}

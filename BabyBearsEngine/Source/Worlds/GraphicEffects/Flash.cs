using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds.GraphicEffects;

/// <summary>
/// Drives a brief alpha rise / alpha fall flash on an <see cref="IGraphic"/>'s tint colour —
/// the classic lightning strike, hit flash, pickup glow, or screen blink. Modifies only the
/// alpha channel of <see cref="IGraphic.Colour"/>, leaving RGB alone, so callers pick the
/// flash colour by setting the target's base colour (white for a lightning overlay, red for
/// a damage tint, etc.) and the effect just animates its visibility.
/// </summary>
/// <remarks>
/// <para>Call <see cref="Trigger"/> for manual one-shots; set <see cref="AutoFlash"/> to true
/// to have the effect re-fire itself at random intervals in
/// <c>[<see cref="AutoFlashMinInterval"/>, <see cref="AutoFlashMaxInterval"/>]</c>.</para>
///
/// <para>The "rest alpha" is captured lazily on the first <see cref="Trigger"/> — whatever
/// alpha the target has then is what it returns to between flashes (typically 0 for an
/// overlay). Capturing at first trigger rather than at construction lets a theme or other
/// post-construction step finalise the target's colour before the rest is locked in.</para>
/// </remarks>
public sealed class Flash(IGraphic target, IRandom? random = null) : UpdateableBase
{
    private readonly IRandom _random = random ?? EngineConfiguration.RandomService;
    private byte _restAlpha = 0;
    private bool _restAlphaCaptured = false;
    private float _flashTime = 0f;
    private float _flashDuration = 0f;
    private float _autoTimer = 0f;
    private bool _autoFlash = false;

    /// <summary>Time in seconds for the flash alpha to rise from rest to peak. Default 0.06.</summary>
    public float RiseSeconds { get; set; } = 0.06f;

    /// <summary>Time in seconds for the flash alpha to fall from peak back to rest. Default 0.55.</summary>
    public float FallSeconds { get; set; } = 0.55f;

    /// <summary>Peak alpha reached at the apex of a flash, in [0, 255]. Default 200.</summary>
    public byte PeakAlpha { get; set; } = 200;

    /// <summary>Minimum random interval in seconds between auto-fired flashes. Default 4.</summary>
    public float AutoFlashMinInterval { get; set; } = 4f;

    /// <summary>Maximum random interval in seconds between auto-fired flashes. Default 11.</summary>
    public float AutoFlashMaxInterval { get; set; } = 11f;

    /// <summary>
    /// When true, the effect schedules its own flashes at random intervals between
    /// <see cref="AutoFlashMinInterval"/> and <see cref="AutoFlashMaxInterval"/>. Setting this
    /// to true resets the next-fire timer immediately; setting it false pauses auto-firing
    /// (any flash already in flight runs to completion).
    /// </summary>
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

    /// <summary>True while a flash is mid-rise or mid-fall; false at rest.</summary>
    public bool IsFlashing => _flashDuration > 0f;

    /// <summary>
    /// Starts a flash now. Resets the rise/fall timer so the new flash plays from the
    /// beginning, even if the previous one hadn't finished — manual triggers always win.
    /// </summary>
    public void Trigger()
    {
        if (!_restAlphaCaptured)
        {
            _restAlpha = target.Colour.A;
            _restAlphaCaptured = true;
        }

        _flashTime = 0f;
        _flashDuration = RiseSeconds + FallSeconds;
    }

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        float dt = (float)elapsed;

        if (_flashDuration > 0f)
        {
            _flashTime += dt;
            if (_flashTime >= _flashDuration)
            {
                _flashDuration = 0f;
                SetAlpha(_restAlpha);
            }
            else
            {
                float curve = _flashTime < RiseSeconds
                    ? _flashTime / RiseSeconds
                    : 1f - (_flashTime - RiseSeconds) / FallSeconds;
                float clamped = Math.Clamp(curve, 0f, 1f);
                byte alpha = (byte)Math.Round(_restAlpha + (PeakAlpha - _restAlpha) * clamped);
                SetAlpha(alpha);
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
        return _random.Float(AutoFlashMinInterval, AutoFlashMaxInterval);
    }

    private void SetAlpha(byte alpha)
    {
        Colour current = target.Colour;
        target.Colour = new Colour(current.R, current.G, current.B, alpha);
    }
}

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Translates an <see cref="IMoveFadeable"/> target along a straight path while fading its
/// alpha to zero, then removes it from the world when the effect lifetime expires.
/// </summary>
/// <remarks>
/// Add this controller to the same container as the target so it receives
/// <see cref="Update"/> calls each frame. The controller raises <see cref="Completed"/> when
/// it removes the target; subscribe to clean up the controller itself at that point.
/// </remarks>
/// <param name="target">The entity or graphic to move, fade, and remove.</param>
/// <param name="velocityX">Horizontal speed in pixels per second.</param>
/// <param name="velocityY">Vertical speed in pixels per second.</param>
/// <param name="duration">Total effect lifetime in seconds.</param>
public class MoveFadeRemoveController(IMoveFadeable target, float velocityX, float velocityY, double duration) : UpdateableBase
{
    private readonly byte _initialAlpha = target.Colour.A;
    private bool _done = false;
    private double _elapsed = 0.0;

    /// <summary>Raised when the effect completes and the target has been removed from the world.</summary>
    public event EventHandler? Completed;

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        if (_done)
        {
            return;
        }

        _elapsed += elapsed;

        float progress = (float)Math.Min(_elapsed / duration, 1.0);
        target.X += velocityX * (float)elapsed;
        target.Y += velocityY * (float)elapsed;

        Colour c = target.Colour;
        target.Colour = new Colour(c.R, c.G, c.B, (byte)(_initialAlpha * (1f - progress)));

        if (_elapsed >= duration)
        {
            _done = true;
            target.Remove();
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}

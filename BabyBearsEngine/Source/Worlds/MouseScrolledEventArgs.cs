namespace BabyBearsEngine.Worlds;

/// <summary>Event data for <see cref="Entity.MouseScrolled"/>.</summary>
/// <param name="delta">Vertical scroll amount this frame. Positive scrolls up, negative scrolls down.</param>
public sealed class MouseScrolledEventArgs(float delta) : EventArgs
{
    /// <summary>Vertical scroll amount this frame. Positive scrolls up, negative scrolls down.</summary>
    public float Delta { get; } = delta;
}

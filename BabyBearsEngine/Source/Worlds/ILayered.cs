namespace BabyBearsEngine.Worlds;

public interface ILayered
{
    /// <summary>
    /// The rendering layer. Higher values are drawn first (further behind).
    /// Layer 0 is drawn last (on top). Must be 0 or greater.
    /// </summary>
    int Layer { get; }

    event EventHandler<LayerChangedEventArgs>? LayerChanged;

    /// <summary>Sets the rendering layer. Must be 0 or greater.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="layer"/> is negative.</exception>
    void SetLayer(int layer);
}

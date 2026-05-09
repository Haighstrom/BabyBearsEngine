namespace BabyBearsEngine.Worlds;

/// <summary>
/// Implemented by anything that participates in layered ordering inside a container.
/// Higher-numbered layers are drawn first (further behind); lower layers are drawn on top.
/// Containers re-sort themselves automatically when <see cref="LayerChanged"/> fires.
/// </summary>
public interface ILayered
{
    /// <summary>
    /// The rendering layer. Higher values are drawn first (further behind).
    /// Layer 0 is drawn last (on top). Must be 0 or greater.
    /// </summary>
    int Layer { get; }

    /// <summary>Raised after <see cref="Layer"/> changes via <see cref="SetLayer"/>. Containers subscribe to this to keep their render order correct.</summary>
    event EventHandler<LayerChangedEventArgs>? LayerChanged;

    /// <summary>Sets the rendering layer. Must be 0 or greater.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="layer"/> is negative.</exception>
    void SetLayer(int layer);
}

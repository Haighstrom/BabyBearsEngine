namespace BabyBearsEngine.Worlds;

/// <summary>
/// Payload for <see cref="ILayered.LayerChanged"/>. Carries the previous and new layer values
/// so subscribers can decide whether they need to react (e.g. resort).
/// </summary>
/// <param name="oldLayer">The layer value before the change.</param>
/// <param name="newLayer">The layer value after the change.</param>
public sealed class LayerChangedEventArgs(int oldLayer, int newLayer) : EventArgs
{
    /// <summary>The new layer value.</summary>
    public int NewLayer { get; } = newLayer;

    /// <summary>The previous layer value.</summary>
    public int OldLayer { get; } = oldLayer;
}

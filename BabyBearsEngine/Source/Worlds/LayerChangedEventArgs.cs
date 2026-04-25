namespace BabyBearsEngine.Source.Worlds;

public sealed class LayerChangedEventArgs(int oldLayer, int newLayer) : EventArgs
{
    public int NewLayer { get; } = newLayer;

    public int OldLayer { get; } = oldLayer;
}

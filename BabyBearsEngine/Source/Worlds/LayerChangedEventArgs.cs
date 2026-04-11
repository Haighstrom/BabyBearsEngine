namespace BabyBearsEngine.Source.Worlds;

public class LayerChangedEventArgs(float oldLayer, float newLayer) : EventArgs
{
    public float NewLayer { get; } = newLayer;

    public float OldLayer { get; } = oldLayer;
}

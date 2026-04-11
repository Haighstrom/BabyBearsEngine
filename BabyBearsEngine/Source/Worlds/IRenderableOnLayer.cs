using BabyBearsEngine.Graphics;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Worlds;

internal interface IRenderableOnLayer : IRenderable
{
    float Layer { get; }
    event EventHandler<LayerChangedEventArgs>? LayerChanged;
}

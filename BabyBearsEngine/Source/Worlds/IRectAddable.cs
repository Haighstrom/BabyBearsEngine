using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Combiner: something that can be added to a container <em>and</em> has a position and
/// size. Most things in the engine that live in the world tree implement this — entities,
/// graphics, cameras, UI widgets — so layout and positioning utilities can target it
/// uniformly.
/// </summary>
public interface IRectAddable : IAddable, IRect
{
}

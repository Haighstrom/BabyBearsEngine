using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Marker interface for everything that participates in the engine's entity model: it can be added
/// to a container (<see cref="IAddable"/>), receives per-frame updates (<see cref="IUpdateable"/>),
/// and renders itself (<see cref="IRenderable"/>).
/// </summary>
public interface IEntity : IUpdateable, IRenderable, IAddable
{
}

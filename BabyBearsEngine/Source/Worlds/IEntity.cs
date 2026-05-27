namespace BabyBearsEngine.Worlds;

/// <summary>
/// The full entity contract. An entity is a layered, rectangular, addable thing that updates
/// each frame, renders each frame, and holds children. Everything in the engine that lives
/// as a "thing in the world" (entities, widgets, cameras) implements this.
/// </summary>
public interface IEntity : IRenderable, ILayered, IRectAddable, IUpdateable, IContainer
{
}

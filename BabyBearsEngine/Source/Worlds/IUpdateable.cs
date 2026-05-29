namespace BabyBearsEngine.Worlds;

/// <summary>
/// Anything driven by the engine each frame. Containers iterate their updateables in layer order
/// and call <see cref="Update"/> on each active one. Inherits <see cref="IAddable"/> because the
/// engine only updates things that live in the entity tree — same shape as <see cref="IRenderable"/>.
/// </summary>
public interface IUpdateable : IAddable
{
    /// <summary>When false, this updateable is skipped during the container's update pass. Defaults to true on concrete types.</summary>
    bool Active { get; set; }

    /// <summary>Advances this updateable's state by <paramref name="elapsed"/> seconds. Called once per frame by the parent container.</summary>
    /// <param name="elapsed">Seconds since the last update.</param>
    void Update(double elapsed);
}

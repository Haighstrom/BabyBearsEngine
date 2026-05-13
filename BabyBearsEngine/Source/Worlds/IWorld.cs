namespace BabyBearsEngine.Worlds;

/// <summary>
/// A top-level scene/screen — a level, menu, loading screen, etc. Acts as the root
/// <see cref="IContainer"/> in the entity tree and owns the per-frame lifecycle:
/// <see cref="Load"/> (one-shot setup) → <see cref="Update"/> (per-frame logic) → <see cref="Draw"/>
/// (per-frame rendering) → <see cref="Unload"/> (one-shot teardown when the world is replaced).
/// </summary>
public interface IWorld : IContainer
{
    /// <summary>The colour used to clear the screen at the start of each frame.</summary>
    Colour BackgroundColour { get; }

    /// <summary>
    /// A second <see cref="IContainer"/> rendered as a separate pass after the main world,
    /// for content that should sit on top of every other widget — tooltips, open dropdowns,
    /// modals, transient overlays. Add to it the same way as the world itself:
    /// <c>world.Overlay.Add(tooltip)</c>. Children of the overlay are <see cref="IUpdateable"/>-updated
    /// alongside the main scene and share the world's coordinate space.
    /// </summary>
    IContainer Overlay { get; }

    /// <summary>One-shot initialisation called when this world becomes the active world.</summary>
    void Load();

    /// <summary>One-shot teardown called when this world is being replaced. Dispose any owned resources here.</summary>
    void Unload();

    /// <summary>Per-frame update. Called once per frame before <see cref="Draw"/>.</summary>
    /// <param name="elapsed">Seconds since the last update.</param>
    void Update(double elapsed);

    /// <summary>Per-frame render. Called once per frame after <see cref="Update"/>.</summary>
    void Draw();
}

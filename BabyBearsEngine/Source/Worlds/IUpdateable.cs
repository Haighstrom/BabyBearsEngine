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

    /// <summary>
    /// When true, this updateable ticks in a second pass <em>after</em> all regular updateables in the
    /// same container have finished updating. Used by world-level coordinators (e.g. collision solvers)
    /// that need to observe entity state once everything else has moved this frame. Defaults to
    /// <c>false</c> so existing implementers don't need to do anything.
    /// </summary>
    /// <remarks>
    /// <para><strong>Snapshot at Add.</strong> The container reads this property <em>once</em>, when the
    /// updateable is added, and uses it to route into either the regular or last-pass bucket. Changing
    /// the value after the fact is <em>not</em> observed — re-add to switch buckets. This matches the
    /// engine's general treatment of structural ordering properties as snapshots, not live signals.</para>
    /// </remarks>
    bool UpdateLast => false;

    /// <summary>Advances this updateable's state by <paramref name="elapsed"/> seconds. Called once per frame by the parent container.</summary>
    /// <param name="elapsed">Seconds since the last update.</param>
    void Update(double elapsed);
}

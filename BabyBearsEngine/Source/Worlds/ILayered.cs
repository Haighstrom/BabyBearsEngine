namespace BabyBearsEngine.Worlds;

/// <summary>
/// Implemented by anything that participates in layered ordering inside a container.
/// </summary>
/// <remarks>
/// <para><strong>Convention:</strong> think of <see cref="Layer"/> as <em>distance from
/// the camera</em>. Higher values are further behind; lower values are on top. Layer 0
/// is the default and renders on top of everything else in the same container.</para>
/// <para><strong>Why unlayered = top:</strong> renderables that don't implement
/// <see cref="ILayered"/> are treated as Layer 0. If a renderable hasn't expressed a
/// preference, the safest interpretation is "just show me" — i.e. on top of anything
/// that <em>has</em> requested to be behind.</para>
/// <para><strong>Need to render above everything?</strong> Use <c>IWorld.Overlay</c>
/// — a separate render pass after the main container. Negative layers are not
/// supported; the constraint here is <see cref="Layer"/> ≥ 0.</para>
/// <para><strong>Mutation:</strong> setting <see cref="Layer"/> raises
/// <see cref="LayerChanged"/>. Containers subscribe to keep their render order correct.</para>
/// </remarks>
public interface ILayered
{
    /// <summary>The render layer (depth from the camera). Higher = further behind, lower = on top, Layer 0 = default top. Must be ≥ 0.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown by implementers when set to a negative value.</exception>
    int Layer { get; set; }

    /// <summary>Raised after <see cref="Layer"/> changes. Containers subscribe to this to keep their render order correct.</summary>
    event EventHandler<LayerChangedEventArgs>? LayerChanged;
}

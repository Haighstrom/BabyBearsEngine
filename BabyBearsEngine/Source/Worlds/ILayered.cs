namespace BabyBearsEngine.Worlds;

/// <summary>
/// Implemented by anything that participates in layered ordering inside a container.
/// </summary>
/// <remarks>
/// <para><strong>Convention:</strong> think of <see cref="Layer"/> as <em>distance from
/// the camera</em>. Higher values are further behind; lower values are on top. Negative
/// layers are not supported; the constraint here is <see cref="Layer"/> ≥ 0.</para>
/// <para><strong>Defaults vary by implementer:</strong> classes under <c>Worlds.Graphics</c>
/// (raw <c>IGraphic</c> leaves like <c>ColourGraphic</c>, <c>TextureGraphic</c>, <c>Sprite</c>,
/// etc.) default to <see cref="int.MaxValue"/> (drawn on bottom), so an explicitly-layered
/// sibling will render in front of them. Container types — <c>Entity</c>,
/// <c>ContainerEntity</c>, and the <c>Worlds.UI</c> widgets — default to <c>0</c> (drawn on
/// top), since UI chrome is normally authored as the foreground.</para>
/// <para><strong>Unlayered renderables</strong> (things that implement <see cref="IRenderable"/>
/// but not <see cref="ILayered"/>) are treated as <see cref="int.MaxValue"/> by the container.</para>
/// <para><strong>Need to render above everything?</strong> Use <c>IWorld.Overlay</c>
/// — a separate render pass after the main container.</para>
/// <para><strong>Mutation:</strong> setting <see cref="Layer"/> raises
/// <see cref="LayerChanged"/>. Containers subscribe to keep their render order correct.</para>
/// </remarks>
public interface ILayered
{
    /// <summary>The render layer (depth from the camera). Higher = further behind, lower = on top. Must be ≥ 0. The default at construction varies by implementer — see <see cref="ILayered"/> for the convention.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown by implementers when set to a negative value.</exception>
    int Layer { get; set; }

    /// <summary>Raised after <see cref="Layer"/> changes. Containers subscribe to this to keep their render order correct.</summary>
    event EventHandler<LayerChangedEventArgs>? LayerChanged;
}

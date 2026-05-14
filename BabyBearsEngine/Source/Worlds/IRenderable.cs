using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Anything drawn by the engine each frame. Containers iterate their renderables in layer order
/// and call <see cref="Render"/> on each visible one.
/// </summary>
public interface IRenderable : IAddable
{
    /// <summary>
    /// Renders this object using the supplied projection and model-view matrices.
    /// Both are passed by reference for performance — implementations must not mutate them in ways
    /// that affect siblings (compute a new matrix for any local transforms).
    /// </summary>
    /// <param name="projection">The active projection matrix (typically window-space → NDC).</param>
    /// <param name="modelView">The current model-view matrix accumulated from this renderable's ancestors.</param>
    void Render(ref Matrix3 projection, ref Matrix3 modelView);

    /// <summary>When false, this renderable is skipped during the container's render pass. Defaults to true on concrete types.</summary>
    bool Visible { get; set; }
}

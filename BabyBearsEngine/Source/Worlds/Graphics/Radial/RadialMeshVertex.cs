namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A single vertex of a generated radial fill mesh (see <see cref="RadialFillMeshBuilder"/>):
/// a local-space position plus polar-mapped texture coordinates. Deliberately GL-free so the
/// mesh geometry can be unit tested without an OpenGL context.
/// </summary>
/// <param name="X">Local-space X position, relative to the fill's top-left origin.</param>
/// <param name="Y">Local-space Y position, relative to the fill's top-left origin.</param>
/// <param name="U">
/// Angle fraction around the full circle, in [0, 1], where 0 is the theme's start angle and 1
/// is a full 360-degree loop back to the start angle. Kept relative to the full circle (not
/// rescaled to the current sweep) so the texture doesn't stretch as <see cref="RadialProgressBar.AmountFilled"/> changes.
/// </param>
/// <param name="V">Radius fraction, in [0, 1], where 0 is the inner edge (the centre for <see cref="RadialFillStyle.Pie"/>) and 1 is the outer edge.</param>
public readonly record struct RadialMeshVertex(float X, float Y, float U, float V);

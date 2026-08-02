using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A <see cref="LinePathGraphic"/> whose dash pattern scrolls along the path automatically — a
/// "marching ants" effect. Each update tick advances <see cref="LinePathGraphic.DashOffset"/> by
/// <see cref="DashSpeed"/> * elapsed seconds, wrapping it to stay within one dash period so it
/// never grows unboundedly over a long-running session.
/// </summary>
public sealed class MovingDashedLinePathGraphic : LinePathGraphic, IUpdateable
{
    /// <param name="points">
    /// The path's vertices, in the parent's local space. Consecutive points are joined by a
    /// straight segment, mitered at the shared vertex. Must contain at least 2 points. Pass the
    /// same point as both the first and last entry to close the path into a loop.
    /// </param>
    /// <param name="colour">Line colour.</param>
    /// <param name="thickness">Full line width — in pixels if <paramref name="thicknessInPixels"/>, otherwise in local-space units.</param>
    /// <param name="dashLength">Dash length along the path, in the same units as <paramref name="thickness"/>.</param>
    /// <param name="gapLength">Gap length between dashes, in the same units as <paramref name="thickness"/>.</param>
    /// <param name="dashSpeed">Scroll speed of the dash pattern, in the same units as <paramref name="thickness"/> per second.</param>
    /// <param name="thicknessInPixels">True: <paramref name="thickness"/> is a constant screen-space pixel width, unaffected by scaling. False: thickness scales with the model-view transform.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public MovingDashedLinePathGraphic(IReadOnlyList<Point> points, Colour colour, float thickness, float dashLength, float gapLength, float dashSpeed, bool thicknessInPixels = true, int layer = int.MaxValue)
        : base(points, colour, thickness, thicknessInPixels, layer)
    {
        DashLength = dashLength;
        GapLength = gapLength;
        DashSpeed = dashSpeed;
    }

    /// <inheritdoc/>
    public bool Active { get; set; } = true;

    /// <summary>Scroll speed of the dash pattern, in the same units as <see cref="LinePathGraphic.Thickness"/> per second.</summary>
    public float DashSpeed { get; set; }

    /// <inheritdoc/>
    public void Update(double elapsed)
    {
        DashOffset += DashSpeed * (float)elapsed;

        float period = DashLength + GapLength;
        if (period > 0f)
        {
            DashOffset %= period;
        }
    }
}

using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A <see cref="LineGraphic"/> whose dash pattern scrolls along the line automatically — a
/// "marching ants" effect. Each update tick advances <see cref="LineGraphic.DashOffset"/> by
/// <see cref="DashSpeed"/> * elapsed seconds, wrapping it to stay within one dash period so it
/// never grows unboundedly over a long-running session.
/// </summary>
public sealed class MovingDashedLineGraphic : LineGraphic, IUpdateable
{
    /// <param name="start">Line start point, in the parent's local space.</param>
    /// <param name="end">Line end point, in the parent's local space.</param>
    /// <param name="colour">Line colour.</param>
    /// <param name="thickness">Full line width — in pixels if <paramref name="thicknessInPixels"/>, otherwise in local-space units.</param>
    /// <param name="dashLength">Dash length along the line, in the same units as <paramref name="thickness"/>.</param>
    /// <param name="gapLength">Gap length between dashes, in the same units as <paramref name="thickness"/>.</param>
    /// <param name="dashSpeed">Scroll speed of the dash pattern, in the same units as <paramref name="thickness"/> per second.</param>
    /// <param name="thicknessInPixels">True: <paramref name="thickness"/> is a constant screen-space pixel width, unaffected by scaling. False: thickness scales with the model-view transform.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public MovingDashedLineGraphic(Point start, Point end, Colour colour, float thickness, float dashLength, float gapLength, float dashSpeed, bool thicknessInPixels = true, int layer = int.MaxValue)
        : base(start, end, colour, thickness, thicknessInPixels, layer)
    {
        DashLength = dashLength;
        GapLength = gapLength;
        DashSpeed = dashSpeed;
    }

    /// <inheritdoc/>
    public bool Active { get; set; } = true;

    /// <summary>Scroll speed of the dash pattern, in the same units as <see cref="LineGraphic.Thickness"/> per second.</summary>
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

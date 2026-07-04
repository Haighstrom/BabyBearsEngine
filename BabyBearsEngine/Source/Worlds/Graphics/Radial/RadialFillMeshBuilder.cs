namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Builds the vertex list for a <see cref="RadialProgressBar"/>'s fill mesh: a pie sector
/// (triangle fan) or a ring/annulus band (triangle strip) swept from a start angle up to
/// <c>360 * amountFilled</c> degrees. Pure geometry, no GL dependency, so the math is unit
/// testable without an OpenGL context — mirroring how <see cref="ProgressBar"/>'s fill-sizing
/// logic is testable through a stub graphic.
/// </summary>
/// <remarks>
/// Angles use the "clock" convention: <c>0</c> degrees points to 12 o'clock (straight up,
/// i.e. towards <c>-Y</c> in the engine's Y-down screen space), and positive degrees sweep
/// towards 3 o'clock for <see cref="RadialSweepDirection.Clockwise"/>. This is deliberately
/// different from <see cref="Particles.ArcEmitterShape"/>'s math convention (0 = <c>+X</c>),
/// since a "clock" framing is what this widget is named and documented for.
/// </remarks>
public static class RadialFillMeshBuilder
{
    /// <summary>
    /// Builds the fill mesh for the given sweep amount. Returns an empty array when
    /// <paramref name="amountFilled"/> is 0 (nothing to draw). The bounding box is
    /// <paramref name="width"/> x <paramref name="height"/>; the circle is centred within it
    /// with radius <c>min(width, height) / 2</c>.
    /// </summary>
    /// <param name="width">Bounding box width.</param>
    /// <param name="height">Bounding box height.</param>
    /// <param name="amountFilled">Sweep amount in [0, 1]. Not clamped — caller's responsibility.</param>
    /// <param name="fillStyle">Pie (triangle fan from the centre) or ring (triangle strip annulus).</param>
    /// <param name="ringThickness">
    /// For <see cref="RadialFillStyle.Ring"/>, the band thickness as a fraction of the outer
    /// radius, in (0, 1]. Ignored for <see cref="RadialFillStyle.Pie"/>.
    /// </param>
    /// <param name="startAngleDegrees">Angle of the sweep's start, in the clock convention described in the type remarks.</param>
    /// <param name="direction">Sweep direction from the start angle.</param>
    /// <param name="segments">Arc segments for a full 0..1 sweep; partial sweeps use proportionally fewer. Must be ≥ 1.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segments"/> is less than 1.</exception>
    public static RadialMeshVertex[] Build(float width, float height, float amountFilled, RadialFillStyle fillStyle, float ringThickness, float startAngleDegrees, RadialSweepDirection direction, int segments)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(segments, 1);

        if (amountFilled <= 0f)
        {
            return [];
        }

        float centreX = width / 2f;
        float centreY = height / 2f;
        float outerRadius = Math.Min(width, height) / 2f;
        float directionSign = direction == RadialSweepDirection.Clockwise ? 1f : -1f;
        int arcSteps = Math.Max(1, (int)Math.Ceiling(segments * amountFilled));

        if (fillStyle == RadialFillStyle.Ring)
        {
            float innerRadius = outerRadius * (1f - Math.Clamp(ringThickness, 0f, 1f));
            return BuildRing(centreX, centreY, innerRadius, outerRadius, amountFilled, startAngleDegrees, directionSign, arcSteps);
        }

        return BuildPie(centreX, centreY, outerRadius, amountFilled, startAngleDegrees, directionSign, arcSteps);
    }

    private static RadialMeshVertex[] BuildPie(float centreX, float centreY, float radius, float amountFilled, float startAngleDegrees, float directionSign, int arcSteps)
    {
        RadialMeshVertex[] vertices = new RadialMeshVertex[arcSteps + 2];
        vertices[0] = new RadialMeshVertex(centreX, centreY, 0f, 0f);

        for (int step = 0; step <= arcSteps; step++)
        {
            float sweepFraction = amountFilled * step / arcSteps;
            float angleDegrees = startAngleDegrees + directionSign * 360f * sweepFraction;
            (float x, float y) = PointOnCircle(centreX, centreY, radius, angleDegrees);
            vertices[step + 1] = new RadialMeshVertex(x, y, sweepFraction, 1f);
        }

        return vertices;
    }

    private static RadialMeshVertex[] BuildRing(float centreX, float centreY, float innerRadius, float outerRadius, float amountFilled, float startAngleDegrees, float directionSign, int arcSteps)
    {
        RadialMeshVertex[] vertices = new RadialMeshVertex[(arcSteps + 1) * 2];

        for (int step = 0; step <= arcSteps; step++)
        {
            float sweepFraction = amountFilled * step / arcSteps;
            float angleDegrees = startAngleDegrees + directionSign * 360f * sweepFraction;
            (float outerX, float outerY) = PointOnCircle(centreX, centreY, outerRadius, angleDegrees);
            (float innerX, float innerY) = PointOnCircle(centreX, centreY, innerRadius, angleDegrees);

            vertices[step * 2] = new RadialMeshVertex(outerX, outerY, sweepFraction, 1f);
            vertices[step * 2 + 1] = new RadialMeshVertex(innerX, innerY, sweepFraction, 0f);
        }

        return vertices;
    }

    private static (float X, float Y) PointOnCircle(float centreX, float centreY, float radius, float angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0;
        float x = centreX + radius * (float)Math.Sin(angleRadians);
        float y = centreY - radius * (float)Math.Cos(angleRadians);
        return (x, y);
    }
}

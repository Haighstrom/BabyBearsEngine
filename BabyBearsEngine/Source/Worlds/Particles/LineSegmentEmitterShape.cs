using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// Emits particles from a uniformly-sampled point along the line segment between <see cref="Start"/>
/// and <see cref="End"/>, all sharing the same <see cref="Velocity"/>. Use for waterfalls,
/// rain along the top of the screen, dust along a wall edge.
/// </summary>
/// <param name="start">Local-space start of the line segment.</param>
/// <param name="end">Local-space end of the line segment.</param>
/// <param name="velocity">Initial velocity for every emitted particle, in units per second.</param>
public sealed class LineSegmentEmitterShape(Point start, Point end, Point velocity) : IEmitterShape
{
    public Point Start { get; set; } = start;

    public Point End { get; set; } = end;

    public Point Velocity { get; set; } = velocity;

    public ParticleSpawn Sample(System.Random random)
    {
        float t = (float)random.NextDouble();
        Point position = new(
            Start.X + (End.X - Start.X) * t,
            Start.Y + (End.Y - Start.Y) * t);
        return new ParticleSpawn(position, Velocity);
    }
}

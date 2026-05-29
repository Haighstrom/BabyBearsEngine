using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// Emits particles uniformly from anywhere inside the rectangle <see cref="Area"/>, all sharing
/// the same <see cref="Velocity"/>. Use for snowfall over a region, ambient sparkles, smoke
/// rising from a fire area.
/// </summary>
/// <param name="area">Local-space rectangle that bounds the spawn area.</param>
/// <param name="velocity">Initial velocity for every emitted particle, in units per second.</param>
public sealed class RectEmitterShape(Rect area, Point velocity) : IEmitterShape
{
    public Rect Area { get; set; } = area;

    public Point Velocity { get; set; } = velocity;

    public ParticleSpawn Sample(System.Random random)
    {
        Point position = new(
            Area.X + (float)random.NextDouble() * Area.W,
            Area.Y + (float)random.NextDouble() * Area.H);
        return new ParticleSpawn(position, Velocity);
    }
}

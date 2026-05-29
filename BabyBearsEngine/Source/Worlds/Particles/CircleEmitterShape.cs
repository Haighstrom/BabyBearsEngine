using System;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// Emits particles from a circle centred on <see cref="Centre"/> with the given
/// <see cref="Radius"/>. Each particle is spawned on the perimeter (or anywhere inside, with
/// <see cref="EmitFromInterior"/> = true) and launched outward along the local radial direction
/// at <see cref="Speed"/>. Use for explosions, ring blasts, ambient swirls.
/// </summary>
/// <param name="centre">Local-space centre of the circle.</param>
/// <param name="radius">Circle radius. Must be ≥ 0.</param>
/// <param name="speed">Magnitude of the outward velocity applied to each particle, in units per second.</param>
public sealed class CircleEmitterShape(Point centre, float radius, float speed) : IEmitterShape
{
    public Point Centre { get; set; } = centre;

    public float Radius { get; set; } = radius;

    public float Speed { get; set; } = speed;

    /// <summary>
    /// When true, particles spawn uniformly anywhere inside the disk; when false (the default)
    /// they spawn on the perimeter. Either way the velocity is the outward radial direction.
    /// </summary>
    public bool EmitFromInterior { get; set; } = false;

    public ParticleSpawn Sample(System.Random random)
    {
        double angle = random.NextDouble() * Math.PI * 2;
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);

        // For a uniform disk sample, distance from the centre is sqrt(u) * radius (u uniform in [0,1]).
        // For a perimeter sample, the distance is exactly the radius.
        float distance = EmitFromInterior
            ? Radius * (float)Math.Sqrt(random.NextDouble())
            : Radius;

        Point position = new(Centre.X + distance * cos, Centre.Y + distance * sin);
        Point velocity = new(cos * Speed, sin * Speed);
        return new ParticleSpawn(position, velocity);
    }
}

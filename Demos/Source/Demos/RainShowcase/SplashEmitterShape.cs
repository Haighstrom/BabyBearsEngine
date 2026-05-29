using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Particles;

namespace BabyBearsEngine.Demos.Source.Demos.RainShowcase;

/// <summary>
/// Emits particles from a single mutable <see cref="Centre"/> with velocities sampled in an
/// upward fan — angle uniform in <c>[30°, 150°]</c> measured from the positive X axis, speed
/// uniform in <c>[MinSpeed, MaxSpeed]</c>. Used by <see cref="SplashSpawner"/> to drive a
/// shared particle system: the centre is mutated per splash, then a small burst is emitted,
/// then the centre moves to the next splash location. Already-in-flight particles don't notice
/// because they carry their spawn position with them.
/// </summary>
internal sealed class SplashEmitterShape(float minSpeed, float maxSpeed) : IEmitterShape
{
    public Point Centre { get; set; } = Point.Zero;

    public float MinSpeed { get; } = minSpeed;

    public float MaxSpeed { get; } = maxSpeed;

    public ParticleSpawn Sample(Random random)
    {
        double angleRad = (30 + random.NextDouble() * 120) * Math.PI / 180;
        float speed = MinSpeed + (float)random.NextDouble() * (MaxSpeed - MinSpeed);

        // Screen Y is down, so the math-convention upward sine becomes a negative screen-space
        // Y component. cos picks up the lateral spread.
        Point velocity = new(
            (float)(Math.Cos(angleRad) * speed),
            (float)(-Math.Sin(angleRad) * speed));

        return new ParticleSpawn(Centre, velocity);
    }
}

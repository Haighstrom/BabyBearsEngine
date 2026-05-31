using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// Emits all particles from a single point with a fixed velocity — the simplest emitter.
/// Use for jets, sparks from a fixed origin, etc. Position is in the owning
/// <see cref="ParticleSystem"/>'s local space.
/// </summary>
/// <param name="origin">Local-space position to emit every particle from.</param>
/// <param name="velocity">Initial velocity for every emitted particle, in units per second.</param>
public sealed class PointEmitterShape(Point origin, Point velocity) : IEmitterShape
{
    public Point Origin { get; set; } = origin;

    public Point Velocity { get; set; } = velocity;

    public ParticleSpawn Sample(IRandom random) => new(Origin, Velocity);
}

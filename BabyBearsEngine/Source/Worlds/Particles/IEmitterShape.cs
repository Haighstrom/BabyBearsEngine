namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// Strategy for sampling a single particle's spawn position and initial velocity. The owning
/// <see cref="ParticleSystem"/> calls <see cref="Sample"/> once per particle emitted; concrete
/// implementations cover the standard geometric shapes — <see cref="PointEmitterShape"/>,
/// <see cref="CircleEmitterShape"/>, <see cref="LineSegmentEmitterShape"/>,
/// <see cref="RectEmitterShape"/> — and a game can provide its own for bespoke effects.
/// Implementations must be stateless or thread-safe with respect to the engine update loop.
/// </summary>
public interface IEmitterShape
{
    /// <summary>
    /// Samples one spawn position and velocity. Called once per particle by the owning
    /// <see cref="ParticleSystem"/>. Implementations should use the supplied <paramref name="random"/>
    /// rather than a private instance so the system stays deterministic when seeded.
    /// </summary>
    ParticleSpawn Sample(IRandom random);
}

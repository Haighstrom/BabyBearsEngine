using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// The data an <see cref="IEmitterShape"/> samples for one new particle: the local-space
/// position to spawn at and the initial velocity to launch with. Lifetime, size and colour are
/// taken from the owning <see cref="ParticleSystem"/> so a shape can be shared across systems
/// with different per-particle visual parameters.
/// </summary>
public readonly record struct ParticleSpawn(Point Position, Point Velocity);

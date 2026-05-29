using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// A single particle in flight. Position is in the parent <see cref="ParticleSystem"/>'s local
/// space; velocity is in units per second. <see cref="StartColour"/> and <see cref="StartSize"/>
/// are sampled at spawn time and combined with the system's per-frame colour-over-life and
/// size-over-life functions to compute the current visual values without mutating the spawn
/// state — so the over-life functions can be swapped between frames and still produce sensible
/// results for particles already in flight.
/// </summary>
public struct Particle
{
    public Point Position;
    public Point Velocity;
    public Colour StartColour;
    public float StartSize;
    public float TotalLifetime;
    public float RemainingLifetime;
}

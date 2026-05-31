using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Particles;

namespace BabyBearsEngine.Demos.Source.Demos.RainShowcase;

/// <summary>
/// Drives <see cref="ArcEmitterShape"/>-backed bursts on a shared particle system at random
/// positions sampled from a pre-built candidate list (weighted by the splash-location mask).
/// <see cref="Intensity"/> in [0, 1] scales the splash rate linearly between zero and
/// <see cref="MaxSplashesPerSecond"/>; intensity 0 fully halts new splashes (existing particles
/// continue to age).
/// </summary>
internal sealed class SplashSpawner(
    ParticleSystem system,
    ArcEmitterShape shape,
    IReadOnlyList<Point> candidatePositions,
    IRandom random) : UpdateableBase
{
    private const float MaxSplashesPerSecond = 50f;
    private const int MinParticlesPerSplash = 3;
    private const int MaxParticlesPerSplash = 6;
    private const float JitterPixels = 2f;

    private double _spawnAccumulator = 0.0;

    public float Intensity { get; set; } = 1f;

    public override void Update(double elapsed)
    {
        if (Intensity <= 0.001f || candidatePositions.Count == 0)
        {
            _spawnAccumulator = 0.0;
            return;
        }

        double splashesPerSecond = MaxSplashesPerSecond * Intensity;
        _spawnAccumulator += elapsed * splashesPerSecond;

        while (_spawnAccumulator >= 1.0)
        {
            EmitOneSplash();
            _spawnAccumulator -= 1.0;
        }
    }

    private void EmitOneSplash()
    {
        Point candidate = random.Choose(candidatePositions);
        float jitterX = random.Float(-JitterPixels, JitterPixels);
        float jitterY = random.Float(-JitterPixels, JitterPixels);
        shape.Origin = new Point(candidate.X + jitterX, candidate.Y + jitterY);

        int particles = random.Int(MinParticlesPerSplash, MaxParticlesPerSplash + 1);
        system.EmitBurst(particles);
    }
}

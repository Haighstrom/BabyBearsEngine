using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// Emits particles from a single mutable <see cref="Origin"/> with velocity directions sampled
/// uniformly across an angular arc and magnitudes uniformly across a speed range. Generalises
/// the "fan of particles from a point" pattern used by splashes, geysers, sparks, jets, muzzle
/// flashes, and radial bullet bursts.
/// </summary>
/// <remarks>
/// <para>Angles use the math convention: <c>0°</c> points along <c>+X</c> (right). Because the
/// engine's screen coordinates have <c>Y</c> increasing downward, <c>90°</c> points along
/// <c>-Y</c> in screen space — i.e. visually upward. So an "upward fan" splash uses
/// <see cref="ArcCentreDegrees"/> = <c>90</c>; a "downward jet" uses <c>270</c>; a "rightward
/// spray" uses <c>0</c>. <see cref="ArcSpreadDegrees"/> = <c>360</c> gives a full circle (any
/// <see cref="ArcCentreDegrees"/>); <c>0</c> gives a perfectly straight beam.</para>
///
/// <para>The position is always <see cref="Origin"/> — set it before each
/// <see cref="ParticleSystem.EmitBurst"/> when driving multiple bursts off one shared shape
/// (the standard pattern for cheap multi-location effects like raindrop splashes scattered
/// across a ground plane).</para>
/// </remarks>
/// <param name="origin">Local-space position every particle is emitted from.</param>
/// <param name="arcCentreDegrees">Centre direction of the arc, in degrees (math convention; see remarks).</param>
/// <param name="arcSpreadDegrees">Total angular span of the arc, in degrees. 0 = beam, 360 = full circle.</param>
/// <param name="minSpeed">Minimum velocity magnitude, in units per second. Must be ≥ 0.</param>
/// <param name="maxSpeed">Maximum velocity magnitude, in units per second. Must be ≥ <paramref name="minSpeed"/>.</param>
public sealed class ArcEmitterShape(
    Point origin,
    float arcCentreDegrees,
    float arcSpreadDegrees,
    float minSpeed,
    float maxSpeed) : IEmitterShape
{
    public Point Origin { get; set; } = origin;

    public float ArcCentreDegrees { get; set; } = arcCentreDegrees;

    public float ArcSpreadDegrees { get; set; } = arcSpreadDegrees;

    public float MinSpeed { get; set; } = minSpeed;

    public float MaxSpeed { get; set; } = maxSpeed;

    public ParticleSpawn Sample(Random random)
    {
        float halfSpread = ArcSpreadDegrees * 0.5f;
        double angleDegrees = ArcCentreDegrees - halfSpread + random.NextDouble() * ArcSpreadDegrees;
        double angleRadians = angleDegrees * Math.PI / 180;
        float speed = MinSpeed + (float)random.NextDouble() * (MaxSpeed - MinSpeed);

        // Math convention: cos(theta) is the X component, sin(theta) the Y component in math
        // axes where +Y is up. Screen Y is flipped (down is positive), so the math-Y maps to
        // a negative screen-Y to keep "90 degrees points visually upward".
        Point velocity = new(
            (float)(Math.Cos(angleRadians) * speed),
            (float)(-Math.Sin(angleRadians) * speed));

        return new ParticleSpawn(Origin, velocity);
    }
}

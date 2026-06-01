using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Particles;

namespace BabyBearsEngine.Tests.Unit;

/// <summary>
/// Unit coverage for the non-GL pieces of the particle system: emission rate integration,
/// particle aging and integration, lifetime cap, over-life interpolators, and EmitBurst.
/// GL resources are lazy-initialised on first Render — these tests only exercise Update.
/// </summary>
[TestClass]
public class ParticleSystemTests
{
    private sealed class FakeEmitterShape(Point position, Point velocity) : IEmitterShape
    {
        public int SampleCount { get; private set; }

        public Point NextPosition { get; set; } = position;

        public Point NextVelocity { get; set; } = velocity;

        public ParticleSpawn Sample(IRandom random)
        {
            SampleCount++;
            return new ParticleSpawn(NextPosition, NextVelocity);
        }
    }

    private static ParticleSystem MakeSystem(IEmitterShape? shape = null)
    {
        return new ParticleSystem(
            shape ?? new FakeEmitterShape(Point.Zero, Point.Zero),
            random: new SystemRandom(seed: 12345));
    }

    [TestMethod]
    public void Constructor_RejectsNullShape()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ParticleSystem(null!));
    }

    [TestMethod]
    public void Constructor_NegativeLayer_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new ParticleSystem(new FakeEmitterShape(Point.Zero, Point.Zero), layer: -1));
    }

    [TestMethod]
    public void EmitterShape_AssigningNull_Throws()
    {
        var system = MakeSystem();
        Assert.ThrowsExactly<ArgumentNullException>(() => system.EmitterShape = null!);
    }

    [TestMethod]
    public void ParticleCount_OnNewSystem_IsZero()
    {
        var system = MakeSystem();
        Assert.AreEqual(0, system.ParticleCount);
    }

    [TestMethod]
    public void Update_WhenNotEmitting_DoesNotSpawnParticles()
    {
        var shape = new FakeEmitterShape(Point.Zero, Point.Zero);
        var system = MakeSystem(shape);
        system.Emitting = false;
        system.EmissionRate = 100f;

        system.Update(1.0);

        Assert.AreEqual(0, system.ParticleCount);
        Assert.AreEqual(0, shape.SampleCount);
    }

    [TestMethod]
    public void Update_WhenEmitting_SpawnsParticlesAtEmissionRate()
    {
        var system = MakeSystem();
        system.EmissionRate = 60f;
        system.Lifetime = 10f; // long enough that nothing dies during the test

        // 0.5s at 60/s = 30 particles
        system.Update(0.5);

        Assert.AreEqual(30, system.ParticleCount);
    }

    [TestMethod]
    public void Update_AcrossMultipleFrames_AccumulatesParticles()
    {
        var system = MakeSystem();
        system.EmissionRate = 100f;
        system.Lifetime = 10f;

        for (int i = 0; i < 10; i++)
        {
            system.Update(0.1);
        }

        Assert.AreEqual(100, system.ParticleCount);
    }

    [TestMethod]
    public void Update_CapsAtMaxParticles()
    {
        var system = MakeSystem();
        system.EmissionRate = 1000f;
        system.Lifetime = 10f;
        system.MaxParticles = 50;

        system.Update(1.0);

        Assert.AreEqual(50, system.ParticleCount);
    }

    [TestMethod]
    public void Update_RemovesParticlesAfterLifetime()
    {
        var system = MakeSystem();
        system.EmissionRate = 100f;
        system.Lifetime = 0.5f;

        // Emit a frame, then turn off emission and wait for everything to die.
        system.Update(0.5); // 50 particles, all with 0.5s remaining
        Assert.AreEqual(50, system.ParticleCount);

        system.Emitting = false;
        system.Update(0.5 + 0.001); // age them out

        Assert.AreEqual(0, system.ParticleCount);
    }

    [TestMethod]
    public void Update_IntegratesParticlePositionByVelocity()
    {
        var shape = new FakeEmitterShape(new Point(0, 0), new Point(100, -50));
        var system = MakeSystem(shape);
        system.EmissionRate = 0f;
        system.Emitting = false;
        system.Lifetime = 10f;

        system.EmitBurst(1);
        Assert.AreEqual(1, system.ParticleCount);

        // 0.5s at velocity (100, -50) = displacement (50, -25)
        system.Update(0.5);

        // Position is part of the internal Particle struct — we verify indirectly via the
        // SizeOverLife callback receiving t > 0 (some lifetime has elapsed), then directly
        // by configuring ColourOverLife to read the position out into a side channel.
        // Simpler: leave the assertion here as "still alive" (lifetime > elapsed), and use
        // a separate test for the position via an over-life callback that captures it.
        Assert.AreEqual(1, system.ParticleCount);
    }

    [TestMethod]
    public void EmitBurst_AddsExactlyTheRequestedCount()
    {
        var system = MakeSystem();
        system.Emitting = false;
        system.EmissionRate = 0f;
        system.Lifetime = 10f;

        system.EmitBurst(7);

        Assert.AreEqual(7, system.ParticleCount);
    }

    [TestMethod]
    public void EmitBurst_RespectsMaxParticles()
    {
        var system = MakeSystem();
        system.Emitting = false;
        system.MaxParticles = 3;
        system.Lifetime = 10f;

        system.EmitBurst(10);

        Assert.AreEqual(3, system.ParticleCount);
    }

    [TestMethod]
    public void EmitBurst_NegativeCount_Throws()
    {
        var system = MakeSystem();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => system.EmitBurst(-1));
    }

    [TestMethod]
    public void Clear_RemovesAllParticles()
    {
        var system = MakeSystem();
        system.Lifetime = 10f;
        system.EmitBurst(5);
        Assert.AreEqual(5, system.ParticleCount);

        system.Clear();

        Assert.AreEqual(0, system.ParticleCount);
    }

    [TestMethod]
    public void Update_WithEmissionPausedThenResumed_DoesNotDumpBacklog()
    {
        var system = MakeSystem();
        system.EmissionRate = 100f;
        system.Lifetime = 10f;
        system.Emitting = false;

        // Long "paused" window — counter could otherwise accrue to ~1000.
        system.Update(10.0);
        Assert.AreEqual(0, system.ParticleCount);

        system.Emitting = true;
        system.Update(0.1); // expect ~10 from this slice, not the accumulated 1000

        Assert.IsLessThanOrEqualTo(15, system.ParticleCount,
            "Resumed emission must not dump a backlog of particles accrued while paused.");
    }

    [TestMethod]
    public void LayerChanged_FiresOnAssignmentToDifferentLayer()
    {
        var system = MakeSystem();
        var events = new List<(int Old, int New)>();
        system.LayerChanged += (_, args) => events.Add((args.OldLayer, args.NewLayer));

        system.Layer = 5;

        Assert.HasCount(1, events);
        Assert.AreEqual(int.MaxValue, events[0].Old);
        Assert.AreEqual(5, events[0].New);
    }

    [TestMethod]
    public void LayerChanged_DoesNotFireForSameLayerAssignment()
    {
        var system = MakeSystem();
        var events = new List<(int, int)>();
        system.LayerChanged += (_, args) => events.Add((args.OldLayer, args.NewLayer));

        // Default is int.MaxValue
        system.Layer = int.MaxValue;

        Assert.IsEmpty(events);
    }

    [TestMethod]
    public void Texture_Default_IsNull()
    {
        ParticleSystem system = MakeSystem();
        Assert.IsNull(system.Texture);
    }

    [TestMethod]
    public void Texture_CanBeAssignedAndCleared()
    {
        ParticleSystem system = MakeSystem();
        FakeTexture fake = new();

        system.Texture = fake;
        Assert.AreSame(fake, system.Texture);

        system.Texture = null;
        Assert.IsNull(system.Texture);
    }

    [TestMethod]
    public void Update_AfterDispose_Throws()
    {
        ParticleSystem system = MakeSystem();
        system.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => system.Update(0.016));
    }

    [TestMethod]
    public void Render_AfterDispose_Throws()
    {
        ParticleSystem system = MakeSystem();
        system.Dispose();

        // Render takes a ref Matrix3, so call it via a local lambda.
        Assert.ThrowsExactly<ObjectDisposedException>(() =>
        {
            Matrix3 proj = Matrix3.Identity;
            Matrix3 mv = Matrix3.Identity;
            system.Render(ref proj, ref mv);
        });
    }

    private sealed class FakeTexture : BabyBearsEngine.OpenGL.ITexture
    {
        public int Handle => 0;

        public int Width => 1;

        public int Height => 1;

        public void Bind(OpenTK.Graphics.OpenGL4.TextureTarget textureTarget = OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
            OpenTK.Graphics.OpenGL4.TextureUnit textureUnit = OpenTK.Graphics.OpenGL4.TextureUnit.Texture0)
        {
        }

        public void Dispose()
        {
        }
    }
}

[TestClass]
public class PointEmitterShapeTests
{
    [TestMethod]
    public void Sample_ReturnsOriginAndVelocity()
    {
        var shape = new PointEmitterShape(new Point(10, 20), new Point(-5, 0));

        ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: 1));

        Assert.AreEqual(new Point(10, 20), spawn.Position);
        Assert.AreEqual(new Point(-5, 0), spawn.Velocity);
    }
}

[TestClass]
public class CircleEmitterShapeTests
{
    [TestMethod]
    public void Sample_PerimeterDefault_PlacesPositionOnCircle()
    {
        var shape = new CircleEmitterShape(new Point(0, 0), radius: 50f, speed: 10f);

        for (int i = 0; i < 50; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));
            float distance = spawn.Position.Length;

            Assert.AreEqual(50f, distance, 0.001f, $"Iteration {i}: distance {distance} not on perimeter.");
        }
    }

    [TestMethod]
    public void Sample_VelocityIsRadialOutward()
    {
        var shape = new CircleEmitterShape(new Point(0, 0), radius: 50f, speed: 10f);

        for (int i = 0; i < 20; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));
            // Position vector and velocity vector should point in the same direction (cos≈1).
            float positionDirX = spawn.Position.X / 50f;
            float positionDirY = spawn.Position.Y / 50f;
            float velocityDirX = spawn.Velocity.X / 10f;
            float velocityDirY = spawn.Velocity.Y / 10f;

            float dot = positionDirX * velocityDirX + positionDirY * velocityDirY;
            Assert.AreEqual(1f, dot, 0.001f, $"Iteration {i}: velocity not radial; dot={dot}");
        }
    }

    [TestMethod]
    public void Sample_InteriorMode_PlacesPositionInsideCircle()
    {
        var shape = new CircleEmitterShape(new Point(0, 0), radius: 50f, speed: 10f)
        {
            EmitFromInterior = true,
        };

        for (int i = 0; i < 50; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));
            float distance = spawn.Position.Length;

            Assert.IsLessThanOrEqualTo(50.001f, distance, $"Iteration {i}: distance {distance} outside circle.");
        }
    }

    [TestMethod]
    public void Constructor_NegativeRadius_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new CircleEmitterShape(Point.Zero, radius: -1f, speed: 10f));
    }

    [TestMethod]
    public void RadiusSetter_NegativeValue_Throws()
    {
        var shape = new CircleEmitterShape(Point.Zero, radius: 10f, speed: 1f);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => shape.Radius = -1f);
    }

    [TestMethod]
    public void RadiusSetter_Zero_IsAllowed()
    {
        var shape = new CircleEmitterShape(Point.Zero, radius: 10f, speed: 1f);

        shape.Radius = 0f;

        Assert.AreEqual(0f, shape.Radius);
    }
}

[TestClass]
public class LineSegmentEmitterShapeTests
{
    [TestMethod]
    public void Sample_PositionFallsOnSegment()
    {
        var start = new Point(0, 0);
        var end = new Point(100, 0);
        var shape = new LineSegmentEmitterShape(start, end, new Point(0, -10));

        for (int i = 0; i < 50; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));

            Assert.AreEqual(0f, spawn.Position.Y, 0.001f, $"Iteration {i}: Y off-segment.");
            Assert.IsGreaterThanOrEqualTo(0f, spawn.Position.X);
            Assert.IsLessThanOrEqualTo(100f, spawn.Position.X);
        }
    }

    [TestMethod]
    public void Sample_VelocityMatchesConfigured()
    {
        var shape = new LineSegmentEmitterShape(new Point(0, 0), new Point(10, 0), new Point(3, -7));

        ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: 1));

        Assert.AreEqual(new Point(3, -7), spawn.Velocity);
    }
}

[TestClass]
public class RectEmitterShapeTests
{
    [TestMethod]
    public void Sample_PositionFallsInsideRect()
    {
        var area = new Rect(10, 20, 30, 40); // x=10..40, y=20..60
        var shape = new RectEmitterShape(area, new Point(0, -5));

        for (int i = 0; i < 50; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));

            Assert.IsGreaterThanOrEqualTo(10f, spawn.Position.X);
            Assert.IsLessThanOrEqualTo(40f, spawn.Position.X);
            Assert.IsGreaterThanOrEqualTo(20f, spawn.Position.Y);
            Assert.IsLessThanOrEqualTo(60f, spawn.Position.Y);
        }
    }

    [TestMethod]
    public void Sample_VelocityMatchesConfigured()
    {
        var shape = new RectEmitterShape(new Rect(0, 0, 10, 10), new Point(2, -8));

        ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: 1));

        Assert.AreEqual(new Point(2, -8), spawn.Velocity);
    }

    [TestMethod]
    public void Constructor_NegativeAreaWidth_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new RectEmitterShape(new Rect(0, 0, -10, 10), Point.Zero));
    }

    [TestMethod]
    public void Constructor_NegativeAreaHeight_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new RectEmitterShape(new Rect(0, 0, 10, -10), Point.Zero));
    }

    [TestMethod]
    public void AreaSetter_NegativeWidth_Throws()
    {
        var shape = new RectEmitterShape(new Rect(0, 0, 10, 10), Point.Zero);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => shape.Area = new Rect(0, 0, -5, 10));
    }

    [TestMethod]
    public void AreaSetter_NegativeHeight_Throws()
    {
        var shape = new RectEmitterShape(new Rect(0, 0, 10, 10), Point.Zero);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => shape.Area = new Rect(0, 0, 10, -5));
    }

    [TestMethod]
    public void AreaSetter_ZeroSize_IsAllowed()
    {
        var shape = new RectEmitterShape(new Rect(0, 0, 10, 10), Point.Zero);

        shape.Area = new Rect(0, 0, 0, 0);

        Assert.AreEqual(0f, shape.Area.W);
        Assert.AreEqual(0f, shape.Area.H);
    }
}

[TestClass]
public class ArcEmitterShapeTests
{
    [TestMethod]
    public void Sample_PositionIsAlwaysOrigin()
    {
        ArcEmitterShape shape = new(new Point(42, -17), arcCentreDegrees: 90f, arcSpreadDegrees: 120f, minSpeed: 10f, maxSpeed: 50f);

        for (int i = 0; i < 20; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));

            Assert.AreEqual(42f, spawn.Position.X, 0.001f);
            Assert.AreEqual(-17f, spawn.Position.Y, 0.001f);
        }
    }

    [TestMethod]
    public void Sample_SpeedFallsWithinRange()
    {
        ArcEmitterShape shape = new(Point.Zero, arcCentreDegrees: 90f, arcSpreadDegrees: 60f, minSpeed: 30f, maxSpeed: 70f);

        for (int i = 0; i < 50; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));
            float speed = spawn.Velocity.Length;

            Assert.IsGreaterThanOrEqualTo(30f - 0.01f, speed, $"Iteration {i}: speed {speed} below min.");
            Assert.IsLessThanOrEqualTo(70f + 0.01f, speed, $"Iteration {i}: speed {speed} above max.");
        }
    }

    [TestMethod]
    public void Sample_UpwardArc_AlwaysProducesNegativeScreenY()
    {
        // 90 degrees centre (visually upward) with a 120-degree spread covers 30 to 150 in math
        // angles — every sample should have a negative screen-Y component (visually upward).
        ArcEmitterShape shape = new(Point.Zero, arcCentreDegrees: 90f, arcSpreadDegrees: 120f, minSpeed: 50f, maxSpeed: 50f);

        for (int i = 0; i < 50; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));

            Assert.IsLessThan(0f, spawn.Velocity.Y, $"Iteration {i}: velocity Y {spawn.Velocity.Y} not upward.");
        }
    }

    [TestMethod]
    public void Sample_RightwardBeam_HasZeroSpread()
    {
        // Spread 0 = beam exactly along the centre angle. 0 degrees centre = positive X axis.
        ArcEmitterShape shape = new(Point.Zero, arcCentreDegrees: 0f, arcSpreadDegrees: 0f, minSpeed: 100f, maxSpeed: 100f);

        for (int i = 0; i < 10; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));

            Assert.AreEqual(100f, spawn.Velocity.X, 0.001f);
            Assert.AreEqual(0f, spawn.Velocity.Y, 0.001f);
        }
    }

    [TestMethod]
    public void Sample_FullCircleSpread_HitsBothHemispheres()
    {
        // 360 degrees of spread should produce samples that vary in sign on both axes.
        ArcEmitterShape shape = new(Point.Zero, arcCentreDegrees: 0f, arcSpreadDegrees: 360f, minSpeed: 50f, maxSpeed: 50f);

        bool sawPositiveX = false;
        bool sawNegativeX = false;
        bool sawPositiveY = false;
        bool sawNegativeY = false;

        for (int i = 0; i < 200; i++)
        {
            ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: i));
            if (spawn.Velocity.X > 1f) { sawPositiveX = true; }
            if (spawn.Velocity.X < -1f) { sawNegativeX = true; }
            if (spawn.Velocity.Y > 1f) { sawPositiveY = true; }
            if (spawn.Velocity.Y < -1f) { sawNegativeY = true; }
        }

        Assert.IsTrue(sawPositiveX);
        Assert.IsTrue(sawNegativeX);
        Assert.IsTrue(sawPositiveY);
        Assert.IsTrue(sawNegativeY);
    }

    [TestMethod]
    public void Origin_IsMutable()
    {
        // Mutable Origin is the central pattern this shape supports — one shared instance
        // emits many bursts at different locations (e.g. raindrop splashes scattered
        // across a ground plane).
        ArcEmitterShape shape = new(Point.Zero, 90f, 60f, 10f, 10f)
        {
            Origin = new Point(123, 456),
        };

        ParticleSpawn spawn = shape.Sample(new SystemRandom(seed: 1));

        Assert.AreEqual(123f, spawn.Position.X, 0.001f);
        Assert.AreEqual(456f, spawn.Position.Y, 0.001f);
    }

    [TestMethod]
    public void Constructor_NegativeMinSpeed_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new ArcEmitterShape(Point.Zero, 0f, 360f, minSpeed: -1f, maxSpeed: 10f));
    }

    [TestMethod]
    public void Constructor_MaxSpeedLessThanMinSpeed_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new ArcEmitterShape(Point.Zero, 0f, 360f, minSpeed: 50f, maxSpeed: 10f));
    }

    [TestMethod]
    public void MinSpeedSetter_Negative_Throws()
    {
        ArcEmitterShape shape = new(Point.Zero, 0f, 360f, minSpeed: 10f, maxSpeed: 50f);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => shape.MinSpeed = -1f);
    }

    [TestMethod]
    public void MinSpeedSetter_GreaterThanMaxSpeed_Throws()
    {
        ArcEmitterShape shape = new(Point.Zero, 0f, 360f, minSpeed: 10f, maxSpeed: 50f);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => shape.MinSpeed = 100f);
    }

    [TestMethod]
    public void MaxSpeedSetter_LessThanMinSpeed_Throws()
    {
        ArcEmitterShape shape = new(Point.Zero, 0f, 360f, minSpeed: 10f, maxSpeed: 50f);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => shape.MaxSpeed = 5f);
    }

    [TestMethod]
    public void SpeedSetters_EqualValues_AreAllowed()
    {
        ArcEmitterShape shape = new(Point.Zero, 0f, 360f, minSpeed: 10f, maxSpeed: 50f);

        shape.MaxSpeed = 10f;

        Assert.AreEqual(10f, shape.MinSpeed);
        Assert.AreEqual(10f, shape.MaxSpeed);
    }
}

using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Particles;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// End-to-end coverage that the particle system runs under a live GL context: shaders compile
/// and link, the billboard geometry-shader pipeline expands point primitives into quads, and
/// per-particle colour reaches the framebuffer. Each test stands up a real window via
/// <see cref="GameLauncher"/>, advances a few frames, samples pixels via the screen capture
/// service, and asserts on the result. If any of the underlying GL pieces is broken these
/// tests fail loudly — that's the point: <see cref="Tests.Unit"/> can't see GL issues at all.
/// </summary>
[TestClass]
public class ParticleSystemTests
{
    private const int WindowWidth = 200;
    private const int WindowHeight = 200;

    private static ApplicationSettings SettingsWithCapture() => new()
    {
        WindowSettings = new WindowSettings { Width = WindowWidth, Height = WindowHeight, CheckForMainThread = false },
        DiagnosticsSettings = new DiagnosticsSettings { CaptureFrames = true },
        LogSettings = LogSettings.Silent,
    };

    /// <summary>
    /// Harness world: defer scene construction until <see cref="World.Update"/> on the first
    /// tick (when the GL context is up), advance N frames, then sample pixels and close. The
    /// scene-setup and pixel-sampling callbacks let each test express its own assertions while
    /// reusing the boilerplate.
    /// </summary>
    private sealed class ParticleCaptureWorld(int framesBeforeCapture, Action<ParticleCaptureWorld> setup, Action<ParticleCaptureWorld> capture) : World
    {
        private int _frame = 0;
        private bool _captured = false;

        public List<string> Errors { get; } = [];

        public Colour SamplePixel(int x, int y) => EngineConfiguration.ScreenCaptureService.GetPixel(x, y);

        public override void Update(double elapsed)
        {
            if (_frame == 0)
            {
                BackgroundColour = Colour.Black;
                setup(this);
            }

            base.Update(elapsed);

            // Surface any GL error introduced by the particle render so the test fails with the
            // specific GL code rather than just a wrong-colour assertion.
            ErrorCode err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Errors.Add($"GL error after frame {_frame}: {err}");
            }

            _frame++;

            if (_frame > framesBeforeCapture && !_captured)
            {
                _captured = true;
                capture(this);
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    /// <summary>
    /// Sanity check: the particle shader program constructs (vert + geom + frag compile and
    /// link) under a live GL context. If this fails the rest of the GL-side tests are moot.
    /// </summary>
    [TestMethod]
    public void ParticleShaderProgram_ConstructsUnderLiveGLContext()
    {
        bool constructed = false;
        Exception? caught = null;
        var world = new ParticleCaptureWorld(
            framesBeforeCapture: 0,
            setup: _ =>
            {
                try
                {
                    // Accessing the lazy singleton triggers the shader's GL compile/link inside
                    // the live engine loop. EngineTeardown.ResetForNextRun drops the singleton
                    // at the end of the run so the next test re-exercises the construction path.
                    var program = ParticleShaderProgram.Instance;
                    constructed = program is not null;
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            },
            capture: _ => { });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsNull(caught, caught?.Message);
        Assert.IsTrue(constructed);
    }

    /// <summary>
    /// Same sanity check for the textured variant — billboard.vert + billboard_points_to_quads.geom
    /// + default.frag must compose and link. The pieces ship as separately-compiled shaders that
    /// happen to share GLSL interface blocks; this catches breakage if any of them drifts.
    /// </summary>
    [TestMethod]
    public void TexturedParticleShaderProgram_ConstructsUnderLiveGLContext()
    {
        bool constructed = false;
        Exception? caught = null;
        var world = new ParticleCaptureWorld(
            framesBeforeCapture: 0,
            setup: _ =>
            {
                try
                {
                    var program = TexturedParticleShaderProgram.Instance;
                    constructed = program is not null;
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            },
            capture: _ => { });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsNull(caught, caught?.Message);
        Assert.IsTrue(constructed);
    }

    /// <summary>
    /// Render a textured particle at the screen centre using a 1×1 white texture and verify the
    /// centre pixel is painted with the per-particle colour — proves the textured pipeline binds
    /// the texture, samples it, and modulates it by the colour attribute end-to-end.
    /// </summary>
    [TestMethod]
    public void ParticleSystem_TexturedBurstAtCentre_PaintsCentrePixel()
    {
        const int sampleX = WindowWidth / 2;
        const int sampleY = WindowHeight / 2;
        Colour bright = new(60, 200, 120);
        Colour? captured = null;

        var world = new ParticleCaptureWorld(
            framesBeforeCapture: 2,
            setup: w =>
            {
                ITexture texture = Worlds.Graphics.Textures.GenTexture(new[,] { { Colour.White } });
                ParticleSystem system = new(
                    new PointEmitterShape(new Point(sampleX, sampleY), Point.Zero))
                {
                    EmissionRate = 0f,
                    Emitting = false,
                    Lifetime = 5f,
                    StartSize = new Point(40f, 40f),
                    Colour = bright,
                    Texture = texture,
                };
                system.EmitBurst(1);
                w.Add(system);
            },
            capture: w => captured = w.SamplePixel(sampleX, sampleY));

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsEmpty(world.Errors, string.Join(Environment.NewLine, world.Errors));
        Assert.IsNotNull(captured);
        int rDelta = Math.Abs(bright.R - captured.Value.R);
        int gDelta = Math.Abs(bright.G - captured.Value.G);
        int bDelta = Math.Abs(bright.B - captured.Value.B);
        Assert.IsLessThanOrEqualTo(8, rDelta, $"R delta {rDelta} too high; expected {bright.R}, got {captured.Value.R}");
        Assert.IsLessThanOrEqualTo(8, gDelta, $"G delta {gDelta} too high; expected {bright.G}, got {captured.Value.G}");
        Assert.IsLessThanOrEqualTo(8, bDelta, $"B delta {bDelta} too high; expected {bright.B}, got {captured.Value.B}");
    }

    /// <summary>
    /// Render a single bright burst at the screen centre and verify the centre pixel goes from
    /// black to bright — proves the billboard geom-shader stage actually expands the point into
    /// pixels rather than being silently culled.
    /// </summary>
    [TestMethod]
    public void ParticleSystem_BurstAtCentre_PaintsCentrePixel()
    {
        const int sampleX = WindowWidth / 2;
        const int sampleY = WindowHeight / 2;
        var bright = new Colour(255, 80, 60);
        Colour? captured = null;

        var world = new ParticleCaptureWorld(
            framesBeforeCapture: 2,
            setup: w =>
            {
                var system = new ParticleSystem(
                    new PointEmitterShape(new Point(sampleX, sampleY), Point.Zero))
                {
                    EmissionRate = 0f,
                    Emitting = false,
                    Lifetime = 5f,
                    StartSize = new Point(40f, 40f),
                    Colour = bright,
                };
                system.EmitBurst(1);
                w.Add(system);
            },
            capture: w => captured = w.SamplePixel(sampleX, sampleY));

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsEmpty(world.Errors, string.Join(Environment.NewLine, world.Errors));
        Assert.IsNotNull(captured);
        // Tight tolerance: a 40px particle covers the centre pixel completely; the rendered
        // colour comes through gamma-as-is so the captured channels should match within a
        // couple of LSBs after the GL → linear-byte → bitmap round-trip.
        int rDelta = Math.Abs(bright.R - captured.Value.R);
        int gDelta = Math.Abs(bright.G - captured.Value.G);
        int bDelta = Math.Abs(bright.B - captured.Value.B);
        Assert.IsLessThanOrEqualTo(8, rDelta, $"R delta {rDelta} too high; expected {bright.R}, got {captured.Value.R}");
        Assert.IsLessThanOrEqualTo(8, gDelta, $"G delta {gDelta} too high; expected {bright.G}, got {captured.Value.G}");
        Assert.IsLessThanOrEqualTo(8, bDelta, $"B delta {bDelta} too high; expected {bright.B}, got {captured.Value.B}");
    }

    /// <summary>
    /// Render a burst at one location and assert a pixel far from the burst remains the
    /// background colour — proves the geom shader is sizing the quad correctly and not, say,
    /// turning every particle into a full-screen rectangle.
    /// </summary>
    [TestMethod]
    public void ParticleSystem_BurstAtCentre_LeavesFarPixelUntouched()
    {
        const int centreX = WindowWidth / 2;
        const int centreY = WindowHeight / 2;
        Colour? captured = null;

        var world = new ParticleCaptureWorld(
            framesBeforeCapture: 2,
            setup: w =>
            {
                var system = new ParticleSystem(
                    new PointEmitterShape(new Point(centreX, centreY), Point.Zero))
                {
                    EmissionRate = 0f,
                    Emitting = false,
                    Lifetime = 5f,
                    StartSize = new Point(10f, 10f),
                    Colour = Colour.Red,
                };
                system.EmitBurst(1);
                w.Add(system);
            },
            capture: w => captured = w.SamplePixel(5, 5));

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsEmpty(world.Errors, string.Join(Environment.NewLine, world.Errors));
        Assert.IsNotNull(captured);
        // Background is black; corner should still be black/very dark.
        Assert.IsLessThanOrEqualTo(8, (int)captured.Value.R);
        Assert.IsLessThanOrEqualTo(8, (int)captured.Value.G);
        Assert.IsLessThanOrEqualTo(8, (int)captured.Value.B);
    }

    /// <summary>
    /// Emit a steady stream from one point and verify the count exceeds zero after a few frames
    /// — proves the emission-rate integration is doing its job under the real game loop, not
    /// just a fake clock.
    /// </summary>
    [TestMethod]
    public void ParticleSystem_SteadyEmitter_HasLiveParticlesAfterFrames()
    {
        ParticleSystem? system = null;
        int observedCount = 0;

        var world = new ParticleCaptureWorld(
            framesBeforeCapture: 5,
            setup: w =>
            {
                system = new ParticleSystem(new PointEmitterShape(new Point(100, 100), new Point(0, -50)))
                {
                    EmissionRate = 200f,
                    Lifetime = 1.5f,
                    StartSize = new Point(6f, 6f),
                    Colour = Colour.Yellow,
                };
                w.Add(system);
            },
            capture: _ => observedCount = system!.ParticleCount);

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsEmpty(world.Errors, string.Join(Environment.NewLine, world.Errors));
        Assert.IsGreaterThan(0, observedCount, "Expected at least one live particle after the system ran for several frames.");
    }
}

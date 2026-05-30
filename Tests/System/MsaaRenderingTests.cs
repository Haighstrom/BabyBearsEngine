using System;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// End-to-end MSAA rendering smoke test: run a real frame through the engine with MSAA
/// enabled and disabled, sample the centre pixel via the screen-capture service, and assert
/// the camera produced the expected colour rather than a black frame (which is the most
/// likely failure mode when the MSAA FBO/blit pipeline breaks).
/// </summary>
[TestClass]
public class MsaaRenderingTests
{
    private const int WindowWidth = 200;
    private const int WindowHeight = 100;
    private const int SampleX = WindowWidth / 2;
    private const int SampleY = WindowHeight / 2;

    private sealed class CaptureWorld(Action<CaptureWorld> setup) : World
    {
        private int _frame = 0;

        public Colour Captured { get; private set; }

        public override void Update(double elapsed)
        {
            if (_frame == 0)
            {
                BackgroundColour = Colour.Black;
                setup(this);
            }

            base.Update(elapsed);
            _frame++;

            if (_frame > 1)
            {
                Captured = EngineConfiguration.ScreenCaptureService.GetPixel(SampleX, SampleY);
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    private static ApplicationSettings SettingsWithCapture(MsaaSamples samples) => new()
    {
        WindowSettings = new WindowSettings { Width = WindowWidth, Height = WindowHeight, CheckForMainThread = false },
        DiagnosticsSettings = new DiagnosticsSettings { CaptureFrames = true },
        LogSettings = LogSettings.Silent,
        DefaultCameraMsaa = samples,
    };

    private static void AssertColourClose(Colour expected, Colour actual, int tolerance = 4)
    {
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.R - actual.R), $"R: expected {expected.R}, got {actual.R}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.G - actual.G), $"G: expected {expected.G}, got {actual.G}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.B - actual.B), $"B: expected {expected.B}, got {actual.B}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.A - actual.A), $"A: expected {expected.A}, got {actual.A}");
    }

    [TestMethod]
    public void MsaaDisabled_CameraWithFullScreenColouredRect_RendersExpectedColour()
    {
        var world = new CaptureWorld(w =>
        {
            Camera camera = Camera.WithView(0, 0, WindowWidth, WindowHeight, 0, 0, WindowWidth, WindowHeight, samples: MsaaSamples.Disabled);
            camera.BackgroundColour = Colour.Black;
            camera.Add(new ColourGraphic(Colour.Blue, 0, 0, WindowWidth, WindowHeight));
            w.Add(camera);
        });

        GameLauncher.Run(SettingsWithCapture(MsaaSamples.Disabled), () => world);

        AssertColourClose(Colour.Blue, world.Captured);
    }

    [TestMethod]
    public void MsaaX4_CameraWithFullScreenColouredRect_RendersExpectedColour()
    {
        var world = new CaptureWorld(w =>
        {
            Camera camera = Camera.WithView(0, 0, WindowWidth, WindowHeight, 0, 0, WindowWidth, WindowHeight, samples: MsaaSamples.X4);
            camera.BackgroundColour = Colour.Black;
            camera.Add(new ColourGraphic(Colour.Blue, 0, 0, WindowWidth, WindowHeight));
            w.Add(camera);
        });

        GameLauncher.Run(SettingsWithCapture(MsaaSamples.X4), () => world);

        AssertColourClose(Colour.Blue, world.Captured);
    }
}

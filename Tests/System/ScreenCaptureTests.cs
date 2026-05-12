using System;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.System;

[TestClass]
public class ScreenCaptureTests
{
    private sealed class CapturePixelAfterFrameWorld(Colour background, int x, int y) : World
    {
        private int _frame = 0;

        public Colour? Captured { get; private set; }

        public override void Update(double elapsed)
        {
            BackgroundColour = background;
            _frame++;

            if (_frame > 1)
            {
                Captured = EngineConfiguration.ScreenCaptureService.GetPixel(x, y);
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    private sealed class CaptureLatestFrameWorld(Colour background) : World
    {
        private int _frame = 0;

        public Colour[,]? Captured { get; private set; }

        public override void Update(double elapsed)
        {
            BackgroundColour = background;
            _frame++;

            if (_frame > 1)
            {
                Captured = EngineConfiguration.ScreenCaptureService.LatestFrame;
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    private sealed class AccessWithoutFlagWorld : World
    {
        public Exception? CapturedException { get; private set; }

        public override void Update(double elapsed)
        {
            try
            {
                _ = EngineConfiguration.ScreenCaptureService.GetPixel(0, 0);
            }
            catch (Exception ex)
            {
                CapturedException = ex;
            }

            EngineConfiguration.WindowService.Close();
        }
    }

    private sealed class ManualCaptureMidDrawWorld(Colour midDrawBackground, Colour finalBackground) : World
    {
        private int _frame = 0;

        public Colour? MidDrawPixel { get; private set; }

        public override void Draw()
        {
            if (_frame == 1 && MidDrawPixel is null)
            {
                BackgroundColour = midDrawBackground;
                base.Draw();

                EngineConfiguration.ScreenCaptureService.CaptureCurrentBackbuffer();
                MidDrawPixel = EngineConfiguration.ScreenCaptureService.GetPixel(10, 10);
            }

            BackgroundColour = finalBackground;
            base.Draw();
        }

        public override void Update(double elapsed)
        {
            _frame++;

            if (_frame > 2)
            {
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    private static ApplicationSettings SettingsWithCapture(int width = 200, int height = 100) => new()
    {
        WindowSettings = new WindowSettings { Width = width, Height = height, CheckForMainThread = false },
        DiagnosticsSettings = new DiagnosticsSettings { CaptureFrames = true }
    };

    private static ApplicationSettings SettingsWithoutCapture => new()
    {
        WindowSettings = new WindowSettings { CheckForMainThread = false }
    };

    private static void AssertColourClose(Colour expected, Colour actual, int tolerance = 1)
    {
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.R - actual.R), $"R: expected {expected.R}, got {actual.R}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.G - actual.G), $"G: expected {expected.G}, got {actual.G}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.B - actual.B), $"B: expected {expected.B}, got {actual.B}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.A - actual.A), $"A: expected {expected.A}, got {actual.A}");
    }

    [TestMethod]
    public void GetPixel_AfterAutoCaptureRedBackground_ReturnsRed()
    {
        var world = new CapturePixelAfterFrameWorld(Colour.Red, x: 50, y: 25);

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsNotNull(world.Captured);
        AssertColourClose(Colour.Red, world.Captured.Value);
    }

    [TestMethod]
    public void LatestFrame_AfterAutoCaptureBlueBackground_IsBlueEverywhere()
    {
        var blue = new Colour(0, 0, 255, 255);
        var world = new CaptureLatestFrameWorld(blue);

        GameLauncher.Run(SettingsWithCapture(width: 200, height: 100), () => world);

        Assert.IsNotNull(world.Captured);
        var frame = world.Captured;

        Assert.AreEqual(100, frame.GetLength(0));
        Assert.AreEqual(200, frame.GetLength(1));

        AssertColourClose(blue, frame[0, 0]);
        AssertColourClose(blue, frame[50, 100]);
        AssertColourClose(blue, frame[99, 199]);
    }

    [TestMethod]
    public void ScreenCaptureService_AccessedWithoutFlag_ThrowsWithHelpfulMessage()
    {
        var world = new AccessWithoutFlagWorld();

        GameLauncher.Run(SettingsWithoutCapture, () => world);

        Assert.IsNotNull(world.CapturedException);
        Assert.IsInstanceOfType<InvalidOperationException>(world.CapturedException);
        Assert.Contains("CaptureFrames", world.CapturedException.Message);
    }

    [TestMethod]
    public void CaptureCurrentBackbuffer_MidDraw_SeesPartialState()
    {
        var midDrawColour = new Colour(0, 200, 0, 255);
        var finalColour = new Colour(0, 0, 200, 255);
        var world = new ManualCaptureMidDrawWorld(midDrawColour, finalColour);

        GameLauncher.Run(SettingsWithCapture(), () => world);

        Assert.IsNotNull(world.MidDrawPixel);
        AssertColourClose(midDrawColour, world.MidDrawPixel.Value);
    }
}

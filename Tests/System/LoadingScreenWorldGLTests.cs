using System;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Verifies that a texture constructed on the LoadingScreenWorld worker thread (with its shared
/// GL context current) is usable from the main thread once loading completes. Catches breakage
/// in the shared-context plumbing, the worker-thread MakeCurrent step, the GLThread debug
/// guards across the cross-thread handoff, and the post-load main-context sync.
/// </summary>
[TestClass]
public class LoadingScreenWorldGLTests
{
    private const int WindowWidth = 200;
    private const int WindowHeight = 100;
    private const int SampleX = WindowWidth / 2;
    private const int SampleY = WindowHeight / 2;

    /// <summary>
    /// World that shows the loaded texture full-screen and captures the centre pixel after one
    /// rendered frame, then closes the window.
    /// </summary>
    private sealed class DisplayWorld(ITexture loadedTexture, Action<Colour> onCaptured) : World
    {
        private int _frame = 0;
        private bool _captured = false;

        public override void Load()
        {
            base.Load();
            BackgroundColour = Colour.Black;
            Add(new TextureGraphic(loadedTexture, 0, 0, WindowWidth, WindowHeight));
        }

        public override void Update(double elapsed)
        {
            base.Update(elapsed);
            _frame++;

            if (!_captured && _frame > 1)
            {
                onCaptured(EngineConfiguration.ScreenCaptureService.GetPixel(SampleX, SampleY));
                _captured = true;
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    private static ApplicationSettings SettingsWithCapture() => new()
    {
        WindowSettings = new WindowSettings { Width = WindowWidth, Height = WindowHeight, CheckForMainThread = false },
        DiagnosticsSettings = new DiagnosticsSettings { CaptureFrames = true },
        LogSettings = LogSettings.Silent,
    };

    private static void AssertColourClose(Colour expected, Colour actual, int tolerance = 4)
    {
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.R - actual.R), $"R: expected {expected.R}, got {actual.R}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.G - actual.G), $"G: expected {expected.G}, got {actual.G}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.B - actual.B), $"B: expected {expected.B}, got {actual.B}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.A - actual.A), $"A: expected {expected.A}, got {actual.A}");
    }

    /// <summary>
    /// Control: same texture, same DisplayWorld, but the texture is constructed on the MAIN
    /// thread before the loading screen. If this passes but the cross-thread test fails, the
    /// problem is in the shared-context plumbing, not the rendering path.
    /// </summary>
    [TestMethod]
    public void Control_TextureCreatedOnMainThread_RendersExpectedColour()
    {
        Colour? captured = null;
        ITexture? texture = null;

        World BuildWorld()
        {
            Colour[,] redPixels = new Colour[2, 2];
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    redPixels[x, y] = Colour.Red;
                }
            }
            texture = Textures.GenTexture(redPixels);
            return new DisplayWorld(texture, c => captured = c);
        }

        GameLauncher.Run(SettingsWithCapture(), BuildWorld);

        Assert.IsNotNull(texture);
        Assert.IsNotNull(captured);
        AssertColourClose(Colour.Red, captured!.Value);
    }

    [TestMethod]
    public void SyncLoadAssets_TextureCreatedOnWorker_IsDrawableFromMainThread()
    {
        Colour? captured = null;
        int? loadingThreadId = null;
        Exception? loadError = null;
        ITexture? loadedTexture = null;

        Colour[,] redPixels = new Colour[2, 2];
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                redPixels[x, y] = Colour.Red;
            }
        }

        World BuildLoadingWorld() => new LoadingScreenWorld(
            loadAssets: (progress, token) =>
            {
                try
                {
                    loadingThreadId = Environment.CurrentManagedThreadId;
                    loadedTexture = Textures.GenTexture(redPixels);
                    progress.Report(1f);
                }
                catch (Exception ex)
                {
                    loadError = ex;
                }
            },
            nextWorldFactory: () => new DisplayWorld(loadedTexture!, c => captured = c));

        GameLauncher.Run(SettingsWithCapture(), BuildLoadingWorld);

        Assert.IsNull(loadError, loadError?.Message);
        Assert.IsNotNull(loadingThreadId);
        Assert.AreNotEqual(Environment.CurrentManagedThreadId, loadingThreadId, "Sync loadAssets ran on the main thread instead of a worker.");
        Assert.IsNotNull(loadedTexture);
        Assert.IsNotNull(captured);
        AssertColourClose(Colour.Red, captured!.Value);
    }
}

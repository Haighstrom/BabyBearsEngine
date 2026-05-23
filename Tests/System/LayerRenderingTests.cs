using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// End-to-end render-order tests: set up a scene, run for two frames so a real frame is
/// drawn, then sample the centre pixel via <see cref="EngineConfiguration.ScreenCaptureService"/>
/// and assert the expected colour landed on top.
/// </summary>
[TestClass]
public class LayerRenderingTests
{
    private const int WindowWidth = 200;
    private const int WindowHeight = 100;
    private const int SampleX = WindowWidth / 2;
    private const int SampleY = WindowHeight / 2;

    /// <summary>
    /// World harness: defer scene construction until <see cref="World.Update"/> on the first
    /// tick (when the GL context is up and the engine is fully booted), then capture the
    /// centre pixel after another frame has rendered and close.
    /// </summary>
    private sealed class LayerCaptureWorld(Action<LayerCaptureWorld> setup) : World
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

    /// <summary>
    /// An <see cref="IRenderable"/> that deliberately does NOT implement <see cref="ILayered"/>,
    /// used to verify the engine's "unlayered = treat as layer 0 (on top)" rule. Composes a
    /// real <see cref="ColourGraphic"/> internally so it actually paints.
    /// </summary>
    private sealed class UnlayeredRectangle(Colour colour, float x, float y, float w, float h) : AddableBase, IRenderable, IDisposable
    {
        private readonly ColourGraphic _inner = new(colour, x, y, w, h);

        public bool Visible { get; set; } = true;

        public void Render(ref Matrix3 projection, ref Matrix3 modelView)
        {
            _inner.Render(ref projection, ref modelView);
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }

    private static ApplicationSettings SettingsWithCapture() => new()
    {
        WindowSettings = new WindowSettings { Width = WindowWidth, Height = WindowHeight, CheckForMainThread = false },
        DiagnosticsSettings = new DiagnosticsSettings { CaptureFrames = true },
        LogSettings = LogSettings.Silent,
    };

    private static void AssertColourClose(Colour expected, Colour actual, int tolerance = 2)
    {
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.R - actual.R), $"R: expected {expected.R}, got {actual.R}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.G - actual.G), $"G: expected {expected.G}, got {actual.G}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.B - actual.B), $"B: expected {expected.B}, got {actual.B}");
        Assert.IsLessThanOrEqualTo(tolerance, Math.Abs(expected.A - actual.A), $"A: expected {expected.A}, got {actual.A}");
    }

    [TestMethod]
    public void Smoke_SingleColouredRectangleGraphicDirectlyOnWorld_Renders()
    {
        var world = new LayerCaptureWorld(w =>
        {
            w.Add(new ColourGraphic(Colour.Blue, 0, 0, WindowWidth, WindowHeight));
        });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        AssertColourClose(Colour.Blue, world.Captured);
    }

    [TestMethod]
    public void Child_RendersOnTopOfParentsOwnContent()
    {
        // Parent entity holds a full-screen red rectangle AND a child entity holding a full-screen
        // blue rectangle. The red graphic defaults to layer int.MaxValue; the child Entity defaults
        // to layer 0. So within the parent's container the child renders on top of red regardless
        // of add order, and the blue rectangle inside the child wins the centre pixel.
        var world = new LayerCaptureWorld(w =>
        {
            var parent = new Entity(0, 0, WindowWidth, WindowHeight);
            parent.Add(new ColourGraphic(Colour.Red, 0, 0, WindowWidth, WindowHeight));

            var child = new Entity(0, 0, WindowWidth, WindowHeight);
            child.Add(new ColourGraphic(Colour.Blue, 0, 0, WindowWidth, WindowHeight));
            parent.Add(child);

            w.Add(parent);
        });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        AssertColourClose(Colour.Blue, world.Captured);
    }

    [TestMethod]
    public void Sibling_WithLowerLayer_RendersOnTop()
    {
        // Red has layer 5 (further behind), blue has layer 0 (on top). Despite red being
        // added later, blue's lower layer wins.
        var world = new LayerCaptureWorld(w =>
        {
            var blue = new ColourGraphic(Colour.Blue, 0, 0, WindowWidth, WindowHeight)
            {
                Layer = 0,
            };
            var red = new ColourGraphic(Colour.Red, 0, 0, WindowWidth, WindowHeight)
            {
                Layer = 5,
            };
            w.Add(blue);
            w.Add(red);
        });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        AssertColourClose(Colour.Blue, world.Captured);
    }

    [TestMethod]
    public void Sibling_SameLayer_LaterAddRendersOnTop()
    {
        // Both at the default layer (int.MaxValue, drawn at the back). Red added first,
        // blue added second — blue should win (deterministic add order within a layer).
        var world = new LayerCaptureWorld(w =>
        {
            w.Add(new ColourGraphic(Colour.Red, 0, 0, WindowWidth, WindowHeight));
            w.Add(new ColourGraphic(Colour.Blue, 0, 0, WindowWidth, WindowHeight));
        });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        AssertColourClose(Colour.Blue, world.Captured);
    }

    [TestMethod]
    public void UnlayeredRenderable_SortedToBack_RendersBehindLayeredSibling()
    {
        // Blue is an unlayered renderable, so Container.InsertRenderable treats it as
        // int.MaxValue and pushes it to the very back. Red has Layer = 5 and so renders
        // in front — even though blue was added FIRST.
        var world = new LayerCaptureWorld(w =>
        {
            w.Add(new UnlayeredRectangle(Colour.Blue, 0, 0, WindowWidth, WindowHeight));
            w.Add(new ColourGraphic(Colour.Red, 0, 0, WindowWidth, WindowHeight)
            {
                Layer = 5,
            });
        });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        AssertColourClose(Colour.Red, world.Captured);
    }

    [TestMethod]
    public void Overlay_RendersAboveMainWorldContent()
    {
        // Red is added to the main world container. Blue is added to the overlay, which is
        // rendered as a separate pass after main. Blue should be on top regardless of layer
        // values or add order in the main container.
        var world = new LayerCaptureWorld(w =>
        {
            w.Add(new ColourGraphic(Colour.Red, 0, 0, WindowWidth, WindowHeight));
            w.Overlay.Add(new ColourGraphic(Colour.Blue, 0, 0, WindowWidth, WindowHeight));
        });

        GameLauncher.Run(SettingsWithCapture(), () => world);

        AssertColourClose(Colour.Blue, world.Captured);
    }
}

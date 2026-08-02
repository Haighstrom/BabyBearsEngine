using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Exercises <see cref="LinePathGraphic"/> inside a real running <see cref="GameLauncher"/> world,
/// since its constructor allocates GL resources that require a live GL context — it cannot be
/// instantiated from Tests/Unit.
/// </summary>
[TestClass]
public class LinePathGraphicTests
{
    private const int WindowWidth = 200;
    private const int WindowHeight = 200;

    private sealed class OneFrameWorld(Action<OneFrameWorld> setup) : World
    {
        private int _frame = 0;

        public override void Update(double elapsed)
        {
            if (_frame == 0)
            {
                setup(this);
            }

            base.Update(elapsed);
            _frame++;

            if (_frame > 1)
            {
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    private static ApplicationSettings Settings() => new()
    {
        WindowSettings = new WindowSettings { Width = WindowWidth, Height = WindowHeight, CheckForMainThread = false },
        LogSettings = new LogSettings
        {
            ConsoleLevels = LogLevel.None,
            FileLevels = LogLevel.None,
            ErrorFileLevels = LogLevel.None,
            FilePath = null,
            ErrorFilePath = null,
            ErrorArchivePath = null,
        },
    };

    private static void RunOneFrame(Action<OneFrameWorld> setup)
    {
        GameLauncher.Run(Settings(), () => new OneFrameWorld(setup));
    }

    [TestMethod]
    public void Constructor_ComputesBoundsFromPoints()
    {
        LinePathGraphic? path = null;

        RunOneFrame(w =>
        {
            path = new LinePathGraphic([new Point(10, 40), new Point(60, 10), new Point(30, 90)], Colour.Red, thickness: 4f);
            w.Add(path);
        });

        Assert.AreEqual(10f, path!.X);
        Assert.AreEqual(10f, path.Y);
        Assert.AreEqual(50f, path.Width);
        Assert.AreEqual(80f, path.Height);
        Assert.AreEqual(3, path.Points.Count);
        Assert.AreEqual(Colour.Red, path.Colour);
        Assert.AreEqual(4f, path.Thickness);
        Assert.AreEqual(0f, path.GapLength);
    }

    [TestMethod]
    public void Constructor_SinglePoint_Throws()
    {
        RunOneFrame(w =>
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LinePathGraphic([new Point(0, 0)], Colour.White, thickness: 2f));
        });
    }

    [TestMethod]
    public void SetPoints_RecomputesBounds()
    {
        LinePathGraphic? path = null;

        RunOneFrame(w =>
        {
            path = new LinePathGraphic([new Point(0, 0), new Point(10, 10)], Colour.White, thickness: 2f);
            w.Add(path);

            path.SetPoints([new Point(100, 200), new Point(150, 250), new Point(120, 300)]);
        });

        Assert.AreEqual(100f, path!.X);
        Assert.AreEqual(200f, path.Y);
        Assert.AreEqual(50f, path.Width);
        Assert.AreEqual(100f, path.Height);
    }

    [TestMethod]
    public void AppendPoint_AddsPointAndExtendsBounds()
    {
        LinePathGraphic? path = null;

        RunOneFrame(w =>
        {
            path = new LinePathGraphic([new Point(0, 0), new Point(10, 10)], Colour.White, thickness: 2f);
            w.Add(path);

            path.AppendPoint(new Point(-5, 30));
        });

        Assert.AreEqual(3, path!.Points.Count);
        Assert.AreEqual(new Point(-5, 30), path.Points[2]);
        Assert.AreEqual(-5f, path.X);
        Assert.AreEqual(0f, path.Y);
        Assert.AreEqual(15f, path.Width);
        Assert.AreEqual(30f, path.Height);
    }

    [TestMethod]
    public void AppendPoint_ThenRender_DoesNotThrow()
    {
        RunOneFrame(w =>
        {
            LinePathGraphic path = new([new Point(0, 0), new Point(10, 10)], Colour.White, thickness: 2f);
            w.Add(path);

            path.AppendPoint(new Point(20, 0));
            path.AppendPoint(new Point(30, 10));
        });
    }

    [TestMethod]
    public void AddedToWorld_OpenPath_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            LinePathGraphic path = new(
                [new Point(0, 0), new Point(20, 40), new Point(40, 0), new Point(60, 40)],
                Colour.Blue, thickness: 3f, thicknessInPixels: false, layer: 0);
            w.Add(path);
        });
    }

    [TestMethod]
    public void AddedToWorld_ClosedLoop_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            Point p0 = new(0, 0);
            LinePathGraphic path = new([p0, new Point(40, 0), new Point(40, 40), new Point(0, 40), p0], Colour.Green, thickness: 3f);
            w.Add(path);
        });
    }

    [TestMethod]
    public void AddedToWorld_Dashed_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            LinePathGraphic path = new(
                [new Point(0, 0), new Point(40, 0), new Point(40, 40), new Point(80, 40)],
                Colour.Black, thickness: 3f)
            {
                DashLength = 8f,
                GapLength = 4f,
            };
            w.Add(path);
        });
    }

    [TestMethod]
    public void AddedToWorld_DashedClosedLoop_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            Point p0 = new(0, 0);
            LinePathGraphic path = new([p0, new Point(40, 0), new Point(40, 40), new Point(0, 40), p0], Colour.Green, thickness: 3f)
            {
                DashLength = 6f,
                GapLength = 6f,
            };
            w.Add(path);
        });
    }

    [TestMethod]
    public void AddedToWorld_DashedSharpZigzag_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            // The turn at (100, 0) is sharp enough (~100 degrees past straight-back) to trigger
            // smooth_lines.geom's miter-limit "close the gap" bevel branch, which needed its own
            // TexCoord assignment for dashing to work there too.
            LinePathGraphic path = new([new Point(0, 0), new Point(100, 0), new Point(5, 50)], Colour.Black, thickness: 6f)
            {
                DashLength = 8f,
                GapLength = 4f,
            };
            w.Add(path);
        });
    }
}

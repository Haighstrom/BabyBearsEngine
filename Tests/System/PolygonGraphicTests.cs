using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Exercises <see cref="PolygonGraphic"/> inside a real running <see cref="GameLauncher"/> world,
/// since its constructor allocates GL resources that require a live GL context — it cannot be
/// instantiated from Tests/Unit.
/// </summary>
[TestClass]
public class PolygonGraphicTests
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
        PolygonGraphic? polygon = null;

        RunOneFrame(w =>
        {
            polygon = new PolygonGraphic([new Point(10, 40), new Point(60, 10), new Point(60, 90), new Point(10, 60)], Colour.Red);
            w.Add(polygon);
        });

        Assert.AreEqual(10f, polygon!.X);
        Assert.AreEqual(10f, polygon.Y);
        Assert.AreEqual(50f, polygon.Width);
        Assert.AreEqual(80f, polygon.Height);
        Assert.AreEqual(4, polygon.Points.Count);
        Assert.AreEqual(Colour.Red, polygon.Colour);
    }

    [TestMethod]
    public void Constructor_ClosedInput_StripsRepeatedFirstPoint()
    {
        PolygonGraphic? polygon = null;
        Point first = new(0, 0);

        RunOneFrame(w =>
        {
            polygon = new PolygonGraphic([first, new Point(10, 0), new Point(10, 10), new Point(0, 10), first], Colour.White);
            w.Add(polygon);
        });

        Assert.AreEqual(4, polygon!.Points.Count);
    }

    [TestMethod]
    public void Constructor_FewerThanThreeDistinctPoints_Throws()
    {
        RunOneFrame(w =>
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PolygonGraphic([new Point(0, 0), new Point(10, 0)], Colour.White));
        });
    }

    [TestMethod]
    public void SetPoints_RecomputesBounds()
    {
        PolygonGraphic? polygon = null;

        RunOneFrame(w =>
        {
            polygon = new PolygonGraphic([new Point(0, 0), new Point(10, 0), new Point(0, 10)], Colour.White);
            w.Add(polygon);

            polygon.SetPoints([new Point(100, 200), new Point(150, 200), new Point(150, 250), new Point(100, 250)]);
        });

        Assert.AreEqual(100f, polygon!.X);
        Assert.AreEqual(200f, polygon.Y);
        Assert.AreEqual(50f, polygon.Width);
        Assert.AreEqual(50f, polygon.Height);
    }

    [TestMethod]
    public void AddedToWorld_ConvexPolygon_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            PolygonGraphic polygon = new([new Point(0, 0), new Point(40, 0), new Point(40, 40), new Point(0, 40)], Colour.Blue, layer: 0);
            w.Add(polygon);
        });
    }

    [TestMethod]
    public void AddedToWorld_ConcavePolygon_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            // L-shape.
            PolygonGraphic polygon = new(
                [new Point(0, 0), new Point(60, 0), new Point(60, 30), new Point(30, 30), new Point(30, 60), new Point(0, 60)],
                Colour.Green);
            w.Add(polygon);
        });
    }
}

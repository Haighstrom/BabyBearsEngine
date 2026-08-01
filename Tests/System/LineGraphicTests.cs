using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Exercises <see cref="LineGraphic"/> inside a real running <see cref="GameLauncher"/> world,
/// since its constructor allocates GL resources that require a live GL context — it cannot be
/// instantiated from Tests/Unit.
/// </summary>
[TestClass]
public class LineGraphicTests
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
    public void Constructor_SetsStartEndColourAndThickness()
    {
        LineGraphic? line = null;

        RunOneFrame(w =>
        {
            line = new LineGraphic(new Point(10, 20), new Point(110, 220), Colour.Red, thickness: 4f);
            w.Add(line);
        });

        Assert.AreEqual(new Point(10, 20), line!.Start);
        Assert.AreEqual(new Point(110, 220), line.End);
        Assert.AreEqual(Colour.Red, line.Colour);
        Assert.AreEqual(4f, line.Thickness);
        Assert.IsTrue(line.ThicknessInPixels);
    }

    [TestMethod]
    public void Start_Set_KeepsEndFixed()
    {
        LineGraphic? line = null;

        RunOneFrame(w =>
        {
            line = new LineGraphic(new Point(0, 0), new Point(100, 100), Colour.White, thickness: 2f);
            w.Add(line);

            line.Start = new Point(30, 40);
        });

        Assert.AreEqual(new Point(30, 40), line!.Start);
        Assert.AreEqual(new Point(100, 100), line.End);
    }

    [TestMethod]
    public void End_Set_KeepsStartFixed()
    {
        LineGraphic? line = null;

        RunOneFrame(w =>
        {
            line = new LineGraphic(new Point(0, 0), new Point(100, 100), Colour.White, thickness: 2f);
            w.Add(line);

            line.End = new Point(50, 10);
        });

        Assert.AreEqual(new Point(0, 0), line!.Start);
        Assert.AreEqual(new Point(50, 10), line.End);
    }

    [TestMethod]
    public void AddedToWorld_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            LineGraphic line = new(new Point(0, 0), new Point(50, 50), Colour.Blue, thickness: 3f, thicknessInPixels: false, layer: 0);
            w.Add(line);
        });
    }
}

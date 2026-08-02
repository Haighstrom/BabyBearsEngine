using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Exercises <see cref="MovingDashedLineGraphic"/> inside a real running <see cref="GameLauncher"/>
/// world, since its constructor allocates GL resources that require a live GL context — it cannot
/// be instantiated from Tests/Unit.
/// </summary>
[TestClass]
public class MovingDashedLineGraphicTests
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
    public void Constructor_SetsDashProperties()
    {
        MovingDashedLineGraphic? line = null;

        RunOneFrame(w =>
        {
            // Deliberately not added to the world - these assertions need exact, hand-controlled
            // DashOffset values, and adding it would let the container's automatic per-frame tick
            // (real, unpredictable elapsed time) advance it too.
            line = new MovingDashedLineGraphic(new Point(0, 0), new Point(100, 0), Colour.Black, thickness: 3f, dashLength: 8f, gapLength: 4f, dashSpeed: 20f);
        });

        Assert.AreEqual(8f, line!.DashLength);
        Assert.AreEqual(4f, line.GapLength);
        Assert.AreEqual(20f, line.DashSpeed);
        Assert.AreEqual(0f, line.DashOffset);
        Assert.IsTrue(line.Active);
    }

    [TestMethod]
    public void Update_AdvancesDashOffsetBySpeedTimesElapsed()
    {
        MovingDashedLineGraphic? line = null;

        RunOneFrame(w =>
        {
            // Not added to the world - see the comment in Constructor_SetsDashProperties.
            line = new MovingDashedLineGraphic(new Point(0, 0), new Point(100, 0), Colour.Black, thickness: 3f, dashLength: 100f, gapLength: 100f, dashSpeed: 10f);
        });

        line!.Update(0.5);

        Assert.AreEqual(5f, line.DashOffset, 1e-4f);
    }

    [TestMethod]
    public void Update_WrapsDashOffsetWithinOnePeriod()
    {
        MovingDashedLineGraphic? line = null;

        RunOneFrame(w =>
        {
            // Not added to the world - see the comment in Constructor_SetsDashProperties.
            line = new MovingDashedLineGraphic(new Point(0, 0), new Point(100, 0), Colour.Black, thickness: 3f, dashLength: 6f, gapLength: 4f, dashSpeed: 100f);
        });

        line!.Update(1.0); // would be 100, period is 10

        Assert.AreEqual(0f, line.DashOffset, 1e-3f);
    }

    [TestMethod]
    public void AddedToWorld_TicksAutomaticallyWithoutManualUpdateCalls()
    {
        MovingDashedLineGraphic? line = null;

        RunOneFrame(w =>
        {
            line = new MovingDashedLineGraphic(new Point(0, 0), new Point(100, 0), Colour.Black, thickness: 3f, dashLength: 1000f, gapLength: 1000f, dashSpeed: 1_000_000f);
            w.Add(line);
        });

        // Never called line.Update(...) ourselves - if this moved at all, the container picked it
        // up as an IUpdateable and ticked it, same as any other updateable entity.
        Assert.AreNotEqual(0f, line!.DashOffset);
    }

    [TestMethod]
    public void AddedToWorld_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            MovingDashedLineGraphic line = new(new Point(0, 0), new Point(100, 0), Colour.Blue, thickness: 3f, dashLength: 8f, gapLength: 4f, dashSpeed: 20f, thicknessInPixels: false, layer: 0);
            w.Add(line);
            line.Update(0.1);
        });
    }
}

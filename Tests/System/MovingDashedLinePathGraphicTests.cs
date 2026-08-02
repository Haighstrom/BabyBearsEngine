using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Exercises <see cref="MovingDashedLinePathGraphic"/> inside a real running
/// <see cref="GameLauncher"/> world, since its constructor allocates GL resources that require a
/// live GL context — it cannot be instantiated from Tests/Unit.
/// </summary>
[TestClass]
public class MovingDashedLinePathGraphicTests
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

    private static Point[] SamplePoints() => [new(0, 0), new(40, 0), new(40, 40), new(0, 40)];

    [TestMethod]
    public void Constructor_SetsDashProperties()
    {
        MovingDashedLinePathGraphic? path = null;

        RunOneFrame(w =>
        {
            // Deliberately not added to the world - these assertions need exact, hand-controlled
            // DashOffset values, and adding it would let the container's automatic per-frame tick
            // (real, unpredictable elapsed time) advance it too.
            path = new MovingDashedLinePathGraphic(SamplePoints(), Colour.Black, thickness: 3f, dashLength: 8f, gapLength: 4f, dashSpeed: 20f);
        });

        Assert.AreEqual(8f, path!.DashLength);
        Assert.AreEqual(4f, path.GapLength);
        Assert.AreEqual(20f, path.DashSpeed);
        Assert.AreEqual(0f, path.DashOffset);
        Assert.IsTrue(path.Active);
    }

    [TestMethod]
    public void Update_AdvancesDashOffsetBySpeedTimesElapsed()
    {
        MovingDashedLinePathGraphic? path = null;

        RunOneFrame(w =>
        {
            // Not added to the world - see the comment in Constructor_SetsDashProperties.
            path = new MovingDashedLinePathGraphic(SamplePoints(), Colour.Black, thickness: 3f, dashLength: 100f, gapLength: 100f, dashSpeed: 10f);
        });

        path!.Update(0.5);

        Assert.AreEqual(5f, path.DashOffset, 1e-4f);
    }

    [TestMethod]
    public void Update_WrapsDashOffsetWithinOnePeriod()
    {
        MovingDashedLinePathGraphic? path = null;

        RunOneFrame(w =>
        {
            // Not added to the world - see the comment in Constructor_SetsDashProperties.
            path = new MovingDashedLinePathGraphic(SamplePoints(), Colour.Black, thickness: 3f, dashLength: 6f, gapLength: 4f, dashSpeed: 100f);
        });

        path!.Update(1.0); // would be 100, period is 10

        Assert.AreEqual(0f, path.DashOffset, 1e-3f);
    }

    [TestMethod]
    public void AddedToWorld_TicksAutomaticallyWithoutManualUpdateCalls()
    {
        MovingDashedLinePathGraphic? path = null;

        RunOneFrame(w =>
        {
            path = new MovingDashedLinePathGraphic(SamplePoints(), Colour.Black, thickness: 3f, dashLength: 1000f, gapLength: 1000f, dashSpeed: 1_000_000f);
            w.Add(path);
        });

        Assert.AreNotEqual(0f, path!.DashOffset);
    }

    [TestMethod]
    public void AddedToWorld_RendersWithoutThrowing()
    {
        RunOneFrame(w =>
        {
            MovingDashedLinePathGraphic path = new(SamplePoints(), Colour.Blue, thickness: 3f, dashLength: 8f, gapLength: 4f, dashSpeed: 20f, thicknessInPixels: false, layer: 0);
            w.Add(path);
            path.Update(0.1);
        });
    }
}

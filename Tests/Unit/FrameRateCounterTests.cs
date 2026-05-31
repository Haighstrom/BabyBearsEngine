using System.IO;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FrameRateCounterTests
{
    // Durations below are exact in IEEE 754 (powers of 2 or sums thereof) so windows
    // close on the exact tick the test expects.

    [TestMethod]
    public void Fps_BeforeFirstSampleCompletes_IsZero()
    {
        FrameRateCounter counter = new();

        counter.Tick(0.25);
        counter.Tick(0.25);

        Assert.AreEqual(0.0, counter.Fps);
    }

    [TestMethod]
    public void Fps_AfterOneSecondOfFrames_ReflectsFrameCount()
    {
        FrameRateCounter counter = new();

        for (int i = 0; i < 4; i++)
        {
            counter.Tick(0.25);
        }

        Assert.AreEqual(4.0, counter.Fps);
    }

    [TestMethod]
    public void Fps_SampleResetsAfterEachSecond()
    {
        FrameRateCounter counter = new();

        for (int i = 0; i < 4; i++)
        {
            counter.Tick(0.25);
        }
        for (int i = 0; i < 2; i++)
        {
            counter.Tick(0.5);
        }

        Assert.AreEqual(2.0, counter.Fps);
    }

    [TestMethod]
    public void Fps_LongFrameOverOneSecond_SamplesImmediately()
    {
        FrameRateCounter counter = new();

        counter.Tick(2.0);

        Assert.AreEqual(0.5, counter.Fps);
    }

    [TestMethod]
    public void Fps_HoldsLastSample_UntilNextWindowCompletes()
    {
        FrameRateCounter counter = new();
        for (int i = 0; i < 4; i++)
        {
            counter.Tick(0.25);
        }

        counter.Tick(0.125);
        counter.Tick(0.125);

        Assert.AreEqual(4.0, counter.Fps);
    }

    [TestMethod]
    public void Fps_RemainderCarriesIntoNextWindow()
    {
        FrameRateCounter counter = new();

        // Window 1: 2 ticks of 0.75 = 1.5s. Sample fires with 0.5s remainder carrying over.
        counter.Tick(0.75);
        counter.Tick(0.75);

        // Window 2: a single 0.5s tick combined with the 0.5s carry reaches the 1.0s threshold,
        // closing the window with Fps = 1. Without carryover it would only reach 0.5s and Fps
        // would still reflect window 1.
        counter.Tick(0.5);

        Assert.AreEqual(1.0, counter.Fps);
    }

    [TestMethod]
    public void Tick_FrameLongerThanStallThreshold_ResetsCleanly()
    {
        FrameRateCounter counter = new();

        counter.Tick(3.0);

        // Stall sample is recorded for the slow frame itself...
        Assert.AreEqual(1.0 / 3.0, counter.Fps, delta: 0.001);

        // ...and subsequent normal frames close a clean 1-second window without leftover stall time.
        for (int i = 0; i < 4; i++)
        {
            counter.Tick(0.25);
        }

        Assert.AreEqual(4.0, counter.Fps);
    }

    [TestMethod]
    public void Tick_StallWarning_LogsAccumulatedWindowDurationNotJustTheLastTick()
    {
        var originalOut = Console.Out;
        var captured = new StringWriter();
        Console.SetOut(captured);

        Logger.Initialise(new LogSettings
        {
            ConsoleLevels = LogLevel.All,
            FileLevels = LogLevel.None,
            ErrorFileLevels = LogLevel.None,
        }, new ConsoleSettings { ColouriseLogOutput = false });

        try
        {
            FrameRateCounter counter = new();

            // Accumulate 0.5s + 3.0s = 3.5s across two ticks. Window closes on the second tick
            // and exceeds the 2.0s stall threshold. The warning should report the accumulated
            // window (3.50s), not just the final tick's elapsed (3.00s).
            counter.Tick(0.5);
            counter.Tick(3.0);

            string output = captured.ToString();
            Assert.Contains("3.50", output);
        }
        finally
        {
            Logger.Initialise(LogSettings.Silent, ConsoleSettings.Default);
            Console.SetOut(originalOut);
            captured.Dispose();
        }
    }
}

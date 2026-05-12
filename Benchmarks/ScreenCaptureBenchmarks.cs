using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Benchmarks;

/// <summary>
/// Measures the per-frame cost of <see cref="DiagnosticsSettings.CaptureFrames"/>. Each benchmark
/// spins up a fresh engine, runs a fixed number of frames with an empty world, then closes the
/// window. The difference between the capture-on and capture-off rows divided by the frame count
/// gives the per-frame cost of the readback.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 3)]
public class ScreenCaptureBenchmarks
{
    private const int FrameCount = 60;

    private sealed class CountdownWorld(int frames) : World
    {
        private int _remaining = frames;

        public override void Update(double elapsed)
        {
            _remaining--;

            if (_remaining <= 0)
            {
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    private static ApplicationSettings MakeSettings(int width, int height, bool captureFrames) => new()
    {
        WindowSettings = new WindowSettings
        {
            Width = width,
            Height = height,
            CheckForMainThread = false,
            VSync = false,
        },
        GameLoopSettings = new GameLoopSettings { TargetFramesPerSecond = 0 },
        DiagnosticsSettings = new DiagnosticsSettings { CaptureFrames = captureFrames },
    };

    [Benchmark(Baseline = true)]
    public void Run60Frames_800x600_NoCapture()
    {
        GameLauncher.Run(MakeSettings(800, 600, captureFrames: false), () => new CountdownWorld(FrameCount));
    }

    [Benchmark]
    public void Run60Frames_800x600_WithCapture()
    {
        GameLauncher.Run(MakeSettings(800, 600, captureFrames: true), () => new CountdownWorld(FrameCount));
    }

    [Benchmark]
    public void Run60Frames_1920x1080_NoCapture()
    {
        GameLauncher.Run(MakeSettings(1920, 1080, captureFrames: false), () => new CountdownWorld(FrameCount));
    }

    [Benchmark]
    public void Run60Frames_1920x1080_WithCapture()
    {
        GameLauncher.Run(MakeSettings(1920, 1080, captureFrames: true), () => new CountdownWorld(FrameCount));
    }
}

using System.IO;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Regression test for a TextGraphic bug where FirstCharToDraw skipped rendering the leading
/// characters of a long string but did not reposition the remaining ("visible") characters to
/// start at the graphic's own origin — they kept the pen position they would have occupied in
/// the full, unfiltered string. For a TextInputBox showing only a scrolled window of a long
/// string, this pushed the intended-visible substring far outside the box's bounds, which fired
/// a spurious truncation warning, left a blank gap on the left of the box where text should have
/// shown, and misaligned the cursor relative to what little text did render.
/// </summary>
[TestClass]
public class TextGraphicScrollingTests
{
    private const int WindowWidth = 400;
    private const int WindowHeight = 100;

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

    private static ApplicationSettings SettingsWithConsoleCapture() => new()
    {
        WindowSettings = new WindowSettings { Width = WindowWidth, Height = WindowHeight, CheckForMainThread = false },
        ConsoleSettings = new ConsoleSettings { ColouriseLogOutput = false },
        LogSettings = new LogSettings
        {
            ConsoleLevels = LogLevel.All,
            FileLevels = LogLevel.None,
            ErrorFileLevels = LogLevel.None,
            FilePath = null,
            ErrorFilePath = null,
            ErrorArchivePath = null,
        },
    };

    private static string RunAndCaptureConsole(Action<OneFrameWorld> setup)
    {
        TextWriter originalConsoleOut = Console.Out;
        StringWriter capturedConsole = new();
        Console.SetOut(capturedConsole);

        try
        {
            GameLauncher.Run(SettingsWithConsoleCapture(), () => new OneFrameWorld(setup));
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }

        return capturedConsole.ToString();
    }

    [TestMethod]
    public void TextInputBox_LongTextRequiringScroll_DoesNotWarnAboutTruncation()
    {
        string output = RunAndCaptureConsole(w =>
        {
            TextInputBox box = new(10, 10, 200, 30, InputBoxTheme.Default);
            // Far longer than the 192px-wide content area can show at once, forcing
            // EnsureScrollOffset to advance FirstCharToDraw well past 0. Before the fix, the
            // discarded prefix's accumulated width pushed the "visible" characters far outside
            // the box, which both left them blank and fired this warning.
            box.Text = new string('x', 200);
            w.Add(box);
        });

        Assert.DoesNotContain("truncated", output, $"Unexpected truncation warning in output:\n{output}");
    }

    [TestMethod]
    public void TextGraphic_TextWiderThanBoxWithNoScrolling_StillWarnsAboutTruncation()
    {
        // Control: proves the console-capture harness genuinely observes the warning when the
        // underlying condition is met — text too wide for the graphic, with FirstCharToDraw left
        // at its default of 0 (no scrolling involved at all) — so the absence of the warning in
        // the scrolling test above is meaningful, not a silently-broken capture, and confirms the
        // fix didn't suppress truncation detection for the ordinary (non-scrolled) case.
        string output = RunAndCaptureConsole(w =>
        {
            TextGraphic textGraphic = new(new FontDefinition("Arial", 12f), new string('x', 200), Colour.Black, 10, 10, 50, 20);
            w.Add(textGraphic);
        });

        Assert.Contains("truncated", output, $"Expected a truncation warning in output:\n{output}");
    }
}

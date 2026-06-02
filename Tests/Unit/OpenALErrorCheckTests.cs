using System.IO;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Platform.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class OpenALErrorCheckTests
{
    private TextWriter _originalConsoleOut = null!;
    private StringWriter _capturedConsole = null!;

    [TestInitialize]
    public void Setup()
    {
        _originalConsoleOut = Console.Out;
        _capturedConsole = new StringWriter();
        Console.SetOut(_capturedConsole);

        Logger.Initialise(new LogSettings
        {
            ConsoleLevels = LogLevel.All,
            FileLevels = LogLevel.None,
            ErrorFileLevels = LogLevel.None,
        }, new ConsoleSettings { ColouriseLogOutput = false });

        OpenALErrorCheck.ResetDedupe();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Logger.Initialise(LogSettings.Silent, ConsoleSettings.Default);
        Console.SetOut(_originalConsoleOut);
        _capturedConsole.Dispose();
        OpenALErrorCheck.ResetDedupe();
    }

    [TestMethod]
    public void Check_WhenAlReportsNoError_DoesNotLog()
    {
        // No AL context in this test, so AL.GetError returns NoError. Verify no log fires.
        OpenALErrorCheck.Check("TestOperation");

        Assert.DoesNotContain("[Warning]", _capturedConsole.ToString());
    }

    // Note: triggering a real ALError without a live AL context isn't practical in a unit
    // test — AL.GetError returns NoError when no context is current. The dedupe-with-error
    // path is exercised in system tests that actually load OpenAL.
}

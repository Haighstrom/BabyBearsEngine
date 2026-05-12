using System.Reflection;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class GameLauncher
{
    private static bool s_running = false;

    public static void Run(ApplicationSettings appSettings, Func<IWorld> worldFactory)
    {
        if (s_running)
        {
            throw new InvalidOperationException("Game already running.");
        }

        // Open the console window (if requested) BEFORE Logger.Initialise so the console sink has
        // somewhere visible to write to. On non-Windows the call no-ops.
        var cs = appSettings.ConsoleSettings;
        if (cs.ShowConsoleWindow)
        {
            ConsoleWindow.Open(cs.X, cs.Y, cs.Width, cs.Height);
            string game = Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
            ConsoleWindow.SetTitle($"BabyBearsEngine - {game}");
        }

        Logger.Initialise(appSettings.LogSettings, appSettings.ConsoleSettings);

        EngineDiagnostics.LogSystemInformation();

        appSettings.DiagnosticsSettings.WarnIfEnabledInRelease();

        s_running = true;

        try
        {
            var engine = new OpenTKGameEngine(appSettings);

            EngineConfiguration.Initialise(
                window: new OpenTKWindowAdapter(engine),
                keyboard: new OpenTKKeyboardAdapter(engine.KeyboardState),
                mouse: new OpenTKMouseAdapter(engine.MouseState),
                worldSwitcher: engine,
                screenCapture: appSettings.DiagnosticsSettings.CaptureFrames ? new OpenTKScreenCaptureAdapter(engine) : null);

            engine.Run(worldFactory());
        }
        finally
        {
            EngineConfiguration.Reset();
            s_running = false;
        }
    }
}

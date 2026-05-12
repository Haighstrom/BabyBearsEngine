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

        Logger.Initialise(appSettings.LogSettings, appSettings.ConsoleSettings);

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

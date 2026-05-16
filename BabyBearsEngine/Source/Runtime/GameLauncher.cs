using System.Reflection;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.OpenGL;
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
        catch (Exception ex)
        {
            Logger.Fatal("Unhandled exception in engine loop.", ex);
            throw;
        }
        finally
        {
            // Drop cached shader-program singletons so the next Run gets fresh GL handles in
            // its new context — otherwise the second Run uses dangling handles from the first.
            // The previous instance's GL resources leak, but the context they belonged to is
            // being torn down anyway.
            SolidColourShaderProgram.ResetForNextRun();
            SolidColourShaderProgramMatrix.ResetForNextRun();

            EngineConfiguration.Reset();
            s_running = false;
        }
    }
}

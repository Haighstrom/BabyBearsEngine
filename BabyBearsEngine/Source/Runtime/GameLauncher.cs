using System.Reflection;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine;

/// <summary>
/// Entry point for running a BabyBearsEngine game. Call <see cref="Run"/> once per process
/// lifetime; it blocks the calling thread until the window closes. Sequential calls (after
/// the previous run has returned) are supported.
/// </summary>
public static class GameLauncher
{
    private static bool s_running = false;

    /// <summary>
    /// Initialises the engine, opens the game window, and runs the main loop until the window
    /// is closed. Blocks the calling thread for the lifetime of the game.
    /// </summary>
    /// <param name="appSettings">Configuration for the window, rendering, input, logging, and other subsystems.</param>
    /// <param name="worldFactory">Factory that produces the initial <see cref="IWorld"/> to load on startup.</param>
    /// <exception cref="InvalidOperationException">Thrown if called while the engine is already running. Nested and concurrent calls are not supported.</exception>
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

        EngineConfiguration.DefaultCameraMsaa = appSettings.DefaultCameraMsaa;

        try
        {
            var engine = new OpenTKGameEngine(appSettings);

            EngineConfiguration.Initialise(
                window: new OpenTKWindowAdapter(engine),
                keyboard: new OpenTKKeyboardAdapter(engine.KeyboardState),
                mouse: new OpenTKMouseAdapter(engine.MouseState),
                worldSwitcher: engine,
                engineInfo: engine.EngineInfo,
                screenCapture: appSettings.DiagnosticsSettings.CaptureFrames ? new OpenTKScreenCaptureAdapter(engine) : null,
                atlasGenerator: ResolveAtlasGenerator(appSettings.TextSettings.Renderer));

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

    private static IFontAtlasGenerator ResolveAtlasGenerator(TextRenderer renderer) => renderer switch
    {
        TextRenderer.Gdi => new GdiFontAtlasGenerator(),
        _ => throw new NotSupportedException($"TextRenderer.{renderer} is not supported by this engine build."),
    };
}

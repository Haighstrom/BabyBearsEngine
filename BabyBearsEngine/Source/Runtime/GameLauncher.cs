using System.Reflection;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.IO;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform;
using BabyBearsEngine.Platform.OpenAL;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Worlds;

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

        // Pre-load any native libraries we ship (currently just OpenAL) so subsequent P/Invoke
        // calls find them already resident. Throws on non-Windows until cross-platform binaries
        // and probe paths are added — see #106.
        NativeLibraryResolver.Initialise();

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

        // The default backend for fonts that don't pin one; Initialise registers both built-ins so
        // individual fonts can still opt into the other via FontDefinition.Renderer.
        EngineConfiguration.DefaultTextRenderer = appSettings.TextSettings.Renderer;

        // If the localisation assets folder exists, switch from the default NullLocaliser to a
        // JsonLocaliser loaded from disk. Missing folder is the opt-out signal; games without
        // translations leave it absent and Strings.Get returns keys verbatim.
        var localisationSettings = appSettings.LocalisationSettings;
        if (Files.DirectoryExists(localisationSettings.AssetsFolder))
        {
            EngineConfiguration.LocalisationService = new JsonLocaliser(
                localisationSettings.AssetsFolder,
                localisationSettings.DefaultLocale,
                localisationSettings.FallbackLocale);
        }

        OpenALAudioService? audioService = null;
        try
        {
            var engine = new OpenTKGameEngine(appSettings);

            // The GameWindow constructor creates the GL context on this thread. Register the
            // thread as a GL thread now (before worldFactory() runs and starts constructing GL
            // resources) and stand up the shared-context factory for LoadingScreenWorld.
            GLThread.Register();
            EngineConfiguration.GLLoadingContextFactory = new OpenTKGLLoadingContextFactory(engine);

            audioService = new OpenALAudioService(appSettings.AudioSettings);

            EngineConfiguration.Initialise(
                window: new OpenTKWindowAdapter(engine),
                keyboard: new OpenTKKeyboardAdapter(engine.KeyboardState),
                mouse: new OpenTKMouseAdapter(engine.MouseState),
                worldSwitcher: engine,
                engineInfo: engine.EngineInfo,
                audio: audioService,
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
            EngineTeardown.ResetForNextRun(audioService);
            s_running = false;
        }
    }
}

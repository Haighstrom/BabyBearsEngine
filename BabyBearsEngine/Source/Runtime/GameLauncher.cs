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

        // Open the console window (if requested AND supported) BEFORE Logger.Initialise so the
        // console sink has somewhere visible to write to. We gate on IsSupported here rather than
        // letting ConsoleWindow.Open no-op so we don't trigger its unsupported-platform warning,
        // which would route to the un-initialised default Logger (creating a stray log.log in
        // CWD). The deferred Logger.Warning below replaces that emission once the user's sinks
        // are bound.
        var cs = appSettings.ConsoleSettings;
        bool consoleRequestedButUnsupported = cs.ShowConsoleWindow && !ConsoleWindow.IsSupported;
        if (cs.ShowConsoleWindow && ConsoleWindow.IsSupported)
        {
            ConsoleWindow.Open(cs.X, cs.Y, cs.Width, cs.Height);
            string game = Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
            ConsoleWindow.SetTitle($"BabyBearsEngine - {game}");
        }

        Files.Settings = appSettings.IoSettings;

        Logger.Initialise(appSettings.LogSettings, appSettings.ConsoleSettings);

        if (consoleRequestedButUnsupported)
        {
            Logger.Warning("ConsoleSettings.ShowConsoleWindow is true but the platform doesn't support console window allocation. Skipping.");
        }

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
                window: new OpenTKWindowAdapter(engine, appSettings.WindowSettings.Cursor, appSettings.WindowSettings.Icon),
                keyboard: new OpenTKKeyboardAdapter(engine),
                mouse: new OpenTKMouseAdapter(engine),
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
            // Best-effort teardown — never let a cleanup failure escape this finally. If it did
            // it would either crash the process on a clean shutdown, OR (worse) replace whatever
            // original exception came out of the engine loop, hiding the real diagnostic. The
            // inner finally guarantees s_running gets reset even if Logger.Error itself throws,
            // so a future GameLauncher.Run isn't locked out by the s_running guard.
            try
            {
                EngineTeardown.ResetForNextRun(audioService);
            }
            catch (Exception teardownEx)
            {
                Logger.Error("Exception during EngineTeardown.ResetForNextRun.", teardownEx);
            }
            finally
            {
                s_running = false;
            }
        }
    }
}

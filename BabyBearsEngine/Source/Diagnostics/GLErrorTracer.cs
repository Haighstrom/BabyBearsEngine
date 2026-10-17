using System.Diagnostics;
using System.Runtime.InteropServices;
using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Opt-in GL error tracing (<see cref="DiagnosticsSettings.TraceGLErrors"/>). Hooks the driver's debug
/// output (KHR_debug) when available, logging each error with the managed stack of the call that
/// raised it, and checks <c>glGetError</c> at frame stages as a fallback. Each distinct error is
/// logged once, and tracing never throws, so one run yields a complete log.
/// </summary>
internal static class GLErrorTracer
{
    private static readonly HashSet<string> s_logged = [];
    private static DebugProc? s_callback;

    public static bool Enabled { get; private set; } = false;

    /// <summary>Turns tracing on and logs any errors already pending; call right after the GL context is live.</summary>
    public static void Enable()
    {
        Enabled = true;
        Check("engine load (before tracing started)");
        Logger.Information("GL error tracing is on: " + TryHookDebugOutput());
        Check("enabling debug output");
    }

    /// <summary>Logs and clears every pending GL error, naming the stage it was found at; no-op when tracing is off.</summary>
    public static void Check(string stage)
    {
        if (!Enabled)
        {
            return;
        }

        ErrorCode error;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            if (s_logged.Add($"stage|{stage}|{error}"))
            {
                Logger.Error($"GL error {error} found at stage: {stage}");
            }
        }
    }

    private static string TryHookDebugOutput()
    {
        try
        {
            s_callback = OnDebugMessage;
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(s_callback, IntPtr.Zero);
            return "driver debug output hooked (errors will include the calling code).";
        }
        catch (Exception exception)
        {
            return $"driver debug output unavailable ({exception.GetType().Name}); only per-stage checks will run.";
        }
    }

    private static void OnDebugMessage(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
    {
        if (type is not (DebugType.DebugTypeError or DebugType.DebugTypeUndefinedBehavior or DebugType.DebugTypeDeprecatedBehavior))
        {
            return;
        }

        string text = Marshal.PtrToStringAnsi(message, length);
        string stack = new StackTrace(skipFrames: 1, fNeedFileInfo: true).ToString();
        string key = $"{id}|{text}|{stack}";

        lock (s_logged)
        {
            if (!s_logged.Add(key))
            {
                return;
            }
        }

        Logger.Error($"GL debug [{type}, {severity}, source {source}, id {id}]: {text}{Environment.NewLine}{stack}");
    }
}

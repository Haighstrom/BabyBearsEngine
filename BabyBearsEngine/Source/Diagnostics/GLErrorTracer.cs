using System.Diagnostics;
using System.Runtime.InteropServices;
using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Opt-in GL diagnostics (<see cref="DiagnosticsSettings.TraceGLErrors"/>). Hooks the driver's debug
/// output (KHR_debug) when available, logging each error with the managed stack of the call that
/// raised it, and checks <c>glGetError</c> at stages as a fallback. It also logs shader info logs and
/// periodically probes the pixels of camera targets, the window and loaded textures, to tell "nothing
/// drawn" from "drawn but not shown". Each distinct error is logged once, and tracing never throws.
/// </summary>
internal static class GLErrorTracer
{
    private const int MaxProbedFrames = 8;
    private const int MaxRecordedTextures = 4;
    private const int ProbeFrameInterval = 180;
    private const int ProbeGridColumns = 8;
    private const int ProbeGridRows = 6;

    private static readonly HashSet<string> s_logged = [];
    private static readonly List<RecordedTexture> s_recordedTextures = [];
    private static DebugProc? s_callback;
    private static int s_frameNumber = 0;

    private readonly record struct RecordedTexture(int Handle, int Width, int Height, string Source);

    public static bool Enabled { get; private set; } = false;

    private static bool IsProbeFrame => s_frameNumber % ProbeFrameInterval == 0 && s_frameNumber / ProbeFrameInterval <= MaxProbedFrames;

    /// <summary>Turns tracing on for the current context and logs any errors already pending; call as soon as the context is live.</summary>
    public static void Enable()
    {
        Enabled = true;
        Check("window and context creation");
        Logger.Information("GL error tracing is on: " + TryHookDebugOutput());
        Check("enabling debug output");
    }

    /// <summary>Hooks driver debug output on the calling thread's current context, e.g. a background loading context; no-op when tracing is off.</summary>
    public static void HookCurrentContext()
    {
        if (Enabled)
        {
            Logger.Information("GL tracing on a background context: " + TryHookDebugOutput());
        }
    }

    /// <summary>Counts a rendered frame, so probes can be spaced out; call once at the start of each frame.</summary>
    public static void NextFrame()
    {
        if (Enabled)
        {
            s_frameNumber++;
        }
    }

    /// <summary>Logs and clears every pending GL error on the current context, naming the stage it was found at; no-op when tracing is off.</summary>
    public static void Check(string stage)
    {
        if (!Enabled)
        {
            return;
        }

        ErrorCode error;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            if (ClaimOnce($"stage|{stage}|{error}"))
            {
                Logger.Error($"GL error {error} found at stage: {stage}");
            }
        }
    }

    /// <summary>Logs an informational line; no-op when tracing is off.</summary>
    public static void Note(string message)
    {
        if (Enabled)
        {
            Logger.Information(message);
        }
    }

    /// <summary>Logs a shader compile or link info log if the driver produced one; no-op when tracing is off.</summary>
    public static void LogShaderInfo(string what, string infoLog)
    {
        if (Enabled && !string.IsNullOrWhiteSpace(infoLog))
        {
            Logger.Warning($"{what} info log: {infoLog.Trim()}");
        }
    }

    /// <summary>Remembers a texture just uploaded so <see cref="ProbeRecordedTextures"/> can later check the main context sees its pixels.</summary>
    public static void RecordTexture(int handle, int width, int height, string source)
    {
        if (!Enabled)
        {
            return;
        }

        lock (s_recordedTextures)
        {
            if (s_recordedTextures.Count < MaxRecordedTextures)
            {
                s_recordedTextures.Add(new RecordedTexture(handle, width, height, source));
            }
        }
    }

    /// <summary>Reads back the recorded textures on the calling (main) context and logs how much of each is non-transparent.</summary>
    public static void ProbeRecordedTextures()
    {
        if (!Enabled)
        {
            return;
        }

        RecordedTexture[] textures;
        lock (s_recordedTextures)
        {
            textures = [.. s_recordedTextures];
        }

        foreach (RecordedTexture texture in textures)
        {
            byte[] pixels = new byte[texture.Width * texture.Height * 4];
            OpenGLHelper.BindTexture(texture.Handle);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 4);

            int visible = 0;
            for (int alphaIndex = 3; alphaIndex < pixels.Length; alphaIndex += 4)
            {
                if (pixels[alphaIndex] != 0)
                {
                    visible++;
                }
            }

            double percent = 100.0 * visible / (texture.Width * texture.Height);
            Logger.Information($"Texture probe on the main context: {texture.Source} ({texture.Width}x{texture.Height}) is {percent:F1}% non-transparent.");
        }

        Check("texture probe");
    }

    /// <summary>On probe frames, samples a grid of pixels from the bound framebuffer and logs how many are non-zero; no-op otherwise.</summary>
    public static void ProbeFramebuffer(string label, int width, int height)
    {
        if (!Enabled || !IsProbeFrame || width <= 0 || height <= 0)
        {
            return;
        }

        byte[] pixel = new byte[4];
        int nonZero = 0;
        string first = "none";
        GL.PixelStore(PixelStoreParameter.PackAlignment, 1);

        for (int row = 0; row < ProbeGridRows; row++)
        {
            for (int column = 0; column < ProbeGridColumns; column++)
            {
                int x = (int)((column + 0.5) / ProbeGridColumns * width);
                int y = (int)((row + 0.5) / ProbeGridRows * height);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixel);

                if ((pixel[0] | pixel[1] | pixel[2] | pixel[3]) != 0)
                {
                    if (nonZero == 0)
                    {
                        first = $"({x},{y}) rgba {pixel[0]},{pixel[1]},{pixel[2]},{pixel[3]}";
                    }

                    nonZero++;
                }
            }
        }

        GL.PixelStore(PixelStoreParameter.PackAlignment, 4);
        Logger.Information($"Frame probe (frame {s_frameNumber}): {label} {width}x{height} has {nonZero} of {ProbeGridColumns * ProbeGridRows} sample points non-zero; first {first}.");
        Check($"frame probe of {label}");
    }

    private static bool ClaimOnce(string key)
    {
        lock (s_logged)
        {
            return s_logged.Add(key);
        }
    }

    private static string TryHookDebugOutput()
    {
        try
        {
            s_callback ??= OnDebugMessage;
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

        if (ClaimOnce($"{id}|{text}|{stack}"))
        {
            Logger.Error($"GL debug [{type}, {severity}, source {source}, id {id}]: {text}{Environment.NewLine}{stack}");
        }
    }
}

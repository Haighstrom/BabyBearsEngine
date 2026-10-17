using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.ImageLoading;
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
    private const int MaxRecordedTextures = 4;
    private const int ProbeGridColumns = 16;
    private const int ProbeGridRows = 9;

    // Seconds after tracing starts at which the frame is probed; wall-clock so slow machines still get every probe.
    private static readonly double[] s_probeTimes = [3, 8, 15, 25];
    private static readonly Stopwatch s_clock = new();
    private static readonly HashSet<string> s_logged = [];
    private static readonly List<RecordedTexture> s_recordedTextures = [];
    private static DebugProc? s_callback;
    private static int s_frameNumber = 0;
    private static int s_nextProbeIndex = 0;
    private static bool s_probeThisFrame = false;

    private readonly record struct RecordedTexture(int Handle, int Width, int Height, string Source);

    public static bool Enabled { get; private set; } = false;


    /// <summary>Turns tracing on for the current context and logs any errors already pending; call as soon as the context is live.</summary>
    public static void Enable()
    {
        Enabled = true;
        s_clock.Start();
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
        if (!Enabled)
        {
            return;
        }

        s_frameNumber++;
        s_probeThisFrame = s_nextProbeIndex < s_probeTimes.Length && s_clock.Elapsed.TotalSeconds >= s_probeTimes[s_nextProbeIndex];

        if (s_probeThisFrame)
        {
            s_nextProbeIndex++;
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

    /// <summary>On probe frames, samples a grid of pixels from the bound framebuffer and logs their colour statistics; no-op otherwise.</summary>
    public static void ProbeFramebuffer(string label, int width, int height)
    {
        if (!Enabled || !s_probeThisFrame || width <= 0 || height <= 0)
        {
            return;
        }

        int nonBlack = 0;
        int transparent = 0;
        int maxChannel = 0;
        long redTotal = 0;
        long greenTotal = 0;
        long blueTotal = 0;
        string firstNonBlack = "none";
        byte[] pixel = new byte[4];
        GL.PixelStore(PixelStoreParameter.PackAlignment, 1);

        for (int row = 0; row < ProbeGridRows; row++)
        {
            for (int column = 0; column < ProbeGridColumns; column++)
            {
                int x = (int)((column + 0.5) / ProbeGridColumns * width);
                int y = (int)((row + 0.5) / ProbeGridRows * height);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixel);

                redTotal += pixel[0];
                greenTotal += pixel[1];
                blueTotal += pixel[2];
                maxChannel = Math.Max(maxChannel, Math.Max(pixel[0], Math.Max(pixel[1], pixel[2])));

                if (pixel[3] == 0)
                {
                    transparent++;
                }

                if ((pixel[0] | pixel[1] | pixel[2]) != 0)
                {
                    if (nonBlack == 0)
                    {
                        firstNonBlack = $"({x},{y}) rgba {pixel[0]},{pixel[1]},{pixel[2]},{pixel[3]}";
                    }

                    nonBlack++;
                }
            }
        }

        GL.PixelStore(PixelStoreParameter.PackAlignment, 4);
        int samples = ProbeGridColumns * ProbeGridRows;
        Logger.Information($"Frame probe (frame {s_frameNumber}, {s_clock.Elapsed.TotalSeconds:F1}s): {label} {width}x{height}: {nonBlack} of {samples} samples have colour, {transparent} fully transparent, brightest channel {maxChannel}, mean rgb {redTotal / samples},{greenTotal / samples},{blueTotal / samples}; first coloured sample {firstNonBlack}.");
        Check($"frame probe of {label}");
    }

    /// <summary>On probe frames, probes the window's back buffer and saves it as gl-probe-N.png beside the exe; call after drawing, before the buffer swap.</summary>
    public static void ProbeWindow(int width, int height)
    {
        if (!Enabled || !s_probeThisFrame || width <= 0 || height <= 0)
        {
            return;
        }

        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        GL.ReadBuffer(ReadBufferMode.Back);
        ProbeFramebuffer("window", width, height);

        try
        {
            byte[] raw = new byte[width * height * 4];
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, raw);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 4);

            byte[] flipped = new byte[raw.Length];
            int rowBytes = width * 4;
            for (int row = 0; row < height; row++)
            {
                Array.Copy(raw, (height - 1 - row) * rowBytes, flipped, row * rowBytes, rowBytes);
            }

            string path = Path.Combine(AppContext.BaseDirectory, $"gl-probe-{s_nextProbeIndex}.png");
            ImageSaver.SaveAsPng(new Rgba8ImageData(width, height, flipped), path);
            Logger.Information($"Saved what the window is drawing to {path}.");
        }
        catch (Exception exception)
        {
            Logger.Warning($"Could not save the window probe image: {exception.Message}");
        }

        Check("window probe capture");
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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Static helpers that emit engine-startup diagnostic sections to <see cref="Logger"/>. Called by
/// <c>GameLauncher</c> and <c>OpenTKGameEngine.OnLoad</c> during initialisation. Each method writes
/// a banner-style section block — readers of <c>log.log</c> get a structured snapshot of the runtime
/// environment without having to grep for individual <c>Info</c> lines.
/// </summary>
internal static class EngineDiagnostics
{
    /// <summary>Emits the System Information section. Call right after <see cref="Logger.Initialise"/>.</summary>
    public static void LogSystemInformation()
    {
        Logger.Section("System Information",
        [
            $"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.ProcessArchitecture})",
            $"Runtime: {RuntimeInformation.FrameworkDescription}",
            $"Processors: {Environment.ProcessorCount}",
        ]);
    }

    /// <summary>
    /// Emits the OpenGL Context and Display Devices sections. Call from <c>OpenTKGameEngine.OnLoad</c>
    /// after the GL context is live — requires <see cref="GL.GetString"/> to be callable.
    /// </summary>
    public static void LogStartupContext(LogSettings logSettings)
    {
        ArgumentNullException.ThrowIfNull(logSettings);

        string dedupe = logSettings.DedupeGLErrors
            ? "enabled (GL errors fire once per call site)"
            : "disabled (GL errors fire every time)";

        Logger.Section("OpenGL Context",
        [
            $"OpenGL {GL.GetString(StringName.Version)}",
            $"GLSL {GL.GetString(StringName.ShadingLanguageVersion)}",
            $"Vendor: {GL.GetString(StringName.Vendor)}",
            $"Renderer: {GL.GetString(StringName.Renderer)}",
            $"GL error deduplication: {dedupe}",
        ]);

        var monitors = Monitors.GetMonitors();
        var lines = new List<string>(monitors.Count);
        for (int i = 0; i < monitors.Count; i++)
        {
            MonitorInfo m = monitors[i];
            // GLFW guarantees index 0 of GetMonitors() is the primary monitor.
            string primaryMark = i == 0 ? " (primary)" : string.Empty;
            lines.Add($"Display {i + 1}: {m.Name}: {m.HorizontalResolution}x{m.VerticalResolution} @ {m.CurrentVideoMode.RefreshRate}Hz{primaryMark}");
        }

        Logger.Section("Display Devices", lines);
    }

    /// <summary>Emits a one-line marker indicating engine startup is complete. Call at the end of <c>OnLoad</c>.</summary>
    public static void LogInitialisationComplete()
    {
        Logger.SectionMarker($"BabyBearsEngine Initialised at {DateTime.Now:HH:mm:ss.fff}");
    }
}

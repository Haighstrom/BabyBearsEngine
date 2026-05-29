using System.IO;
using BabyBearsEngine.Diagnostics;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// GPU capability snapshot queried once at startup. Holds the OpenGL/GLSL version the driver
/// actually reported (which may differ from <see cref="WindowSettings.OpenGLVersion"/>) and
/// limits used to gate optional features such as MSAA. Populated from
/// <see cref="OpenTKGameEngine.OnLoad"/> once the GL context is live, and consumed by the
/// engine-wide version gate, the per-shader version gate, and the MSAA sample clamp.
/// </summary>
public sealed class GpuCapabilities
{
    /// <summary>
    /// The minimum OpenGL version BabyBearsEngine will start under. Set to the lowest
    /// <c>#version</c> floor required by any shader the engine compiles up front. Below this,
    /// <see cref="EnforceEngineMinimum"/> refuses to start with a clear diagnostic rather than
    /// letting individual shader compiles fail with cryptic GLSL errors.
    /// </summary>
    public static readonly Version EngineMinimumOpenGL = new(3, 2);

    private static GpuCapabilities? s_current;

    public GpuCapabilities(Version openGLVersion, int glslVersion, int maxMsaaSamples)
    {
        ArgumentNullException.ThrowIfNull(openGLVersion);
        OpenGLVersion = openGLVersion;
        GlslVersion = glslVersion;
        MaxMsaaSamples = maxMsaaSamples;
    }

    /// <summary>
    /// The capability snapshot for the current run. Populated in <c>OnLoad</c> after the GL
    /// context is live. Throws if accessed before then or after engine teardown.
    /// </summary>
    public static GpuCapabilities Current
        => s_current ?? throw new InvalidOperationException(
            "GpuCapabilities has not been populated. PopulateFromGL() must be called from OnLoad before any shader compiles.");

    /// <summary>
    /// True when <see cref="Current"/> has been populated. Used by code that runs both before
    /// and after the GL context is live (e.g. shader compile paths that may run in tests
    /// without a GL context).
    /// </summary>
    public static bool IsAvailable => s_current is not null;

    /// <summary>The OpenGL version the driver reported (not necessarily what was requested).</summary>
    public Version OpenGLVersion { get; }

    /// <summary>
    /// The GLSL version corresponding to <see cref="OpenGLVersion"/>, e.g. <c>330</c> for GL 3.3.
    /// Matches the integer used in <c>#version NNN</c> shader directives.
    /// </summary>
    public int GlslVersion { get; }

    /// <summary>The maximum MSAA sample count this GPU supports (<c>GL_MAX_SAMPLES</c>).</summary>
    public int MaxMsaaSamples { get; }

    /// <summary>
    /// Maps an OpenGL (major, minor) version to its corresponding GLSL <c>#version</c> integer.
    /// From GL 3.3 the GLSL version is <c>major*100 + minor*10</c>; earlier versions use the
    /// historical exceptions (GL 3.0→130, 3.1→140, 3.2→150, 2.1→120, 2.0→110). Returns 0 for
    /// pre-2.0 contexts which have no fixed GLSL version.
    /// </summary>
    public static int OpenGLToGlslVersion(int major, int minor)
    {
        int combined = (major * 10) + minor;
        return combined switch
        {
            >= 33 => combined * 10,
            32 => 150,
            31 => 140,
            30 => 130,
            21 => 120,
            20 => 110,
            _ => 0,
        };
    }

    /// <summary>
    /// Parses the <c>#version NNN [core|compatibility]</c> directive from the given shader
    /// source and returns the version number, or null when no <c>#version</c> directive is
    /// present at the head of the file. Skips leading blank lines and <c>//</c> line comments.
    /// </summary>
    public static int? TryParseGlslVersion(string shaderSource)
    {
        ArgumentNullException.ThrowIfNull(shaderSource);

        using var reader = new StringReader(shaderSource);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            string trimmed = line.TrimStart();
            if (trimmed.Length == 0)
            {
                continue;
            }
            if (trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }
            if (trimmed.StartsWith("#version", StringComparison.Ordinal))
            {
                string[] parts = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && int.TryParse(parts[1], out int version))
                {
                    return version;
                }
                return null;
            }
            return null;
        }
        return null;
    }

    /// <summary>
    /// Returns null when the driver granted at least the requested OpenGL version; otherwise
    /// returns a human-readable warning message describing the shortfall. Callers route the
    /// message through <see cref="Logger.Warning"/>. Falling short is not fatal so long as
    /// the granted version meets <see cref="EngineMinimumOpenGL"/> — some individual shaders
    /// may still throw <see cref="ShaderRequiresGLVersionException"/> later if their
    /// <c>#version</c> pragma exceeds the granted version.
    /// </summary>
    public static string? GetGrantedBelowRequestedWarning(Version requested, Version granted)
    {
        ArgumentNullException.ThrowIfNull(requested);
        ArgumentNullException.ThrowIfNull(granted);

        if (granted >= requested)
        {
            return null;
        }

        return $"Requested OpenGL {requested.Major}.{requested.Minor} but driver granted " +
            $"{granted.Major}.{granted.Minor}. The engine will continue (above the " +
            $"{EngineMinimumOpenGL.Major}.{EngineMinimumOpenGL.Minor} minimum) but shaders " +
            $"requiring higher GLSL versions may fail to load.";
    }

    /// <summary>
    /// Throws <see cref="EngineInitialisationException"/> when <paramref name="reported"/> is
    /// below <paramref name="minimum"/>. Called from <c>OnLoad</c> before any shader compiles.
    /// </summary>
    public static void EnforceEngineMinimum(Version reported, Version minimum)
    {
        ArgumentNullException.ThrowIfNull(reported);
        ArgumentNullException.ThrowIfNull(minimum);

        if (reported < minimum)
        {
            throw new EngineInitialisationException(
                $"OpenGL {reported.Major}.{reported.Minor} is below the engine minimum of " +
                $"{minimum.Major}.{minimum.Minor}. Update your GPU driver or run on hardware " +
                $"with at least OpenGL {minimum.Major}.{minimum.Minor} support.");
        }
    }

    /// <summary>
    /// Throws <see cref="ShaderRequiresGLVersionException"/> when a shader's required GLSL
    /// version exceeds the GPU's supported GLSL version. Called from
    /// <see cref="IShaderSourceProvider"/> implementations after loading shader source.
    /// </summary>
    public static void EnforceShaderRequirement(string shaderIdentifier, int shaderGlslVersion, int gpuGlslVersion)
    {
        ArgumentNullException.ThrowIfNull(shaderIdentifier);

        if (shaderGlslVersion > gpuGlslVersion)
        {
            throw new ShaderRequiresGLVersionException(shaderIdentifier, shaderGlslVersion, gpuGlslVersion);
        }
    }

    /// <summary>
    /// Convenience overload that parses the shader source's <c>#version</c> directive and
    /// checks against <see cref="Current"/>. No-ops when <see cref="IsAvailable"/> is false
    /// (e.g. unit-test paths without a GL context) or when the source has no <c>#version</c>
    /// directive. Call from source providers right after loading.
    /// </summary>
    public static void EnforceShaderRequirement(string shaderIdentifier, string shaderSource)
    {
        ArgumentNullException.ThrowIfNull(shaderIdentifier);
        ArgumentNullException.ThrowIfNull(shaderSource);

        if (!IsAvailable)
        {
            return;
        }

        int? required = TryParseGlslVersion(shaderSource);
        if (required is int requiredVersion)
        {
            EnforceShaderRequirement(shaderIdentifier, requiredVersion, Current.GlslVersion);
        }
    }

    /// <summary>
    /// Queries the live GL context and stores the result in <see cref="Current"/>. Call from
    /// <c>OnLoad</c> after <c>base.OnLoad()</c> and before any shader compiles.
    /// </summary>
    internal static void PopulateFromGL()
    {
        int major = GL.GetInteger(GetPName.MajorVersion);
        int minor = GL.GetInteger(GetPName.MinorVersion);
        int maxSamples = GL.GetInteger(GetPName.MaxSamples);

        var version = new Version(major, minor);
        int glsl = OpenGLToGlslVersion(major, minor);

        s_current = new GpuCapabilities(version, glsl, maxSamples);
    }

    /// <summary>Clears <see cref="Current"/> at engine teardown.</summary>
    internal static void Reset() => s_current = null;

    /// <summary>
    /// Test seam: sets <see cref="Current"/> directly without hitting the GL context. Tests
    /// must call <see cref="Reset"/> in teardown to avoid state bleeding between cases.
    /// </summary>
    internal static void SetForTesting(GpuCapabilities? capabilities) => s_current = capabilities;
}

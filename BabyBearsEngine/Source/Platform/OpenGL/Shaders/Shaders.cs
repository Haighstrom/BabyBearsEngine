namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Public facade for accessing the engine's built-in shader programs.
///
/// Stateless shaders that hold no per-instance configuration are exposed as properties
/// returning a shared singleton — compiled and linked once, then re-used. Their lifetime is
/// managed by <see cref="EngineTeardown.ResetForNextRun"/>, which drops the cached instance
/// between consecutive <see cref="GameLauncher.Run"/> invocations so the next run doesn't
/// reuse a shader handle from a torn-down GL context.
///
/// Parameterised shaders (whose constructor argument is baked in for the shader's lifetime —
/// blur radius, darken value, MSAA sample count) are exposed as <c>New…</c> factory methods that
/// return a fresh instance per call. Caller owns the returned object's lifetime.
///
/// Call <see cref="LoadAllLazyShaders"/> from a loading screen or boot sequence to instantiate
/// every singleton ahead of time and avoid GL compilation/link stutters mid-frame.
/// </summary>
public static class Shaders
{
    // -------- Singletons --------

    /// <summary>Shared <see cref="CoverageTextShaderProgram"/> — paired with R8 coverage atlases by the FreeType text renderer.</summary>
    public static CoverageTextShaderProgram CoverageText => CoverageTextShaderProgram.Instance;

    /// <summary>Shared <see cref="GreyscaleShaderProgram"/> — maps each sampled colour to its luminance.</summary>
    public static GreyscaleShaderProgram Greyscale => GreyscaleShaderProgram.Instance;

    /// <summary>Shared <see cref="ParticleShaderProgram"/> — used by <see cref="Worlds.Particles.ParticleSystem"/> for untextured particles.</summary>
    public static ParticleShaderProgram Particle => ParticleShaderProgram.Instance;

    /// <summary>Shared <see cref="SdfTextShaderProgram"/> — paired with the SDF font atlas.</summary>
    public static SdfTextShaderProgram SdfText => SdfTextShaderProgram.Instance;

    /// <summary>Shared <see cref="SolidColourShaderProgramMatrix"/> — solid-fill matrix-aware shader, used by <see cref="Graphics.ColourGraphic"/>.</summary>
    public static SolidColourShaderProgramMatrix SolidColour => SolidColourShaderProgramMatrix.Instance;

    /// <summary>Shared <see cref="StandardMatrixShaderProgram"/> — the default matrix-aware textured shader.</summary>
    public static StandardMatrixShaderProgram Standard => StandardMatrixShaderProgram.Instance;

    /// <summary>Shared <see cref="TexturedParticleShaderProgram"/> — used by <see cref="Worlds.Particles.ParticleSystem"/> for textured particles.</summary>
    public static TexturedParticleShaderProgram TexturedParticle => TexturedParticleShaderProgram.Instance;

    // -------- Per-call factories (parameterised) --------

    /// <summary>Creates a fresh <see cref="BlurShaderProgram"/> with the given Gaussian blur radius.</summary>
    public static BlurShaderProgram NewBlur(float blurSize = 2.0f) => new(blurSize);

    /// <summary>Creates a fresh <see cref="DarkenShaderProgram"/> with the given darken intensity.</summary>
    public static DarkenShaderProgram NewDarken(float darkenValue = 1.0f) => new(darkenValue);

    /// <summary>
    /// Creates a fresh <see cref="DefaultShaderProgram"/>. Not a singleton because the shader
    /// subscribes to <see cref="Window.Resize"/> in its constructor and unsubscribes in
    /// <see cref="ShaderProgramBase.Dispose"/>; sharing one instance across the lifetime of the
    /// engine would leak the old delegate when the window adapter is replaced between runs.
    /// </summary>
    public static DefaultShaderProgram NewDefault() => new();

    // -------- Lifecycle --------

    /// <summary>
    /// Eagerly instantiates every singleton shader so GL compilation and linking happens now
    /// rather than mid-frame on first use. Intended for a loading screen or game boot — call
    /// once after the GL context is up. Safe to call multiple times (subsequent calls are no-ops).
    /// </summary>
    public static void LoadAllLazyShaders()
    {
        _ = CoverageText;
        _ = Greyscale;
        _ = Particle;
        _ = SdfText;
        _ = SolidColour;
        _ = Standard;
        _ = TexturedParticle;
    }
}

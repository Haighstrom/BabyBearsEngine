namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-aware shader for single-channel grayscale coverage text. Samples the R8 coverage atlas
/// produced by <see cref="Worlds.Graphics.Text.FreeTypeFontAtlasGenerator"/> and tints it by the
/// glyph colour (see <c>r8_texture.frag</c>). Unlike <see cref="SdfTextShaderProgram"/> the atlas
/// stores alpha coverage rather than distance, so no edge reconstruction happens in the shader —
/// the FreeType rasteriser already authored the antialiased edge at the target pixel size.
/// </summary>
public sealed class CoverageTextShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<CoverageTextShaderProgram> s_instance = new(() => new CoverageTextShaderProgram());

    public static CoverageTextShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<CoverageTextShaderProgram>(() => new CoverageTextShaderProgram());
    }

    private CoverageTextShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.R8Texture)
    {
    }
}

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-aware shader for single-channel signed distance field text. Samples the R8 distance
/// atlas produced by <see cref="Worlds.Graphics.Text.SdfFontAtlasGenerator"/> and reconstructs a
/// resolution-independent antialiased edge in the fragment stage (see <c>sdf_text.frag</c>).
/// </summary>
public sealed class SdfTextShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<SdfTextShaderProgram> s_instance = new(() => new SdfTextShaderProgram());

    public static SdfTextShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<SdfTextShaderProgram>(() => new SdfTextShaderProgram());
    }

    private SdfTextShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.SdfText)
    {
    }
}

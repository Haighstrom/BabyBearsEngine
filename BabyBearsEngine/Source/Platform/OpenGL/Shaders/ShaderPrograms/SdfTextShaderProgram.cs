using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-aware shader for single-channel signed distance field text. Samples the R8 distance
/// atlas produced by <see cref="Worlds.Graphics.Text.SdfFontAtlasGenerator"/> and reconstructs a
/// resolution-independent antialiased edge in the fragment stage (see <c>sdf_text.frag</c>).
/// </summary>
public sealed class SdfTextShaderProgram : ShaderProgramBase, IMatrixShaderProgram
{
    private static Lazy<SdfTextShaderProgram> s_instance = new(() => new SdfTextShaderProgram());

    public static SdfTextShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<SdfTextShaderProgram>(() => new SdfTextShaderProgram());
    }

    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    private SdfTextShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.SdfText)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        var mvMatrix = Matrix3.Identity;
        SetModelViewMatrix(ref mvMatrix);
    }

    public void SetModelViewMatrix(ref Matrix3 matrix)
    {
        Bind();

        unsafe
        {
            fixed (float* valuePointer = matrix.Values)
            {
                GL.UniformMatrix3(_mvMatrixLocation, 1, false, valuePointer);
            }
        }
    }

    public void SetProjectionMatrix(ref Matrix3 matrix)
    {
        Bind();

        unsafe
        {
            fixed (float* valuePointer = matrix.Values)
            {
                GL.UniformMatrix3(_pMatrixLocation, 1, false, valuePointer);
            }
        }
    }
}

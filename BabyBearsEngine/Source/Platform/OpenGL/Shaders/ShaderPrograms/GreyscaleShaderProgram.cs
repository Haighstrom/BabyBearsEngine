using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-transformed textured-quad shader that maps each sampled colour to its luminance
/// (dot(rgb, 0.299, 0.587, 0.114)), leaving alpha unchanged. Assign to
/// <see cref="Worlds.Graphics.TextureGraphic.Shader"/> or <see cref="Worlds.Graphics.Sprite.Shader"/>
/// to greyscale a sprite.
/// </summary>
public sealed class GreyscaleShaderProgram : ShaderProgramBase, IMatrixShaderProgram
{
    private static Lazy<GreyscaleShaderProgram> s_instance = new(() => new GreyscaleShaderProgram());

    public static GreyscaleShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<GreyscaleShaderProgram>(() => new GreyscaleShaderProgram());
    }

    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    private GreyscaleShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.Greyscale)
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

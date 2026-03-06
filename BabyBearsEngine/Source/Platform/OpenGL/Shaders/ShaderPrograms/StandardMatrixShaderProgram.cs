using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;

namespace BabyBearsEngine.OpenGL;

public class StandardMatrixShaderProgram : ShaderProgramBase, IMVPShader
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    public StandardMatrixShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.Default)
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

    public void SetProjectionMatrix(int width, int height)
    {
        var pMatrix = Matrix3.CreateOrtho(width, height);
        SetProjectionMatrix(ref pMatrix);
    }
}

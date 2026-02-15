using BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Mathematics;

namespace BabyBearsEngine.OpenGL;

public class StandardMatrixShaderProgram : ShaderProgramBase, IWorldShader
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

    public void SetModelViewMatrix(ref Matrix3 modelViewMatrix)
    {
        Bind();
        GL.UniformMatrix3(_mvMatrixLocation, true, ref modelViewMatrix);
    }

    public void SetModelViewMatrix(Source.Geometry.Matrix3 matrix)
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

    public void SetProjectionMatrix(int width, int height)
    {
        var pMatrix = OpenGLHelper.CreateOrthographicProjectionMatrix(width, height);
        SetProjectionMatrix(ref pMatrix);
    }

    public void SetProjectionMatrix(ref Matrix3 projectionMatrix)
    {
        Bind();
        GL.UniformMatrix3(_pMatrixLocation, true, ref projectionMatrix);
    }

    public void SetProjectionMatrix(Source.Geometry.Matrix3 matrix)
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

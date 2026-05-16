using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;

namespace BabyBearsEngine.OpenGL;

internal sealed class StencilShaderProgram : ShaderProgramBase, IMVPShader
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;
    private readonly int _thresholdLocation;

    public StencilShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.Stencil)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");
        _thresholdLocation = GL.GetUniformLocation(Handle, "Threshold");

        // Bind each sampler uniform to its texture unit once at construction time.
        Bind();
        GL.Uniform1(GL.GetUniformLocation(Handle, "ImageSampler"), 0);
        GL.Uniform1(GL.GetUniformLocation(Handle, "StencilSampler"), 1);

        var mvMatrix = Matrix3.Identity;
        SetModelViewMatrix(ref mvMatrix);
    }

    public void SetThreshold(float threshold)
    {
        Bind();
        GL.Uniform1(_thresholdLocation, threshold);
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

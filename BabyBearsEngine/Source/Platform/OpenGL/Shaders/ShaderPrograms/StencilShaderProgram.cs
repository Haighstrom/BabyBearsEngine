using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

internal sealed class StencilShaderProgram : ShaderProgramBase, IMatrixShaderProgram
{
    private static Lazy<StencilShaderProgram> s_instance = new(() => new StencilShaderProgram());

    public static StencilShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<StencilShaderProgram>(() => new StencilShaderProgram());
    }

    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;
    private readonly int _thresholdLocation;

    private StencilShaderProgram()
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

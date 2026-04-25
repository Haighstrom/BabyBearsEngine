using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;

namespace BabyBearsEngine.Source.Worlds.Cameras;

/// <summary>
/// Shader used with the multisample FBO for the MSAA antialiasing pass only
/// </summary>
public sealed class CameraMSAAShader : ShaderProgramBase, IMVPShader
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;
    private readonly int _locationSamplesUniform;

    private MsaaSamples _samples;

    public CameraMSAAShader(MsaaSamples samples = MsaaSamples.Disabled)
        : base(VertexShaders.CameraMSAA, FragmentShaders.CameraMSAA)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");
        _locationSamplesUniform = GL.GetUniformLocation(Handle, "MSAASamples");

        var mvMatrix = Matrix3.Identity;
        SetModelViewMatrix(ref mvMatrix);

        Samples = samples;
    }

    public MsaaSamples Samples
    {
        get => _samples;
        set
        {
            _samples = value;
            SetSamples((int)_samples);
        }
    }

    private void SetSamples(int samples) 
    {
        Bind();
        GL.Uniform1(_locationSamplesUniform, samples);
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

    public void SetProjectionMatrix(int width, int height)
    {
        var pMatrix = Matrix3.CreateOrtho(width, height);
        SetProjectionMatrix(ref pMatrix);
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

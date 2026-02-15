using BabyBearsEngine.OpenGL;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Worlds.Cameras;

/// <summary>
/// Shader used with the multisample FBO for the MSAA antialiasing pass only
/// </summary>
public class CameraMSAAShader : ShaderProgramBase
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;
    private readonly int _locationSamplesUniform;

    public CameraMSAAShader(int width, int height, MsaaSamples samples = MsaaSamples.Disabled)
        : base(VertexShaders.CameraMSAA, FragmentShaders.CameraMSAA)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");
        _locationSamplesUniform = GL.GetUniformLocation(Handle, "MSAASamples");

        var mvMatrix = Matrix3.Identity;
        SetModelViewMatrix(ref mvMatrix);

        SetProjectionMatrix(width, height);

        Samples = samples;
    }

    private MsaaSamples _samples;
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

    public void SetModelViewMatrix(ref Matrix3 modelViewMatrix)
    {
        Bind();
        GL.UniformMatrix3(_mvMatrixLocation, true, ref modelViewMatrix);
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
}

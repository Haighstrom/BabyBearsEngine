using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Cameras;

/// <summary>
/// Shader used with the multisample FBO for the MSAA antialiasing pass only
/// </summary>
internal sealed class CameraMSAAShader : MatrixShaderProgramBase
{
    private readonly int _locationSamplesUniform;
    private MsaaSamples _samples;

    public CameraMSAAShader(MsaaSamples samples = MsaaSamples.Disabled)
        : base(VertexShaders.CameraMSAA, FragmentShaders.CameraMSAA)
    {
        _locationSamplesUniform = GL.GetUniformLocation(Handle, "MSAASamples");
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
}

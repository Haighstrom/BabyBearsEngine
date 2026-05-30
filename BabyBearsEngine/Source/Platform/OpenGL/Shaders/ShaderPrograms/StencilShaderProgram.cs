using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

internal sealed class StencilShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<StencilShaderProgram> s_instance = new(() => new StencilShaderProgram());

    public static StencilShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<StencilShaderProgram>(() => new StencilShaderProgram());
    }

    private readonly int _thresholdLocation;

    private StencilShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.Stencil)
    {
        _thresholdLocation = GL.GetUniformLocation(Handle, "Threshold");

        // Bind each sampler uniform to its texture unit once at construction time.
        Bind();
        GL.Uniform1(GL.GetUniformLocation(Handle, "ImageSampler"), 0);
        GL.Uniform1(GL.GetUniformLocation(Handle, "StencilSampler"), 1);
    }

    public void SetThreshold(float threshold)
    {
        Bind();
        GL.Uniform1(_thresholdLocation, threshold);
    }
}

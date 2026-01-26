using OpenTK.Mathematics;

namespace BabyBearsEngine.OpenGL;

public class R8ChannelShaderProgram : ShaderProgramBase
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    public R8ChannelShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.R8Texture)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        var matrix = Matrix3.Identity;
        SetModelViewMatrix(ref matrix);

        SetProjectionMatrix(Window.Width, Window.Height);

        Window.Resize += args => SetProjectionMatrix(args.Width, args.Height);
    }

    private void SetModelViewMatrix(ref Matrix3 modelViewMatrix)
    {
        Bind();
        GL.UniformMatrix3(_mvMatrixLocation, true, ref modelViewMatrix);
    }

    private void SetProjectionMatrix(int width, int height)
    {
        var pMatrix = OpenGLHelper.CreateOrthographicProjectionMatrix(width, height);
        SetProjectionMatrix(ref pMatrix);
    }

    private void SetProjectionMatrix(ref Matrix3 projectionMatrix)
    {
        Bind();
        GL.UniformMatrix3(_pMatrixLocation, true, ref projectionMatrix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Window.Resize -= args => SetProjectionMatrix(args.Width, args.Height);
        }

        base.Dispose(disposing);
    }
}

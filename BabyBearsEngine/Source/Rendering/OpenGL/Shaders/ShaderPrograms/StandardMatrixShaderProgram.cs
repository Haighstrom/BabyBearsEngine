using OpenTK.Mathematics;

namespace BabyBearsEngine.OpenGL;

public class StandardMatrixShaderProgram : ShaderProgramBase
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

        SetProjectionMatrix(Window.Width, Window.Height);

        Window.Resize += args => SetProjectionMatrix(args.Width, args.Height);
    }

    public void SetModelViewMatrix(ref Matrix3 modelViewMatrix)
    {
        Bind();
        GL.UniformMatrix3(_mvMatrixLocation, true, ref modelViewMatrix);
    }

    private void SetProjectionMatrix(int width, int height)
    {
        var pMatrix = OpenGLHelper.CreateOrthographicProjectionMatrix(width, height);
        SetProjectionMatrix(ref pMatrix);
    }

    public void SetProjectionMatrix(ref Matrix3 projectionMatrix)
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

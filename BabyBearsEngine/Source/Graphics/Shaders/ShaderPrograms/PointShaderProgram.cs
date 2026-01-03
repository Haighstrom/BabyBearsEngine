using OpenTK.Mathematics;
using BabyBearsEngine.Source.Core;

namespace BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;

public class PointShaderProgram : ShaderProgramBase
{
    private readonly int _pMatrixLocation;

    public PointShaderProgram()
        :base(VertexShaders.Point, FragmentShaders.Point)
    {
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        GL.Enable(EnableCap.ProgramPointSize);

        var pMatrix = OpenGLHelper.CreateOrthographicProjectionMatrix(Window.Width, Window.Height);
        SetProjectionMatrix(ref pMatrix);

        Window.Resize += args => SetProjectionMatrix(args.Width, args.Height);
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

    public void SetPointSize(float size)
    {
        Bind();
        var location = GL.GetUniformLocation(Handle, "PointSize");
        GL.Uniform1(location, size);
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

using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using BabyBearsEngine.Source.Core;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class PointShaderProgram : ShaderProgramBase
{
    private readonly int _pMatrixLocation;

    public PointShaderProgram()
    {
        string vsSource = File.ReadAllText("Assets/Shaders/point.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/point.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        Window.Resize += Window_Resize;

        GL.Enable(EnableCap.ProgramPointSize);
    }

    public override int Handle { get; }

    private void Window_Resize(ResizeEventArgs args)
    {
        var ortho = OpenGLHelper.CreateOrthographicProjectionMatrix(args.Width, args.Height);

        SetProjectionMatrix(ref ortho);
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
}

using System.IO;
using BabyBearsEngine.Source.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class StandardMatrixShaderProgram : ShaderProgramBase
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    public StandardMatrixShaderProgram()
    {
        string vsSource = File.ReadAllText("Assets/Shaders/vs_default.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/fs_default.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        var matrix = Matrix3.Identity;
        SetModelViewMatrix(ref matrix);

        Window.Resize += Window_Resize;
    }

    public override int Handle { get; }

    private void Window_Resize(ResizeEventArgs args)
    {
        var ortho = OpenGLHelper.CreateOrthographicProjectionMatrix(args.Width, args.Height);

        SetProjectionMatrix(ref ortho);
    }

    public void SetModelViewMatrix(ref Matrix3 modelViewMatrix)
    {
        Bind();
        GL.UniformMatrix3(_mvMatrixLocation, true, ref modelViewMatrix);
    }

    public void SetProjectionMatrix(ref Matrix3 projectionMatrix)
    {
        Bind();
        GL.UniformMatrix3(_pMatrixLocation, true, ref projectionMatrix);
    }
}

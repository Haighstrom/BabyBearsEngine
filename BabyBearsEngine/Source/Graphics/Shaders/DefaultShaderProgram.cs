using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class DefaultShaderProgram : ShaderProgramBase
{
    private readonly int _windowSizeLocation;

    public DefaultShaderProgram(GameWindow window)
    {
        string vsSource = File.ReadAllText("Assets/Shaders/shader.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/shader.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

        _windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");

        window.Resize += Window_Resize;
    }

    public override int Handle { get; }

    private void Window_Resize(ResizeEventArgs args)
    {
        Bind();
        GL.Uniform2(_windowSizeLocation, new Vector2(args.Width, args.Height));
    }
}

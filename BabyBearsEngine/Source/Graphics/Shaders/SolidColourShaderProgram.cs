using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class SolidColourShaderProgram : ShaderProgramBase
{
    public SolidColourShaderProgram(GameWindow window)
    {
        string vsSource = File.ReadAllText("Assets/Shaders/vs_nomatrixsolidcolour.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/fs_solidcolour.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

        window.Resize += Window_Resize;
    }

    public override int Handle { get; }

    private void Window_Resize(ResizeEventArgs args)
    {
        Bind();
        var windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");
        GL.Uniform2(windowSizeLocation, new Vector2(args.Width, args.Height));
    }
}

using System.IO;
using BabyBearsEngine.Source.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public sealed class SolidColourShaderProgram : ShaderProgramBase
{
    private static readonly Lazy<SolidColourShaderProgram> s_instance = new(() => new SolidColourShaderProgram());

    public static SolidColourShaderProgram Instance => s_instance.Value;

    private SolidColourShaderProgram()
    {
        string vsSource = File.ReadAllText("Assets/Shaders/vs_nomatrixsolidcolour.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/fs_solidcolour.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

        Window.Resize += Window_Resize;
    }

    public override int Handle { get; }

    private void Window_Resize(ResizeEventArgs args)
    {
        Bind();
        int windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");
        GL.Uniform2(windowSizeLocation, new Vector2(args.Width, args.Height));
    }
}

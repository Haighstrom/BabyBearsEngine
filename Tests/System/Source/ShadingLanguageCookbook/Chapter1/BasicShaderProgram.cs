using System.IO;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Graphics.Shaders;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;
internal class BasicShaderProgram : ShaderProgramBase
{
    public BasicShaderProgram()
    {
        string vsSource = File.ReadAllText("Assets/basic.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/basic.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);
    }

    public override int Handle { get; }
}

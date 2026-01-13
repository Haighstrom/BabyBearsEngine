using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;

internal class BasicShaderProgram : ShaderProgramBase
{
    public BasicShaderProgram()
        : base(VertexShaders.Shader, FragmentShaders.Shader)
    {
    }
}

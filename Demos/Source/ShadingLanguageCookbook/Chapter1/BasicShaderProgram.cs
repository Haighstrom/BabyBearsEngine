using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Demos.Source.ShadingLanguageCookbook.Chapter1;

internal class BasicShaderProgram : ShaderProgramBase
{
    public BasicShaderProgram()
        : base(VertexShaders.Shader, FragmentShaders.Shader)
    {
    }
}

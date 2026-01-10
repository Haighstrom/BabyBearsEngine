using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;
using BabyBearsEngine.Source.Graphics.Textures;

namespace BabyBearsEngine.Source.Graphics.MemoryManagement;

internal interface IGPUMemoryDisposer
{
    void RequestDeleteShader(IShaderProgram shaderProgram);
    void RequestDeleteTexture(ITexture texture);
    void RequestDeleteVAO(VAO vao);
    void RequestDeleteVBO(VBO vbo);
    void RequestDeleteEBO(EBO ebo);
    void ProcessDeletes();
}

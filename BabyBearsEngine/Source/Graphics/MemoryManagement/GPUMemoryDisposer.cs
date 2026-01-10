using System.Collections.Generic;
using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;
using BabyBearsEngine.Source.Graphics.Textures;

namespace BabyBearsEngine.Source.Graphics.MemoryManagement;

internal static class GPUMemoryDisposer
{
    private static IGPUMemoryDisposer s_instance = new DefaultGPUMemoryDisposer();

    public static void RequestDeleteShader(IShaderProgram shaderProgram) => s_instance.RequestDeleteShader(shaderProgram);

    public static void RequestDeleteTexture(ITexture texture) => s_instance.RequestDeleteTexture(texture);

    public static void RequestDeleteVAO(VAO vao) => s_instance.RequestDeleteVAO(vao);

    public static void RequestDeleteVBO(VBO vbo) => s_instance.RequestDeleteVBO(vbo);

    public static void RequestDeleteEBO(EBO ebo) => s_instance.RequestDeleteEBO(ebo);

    public static void ProcessDeletes() => s_instance.ProcessDeletes();
}

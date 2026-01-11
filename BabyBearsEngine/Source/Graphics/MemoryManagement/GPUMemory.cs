using System.Collections.Generic;
using BabyBearsEngine.Source.Boot;
using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;
using BabyBearsEngine.Source.Graphics.Textures;
using BabyBearsEngine.Source.Services;

namespace BabyBearsEngine.Source.Graphics.MemoryManagement;

internal static class GPUMemory
{
    private static IGPUMemoryService Service => GameServices.GPUMemoryService;

    public static void RequestDeleteShader(IShaderProgram shaderProgram) => Service.RequestDeleteShader(shaderProgram);

    public static void RequestDeleteTexture(ITexture texture) => Service.RequestDeleteTexture(texture);

    public static void RequestDeleteVAO(VAO vao) => Service.RequestDeleteVAO(vao);

    public static void RequestDeleteVBO(VBO vbo) => Service.RequestDeleteVBO(vbo);

    public static void RequestDeleteEBO(EBO ebo) => Service.RequestDeleteEBO(ebo);

    public static void ProcessDeletes() => Service.ProcessDeletes();
}

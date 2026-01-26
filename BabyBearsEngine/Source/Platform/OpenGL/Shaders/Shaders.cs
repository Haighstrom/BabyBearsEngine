using System;
using System.Collections.Generic;
using System.Text;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Source.Platform.OpenGL.Shaders;

internal static class Shaders
{
    public static ShaderReference Default = new("default");
    public static ShaderReference Greyscale = new("grayscale");
}

internal static class ShaderMapping
{
    private static readonly Dictionary<ShaderReference, ShaderProgramBase> s_shaderMap = [];

    public static void RegisterShader(ShaderReference reference, ShaderProgramBase shaderProgram)
    {
        s_shaderMap[reference] = shaderProgram;
    }

    public static ShaderProgramBase GetShader(ShaderReference reference)
    {
        if (s_shaderMap.TryGetValue(reference, out var shaderProgram))
        {
            return shaderProgram;
        }
        throw new KeyNotFoundException($"Shader with reference '{reference.reference}' not found.");
    }
}

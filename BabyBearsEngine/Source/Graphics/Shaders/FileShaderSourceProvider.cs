using System.Collections.Generic;
using System.IO;

namespace BabyBearsEngine.Source.Graphics.Shaders;

internal sealed class FileShaderSourceProvider : IShaderSourceProvider
{
    private readonly Dictionary<string, string> _shaderSourceByPath = new(StringComparer.Ordinal);

    public string GetFragmentSource(FragmentShaderPath fragmentShaderPath) => GetSource(fragmentShaderPath.Path);

    public string GetVertexSource(VertexShaderPath vertexShaderPath) => GetSource(vertexShaderPath.Path);

    public string GetGeometrySource(GeometryShaderPath geometryShaderPath) => GetSource(geometryShaderPath.Path);

    private string GetSource(string shaderPath)
    {
        if (_shaderSourceByPath.TryGetValue(shaderPath, out var cachedSource))
        {
            return cachedSource;
        }
        
        var loadedSource = File.ReadAllText(shaderPath);
        _shaderSourceByPath.Add(shaderPath, loadedSource);
        return loadedSource;
    }
}

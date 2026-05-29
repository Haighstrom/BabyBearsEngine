using System.IO;

namespace BabyBearsEngine.OpenGL;

internal sealed class FileShaderSourceProvider : IShaderSourceProvider
{
    private readonly Dictionary<string, string> _shaderSourceByPath = new(StringComparer.Ordinal);

    public string GetFragmentSource(FragmentShaderPath fragmentShaderPath) => GetSource(fragmentShaderPath.Path);

    public string GetVertexSource(VertexShaderPath vertexShaderPath) => GetSource(vertexShaderPath.Path);

    public string GetGeometrySource(GeometryShaderPath geometryShaderPath) => GetSource(geometryShaderPath.Path);

    private string GetSource(string shaderPath)
    {
        if (_shaderSourceByPath.TryGetValue(shaderPath, out string? cachedSource))
        {
            return cachedSource;
        }

        string loadedSource = File.ReadAllText(shaderPath);
        // Gate at load time so the path is naturally available for the error message — and so
        // a too-new shader is caught before any GL compile call is made.
        GpuCapabilities.EnforceShaderRequirement(shaderPath, loadedSource);
        _shaderSourceByPath.Add(shaderPath, loadedSource);
        return loadedSource;
    }
}

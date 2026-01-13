namespace BabyBearsEngine.OpenGL;

internal interface IShaderSourceProvider
{
    string GetFragmentSource(FragmentShaderPath fragmentShaderPath);
    string GetVertexSource(VertexShaderPath vertexShaderPath);
    string GetGeometrySource(GeometryShaderPath geometryShaderPath);
}

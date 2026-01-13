namespace BabyBearsEngine.OpenGL;

public static class GeometryShaders
{
    private const string BasePath = "Assets/Shaders/Geom/";

    public static GeometryShaderPath Default { get; } = new(BasePath + "default.geom");
    public static GeometryShaderPath SmoothLines { get; } = new(BasePath + "smooth_lines.geom");
}

namespace BabyBearsEngine.OpenGL;

public static class GeometryShaders
{
    private const string BasePath = "Assets/Shaders/Geom/";

    public static GeometryShaderPath BezierLines { get; } = new(BasePath + "bezier_lines.geom");
    public static GeometryShaderPath BillboardPointsToQuads { get; } = new(BasePath + "billboard_points_to_quads.geom");
    public static GeometryShaderPath Default { get; } = new(BasePath + "default.geom");
    public static GeometryShaderPath DefaultLines { get; } = new(BasePath + "default_lines.geom");
    public static GeometryShaderPath DefaultPoints { get; } = new(BasePath + "default_points.geom");
    public static GeometryShaderPath LineToQuad { get; } = new(BasePath + "line_to_quad.geom");
    public static GeometryShaderPath SmoothLines { get; } = new(BasePath + "smooth_lines.geom");
    public static GeometryShaderPath SmoothLinesTextured { get; } = new(BasePath + "smooth_lines_textured.geom");
    public static GeometryShaderPath SplineLines { get; } = new(BasePath + "spline_lines.geom");
}

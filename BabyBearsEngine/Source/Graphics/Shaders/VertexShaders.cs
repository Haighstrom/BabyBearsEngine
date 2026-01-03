namespace BabyBearsEngine.Source.Graphics.Shaders;

public static class VertexShaders
{
    private const string BasePath = "Assets/Shaders/Vert/";

    public static VertexShaderPath CameraMSAA { get; } = new(BasePath + "camera_msaa.vert");
    public static VertexShaderPath Default { get; } = new(BasePath + "default.vert");
    public static VertexShaderPath NoMatrixSolidColour { get; } = new(BasePath + "no_matrix_solid_colour.vert");
    public static VertexShaderPath Point { get; } = new(BasePath +  "point.vert");
    public static VertexShaderPath Shader { get; } = new(BasePath + "shader.vert");
    public static VertexShaderPath SolidColour { get; } = new(BasePath + "solid_colour.vert");
    public static VertexShaderPath Spritesheet { get; } = new(BasePath + "spritesheet.vert");
}

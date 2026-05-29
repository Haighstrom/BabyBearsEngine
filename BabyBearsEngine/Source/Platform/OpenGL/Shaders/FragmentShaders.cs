namespace BabyBearsEngine.OpenGL;

public static class FragmentShaders
{
    private const string BasePath = "Assets/Shaders/Frag/";

    public static FragmentShaderPath Blur { get; } = new(BasePath + "blur.frag");
    public static FragmentShaderPath CameraMSAA { get; } = new(BasePath + "camera_msaa.frag");
    public static FragmentShaderPath Darken { get; } = new(BasePath + "darken.frag");
    public static FragmentShaderPath Default { get; } = new(BasePath + "default.frag");
    public static FragmentShaderPath Greyscale { get; } = new(BasePath + "greyscale.frag");
    public static FragmentShaderPath Invisibility { get; } = new(BasePath + "invisibility.frag");
    public static FragmentShaderPath Lighting { get; } = new(BasePath + "lighting.frag");
    public static FragmentShaderPath Point { get; } = new(BasePath + "point.frag");
    public static FragmentShaderPath R8Texture { get; } = new(BasePath + "r8_texture.frag");
    public static FragmentShaderPath SdfText { get; } = new(BasePath + "sdf_text.frag");
    public static FragmentShaderPath Shader { get; } = new(BasePath + "shader.frag");
    public static FragmentShaderPath SolidColour { get; } = new(BasePath + "solid_colour.frag");
    public static FragmentShaderPath Stencil { get; } = new(BasePath + "stencil.frag");
    public static FragmentShaderPath Texture1D { get; } = new(BasePath + "texture_1d.frag");
}

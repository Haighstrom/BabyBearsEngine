namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-transformed textured-quad shader that maps each sampled colour to its luminance
/// (dot(rgb, 0.299, 0.587, 0.114)), leaving alpha unchanged. Assign to
/// <see cref="Worlds.Graphics.TextureGraphic.Shader"/> or <see cref="Worlds.Graphics.Sprite.Shader"/>
/// to greyscale a sprite.
/// </summary>
public sealed class GreyscaleShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<GreyscaleShaderProgram> s_instance = new(() => new GreyscaleShaderProgram());

    public static GreyscaleShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<GreyscaleShaderProgram>(() => new GreyscaleShaderProgram());
    }

    private GreyscaleShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.Greyscale)
    {
    }
}

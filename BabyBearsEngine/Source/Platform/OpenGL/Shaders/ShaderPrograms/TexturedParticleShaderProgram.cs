namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Shader program used by <see cref="Worlds.Particles.ParticleSystem"/> when a texture is supplied.
/// Combines <see cref="VertexShaders.Billboard"/> + <see cref="GeometryShaders.BillboardPointsToQuads"/>
/// + <see cref="FragmentShaders.Default"/> to expand each particle point into a camera-facing
/// textured quad whose pixel size is the per-particle <c>Size</c> attribute and whose fragments
/// sample the bound texture multiplied by the per-particle colour.
/// </summary>
public sealed class TexturedParticleShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<TexturedParticleShaderProgram> s_instance = new(() => new TexturedParticleShaderProgram());

    public static TexturedParticleShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<TexturedParticleShaderProgram>(() => new TexturedParticleShaderProgram());
    }

    private TexturedParticleShaderProgram()
        : base(VertexShaders.Billboard, GeometryShaders.BillboardPointsToQuads, FragmentShaders.Default)
    {
    }
}

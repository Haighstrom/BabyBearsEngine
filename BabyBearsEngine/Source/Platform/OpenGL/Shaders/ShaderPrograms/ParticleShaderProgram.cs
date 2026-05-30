namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Shader program used by <see cref="Worlds.Particles.ParticleSystem"/>. Combines
/// <see cref="VertexShaders.Billboard"/> + <see cref="GeometryShaders.BillboardPointsToQuads"/>
/// + <see cref="FragmentShaders.Point"/> to expand each particle point into a camera-facing
/// coloured quad whose pixel size is the per-particle <c>Size</c> attribute.
/// </summary>
public sealed class ParticleShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<ParticleShaderProgram> s_instance = new(() => new ParticleShaderProgram());

    public static ParticleShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<ParticleShaderProgram>(() => new ParticleShaderProgram());
    }

    private ParticleShaderProgram()
        : base(VertexShaders.Billboard, GeometryShaders.BillboardPointsToQuads, FragmentShaders.Point)
    {
    }
}

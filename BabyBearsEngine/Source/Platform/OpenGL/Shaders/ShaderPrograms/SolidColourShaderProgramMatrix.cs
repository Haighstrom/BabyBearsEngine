namespace BabyBearsEngine.OpenGL;

public sealed class SolidColourShaderProgramMatrix : MatrixShaderProgramBase
{
    private static Lazy<SolidColourShaderProgramMatrix> s_instance = new(() => new SolidColourShaderProgramMatrix());

    public static SolidColourShaderProgramMatrix Instance => s_instance.Value;

    /// <summary>
    /// Drop the cached instance so the next access reconstructs the GL shader program. Called
    /// between game runs (in <c>EngineTeardown.ResetForNextRun</c>) so the next run doesn't reuse
    /// a shader handle from a destroyed GL context. The previous instance's GL resources are
    /// effectively leaked — the context it belonged to is being torn down anyway.
    /// </summary>
    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<SolidColourShaderProgramMatrix>(() => new SolidColourShaderProgramMatrix());
    }

    private SolidColourShaderProgramMatrix()
        : base(VertexShaders.SolidColour, FragmentShaders.SolidColour)
    {
    }
}

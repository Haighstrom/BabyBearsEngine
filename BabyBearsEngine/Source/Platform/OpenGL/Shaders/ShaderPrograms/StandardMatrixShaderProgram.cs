namespace BabyBearsEngine.OpenGL;

public sealed class StandardMatrixShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<StandardMatrixShaderProgram> s_instance = new(() => new StandardMatrixShaderProgram());

    public static StandardMatrixShaderProgram Instance => s_instance.Value;

    /// <summary>
    /// Drop the cached instance so the next access reconstructs the GL shader program. Called
    /// between game runs (in <c>EngineTeardown.ResetForNextRun</c>) so the next run doesn't reuse
    /// a shader handle from a destroyed GL context. The previous instance's GL resources are
    /// effectively leaked — the context it belonged to is being torn down anyway.
    /// </summary>
    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<StandardMatrixShaderProgram>(() => new StandardMatrixShaderProgram());
    }

    private StandardMatrixShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.Default)
    {
    }
}

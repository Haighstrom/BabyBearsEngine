using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Combines <see cref="VertexShaders.NoMatrixTransform"/> + <see cref="GeometryShaders.LineToQuad"/>
/// + <see cref="FragmentShaders.SolidColour"/> to expand a two-vertex <c>GL_LINES</c> segment into
/// a thick coloured quad. Used by <see cref="Worlds.Graphics.LineGraphic"/>.
/// </summary>
public sealed class LineShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<LineShaderProgram> s_instance = new(() => new LineShaderProgram());

    public static LineShaderProgram Instance => s_instance.Value;

    /// <summary>
    /// Drop the cached instance so the next access reconstructs the GL shader program. Called
    /// between game runs (in <c>EngineTeardown.ResetForNextRun</c>) so the next run doesn't reuse
    /// a shader handle from a destroyed GL context. The previous instance's GL resources are
    /// effectively leaked — the context it belonged to is being torn down anyway.
    /// </summary>
    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<LineShaderProgram>(() => new LineShaderProgram());
    }

    private readonly int _lineThicknessLocation;
    private readonly int _thicknessInPixelsLocation;

    private LineShaderProgram()
        : base(VertexShaders.NoMatrixTransform, GeometryShaders.LineToQuad, FragmentShaders.SolidColour)
    {
        _lineThicknessLocation = GL.GetUniformLocation(Handle, "LineThickness");
        _thicknessInPixelsLocation = GL.GetUniformLocation(Handle, "ThicknessInPixels");
    }

    /// <summary>Full width of the line, in pixels (<see cref="ThicknessInPixels"/> true) or world units (false).</summary>
    public void SetLineThickness(float thickness)
    {
        Bind();
        GL.Uniform1(_lineThicknessLocation, thickness);
    }

    /// <summary>True: thickness is a constant screen-space pixel width. False: thickness scales with the model-view transform.</summary>
    public void SetThicknessInPixels(bool thicknessInPixels)
    {
        Bind();
        GL.Uniform1(_thicknessInPixelsLocation, thicknessInPixels ? 1 : 0);
    }
}

using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Combines <see cref="VertexShaders.NoMatrixTransform"/> + <see cref="GeometryShaders.SmoothLines"/>
/// + <see cref="FragmentShaders.SolidColour"/> to expand a <c>GL_LINE_STRIP_ADJACENCY</c> polyline
/// into mitered thick-line quads, joined neatly at each interior vertex. Used by
/// <see cref="Worlds.Graphics.LinePathGraphic"/>.
/// </summary>
public sealed class LinePathShaderProgram : MatrixShaderProgramBase
{
    private static Lazy<LinePathShaderProgram> s_instance = new(() => new LinePathShaderProgram());

    public static LinePathShaderProgram Instance => s_instance.Value;

    /// <summary>
    /// Drop the cached instance so the next access reconstructs the GL shader program. Called
    /// between game runs (in <c>EngineTeardown.ResetForNextRun</c>) so the next run doesn't reuse
    /// a shader handle from a destroyed GL context. The previous instance's GL resources are
    /// effectively leaked — the context it belonged to is being torn down anyway.
    /// </summary>
    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<LinePathShaderProgram>(() => new LinePathShaderProgram());
    }

    private readonly int _thicknessInPixelsLocation;
    private readonly int _thicknessLocation;

    private LinePathShaderProgram()
        : base(VertexShaders.NoMatrixTransform, GeometryShaders.SmoothLines, FragmentShaders.SolidColour)
    {
        _thicknessLocation = GL.GetUniformLocation(Handle, "Thickness");
        _thicknessInPixelsLocation = GL.GetUniformLocation(Handle, "ThicknessInPixels");

        // ShiftMode shifts the whole strip inward/outward around its vertices — only meaningful
        // for inset/outset border strips, which nothing currently uses this shader for, so it's
        // pinned to 0 (centred on the given points). MiterLimit is set explicitly rather than
        // relying on the GLSL source's own uniform initialiser.
        Bind();
        GL.Uniform1(GL.GetUniformLocation(Handle, "ShiftMode"), 0);
        GL.Uniform1(GL.GetUniformLocation(Handle, "MiterLimit"), 0.75f);
    }

    /// <summary>Full width of the line, in pixels (<see cref="ThicknessInPixels"/> true) or world units (false).</summary>
    public void SetThickness(float thickness)
    {
        Bind();
        GL.Uniform1(_thicknessLocation, thickness);
    }

    /// <summary>True: thickness is a constant screen-space pixel width. False: thickness scales with the model-view transform.</summary>
    public void SetThicknessInPixels(bool thicknessInPixels)
    {
        Bind();
        GL.Uniform1(_thicknessInPixelsLocation, thicknessInPixels ? 1 : 0);
    }
}

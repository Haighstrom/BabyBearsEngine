using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Combines <see cref="VertexShaders.NoMatrixTransform"/> + <see cref="GeometryShaders.SmoothLines"/>
/// + <see cref="FragmentShaders.DashedLine"/> to expand a <c>GL_LINE_STRIP_ADJACENCY</c> polyline
/// into mitered thick-line quads, optionally dashed, joined neatly at each interior vertex. Used
/// by <see cref="Worlds.Graphics.LinePathGraphic"/>.
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

    private readonly int _dashLengthLocation;
    private readonly int _gapLengthLocation;
    private readonly int _thicknessInPixelsLocation;
    private readonly int _thicknessLocation;

    private LinePathShaderProgram()
        : base(VertexShaders.NoMatrixTransform, GeometryShaders.SmoothLines, FragmentShaders.DashedLine)
    {
        _thicknessLocation = GL.GetUniformLocation(Handle, "Thickness");
        _thicknessInPixelsLocation = GL.GetUniformLocation(Handle, "ThicknessInPixels");
        _dashLengthLocation = GL.GetUniformLocation(Handle, "DashLength");
        _gapLengthLocation = GL.GetUniformLocation(Handle, "GapLength");

        // ShiftMode shifts the whole strip inward/outward around its vertices — only meaningful
        // for inset/outset border strips, which nothing currently uses this shader for, so it's
        // pinned to 0 (centred on the given points). MiterLimit, DashLength and GapLength are set
        // explicitly rather than relying on the GLSL source's own uniform initialisers — GapLength
        // 0 never discards (see dashed_line.frag), so a plain solid path is just the degenerate
        // case of this pattern.
        Bind();
        GL.Uniform1(GL.GetUniformLocation(Handle, "ShiftMode"), 0);
        GL.Uniform1(GL.GetUniformLocation(Handle, "MiterLimit"), 0.75f);
        GL.Uniform1(_dashLengthLocation, 1f);
        GL.Uniform1(_gapLengthLocation, 0f);
    }

    /// <summary>Dash length along the path, in the same units as <see cref="SetThickness"/>'s pixel/world-space choice. Irrelevant when <see cref="SetGapLength"/> is 0.</summary>
    public void SetDashLength(float dashLength)
    {
        Bind();
        GL.Uniform1(_dashLengthLocation, dashLength);
    }

    /// <summary>Gap length between dashes. 0 (the default) draws a plain solid path.</summary>
    public void SetGapLength(float gapLength)
    {
        Bind();
        GL.Uniform1(_gapLengthLocation, gapLength);
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

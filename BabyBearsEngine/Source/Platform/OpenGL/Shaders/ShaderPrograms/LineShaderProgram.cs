using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Combines <see cref="VertexShaders.NoMatrixTransform"/> + <see cref="GeometryShaders.LineToQuad"/>
/// + <see cref="FragmentShaders.DashedLine"/> to expand a two-vertex <c>GL_LINES</c> segment into
/// a thick coloured quad, optionally dashed. Used by <see cref="Worlds.Graphics.LineGraphic"/>.
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

    private readonly int _dashLengthLocation;
    private readonly int _dashOffsetLocation;
    private readonly int _gapLengthLocation;
    private readonly int _lineThicknessLocation;
    private readonly int _thicknessInPixelsLocation;

    private LineShaderProgram()
        : base(VertexShaders.NoMatrixTransform, GeometryShaders.LineToQuad, FragmentShaders.DashedLine)
    {
        _lineThicknessLocation = GL.GetUniformLocation(Handle, "LineThickness");
        _thicknessInPixelsLocation = GL.GetUniformLocation(Handle, "ThicknessInPixels");
        _dashLengthLocation = GL.GetUniformLocation(Handle, "DashLength");
        _gapLengthLocation = GL.GetUniformLocation(Handle, "GapLength");
        _dashOffsetLocation = GL.GetUniformLocation(Handle, "DashOffset");

        // GapLength 0 never discards (see dashed_line.frag), so a plain solid line is just the
        // degenerate case of this pattern — set explicitly rather than relying on the GLSL
        // source's own uniform initialiser.
        Bind();
        GL.Uniform1(_dashLengthLocation, 1f);
        GL.Uniform1(_gapLengthLocation, 0f);
        GL.Uniform1(_dashOffsetLocation, 0f);
    }

    /// <summary>Dash length along the line, in the same units as <see cref="SetLineThickness"/>'s pixel/world-space choice. Irrelevant when <see cref="SetGapLength"/> is 0.</summary>
    public void SetDashLength(float dashLength)
    {
        Bind();
        GL.Uniform1(_dashLengthLocation, dashLength);
    }

    /// <summary>Shifts the dash pattern along the line — animate this for a scrolling "marching ants" effect. Irrelevant when <see cref="SetGapLength"/> is 0.</summary>
    public void SetDashOffset(float dashOffset)
    {
        Bind();
        GL.Uniform1(_dashOffsetLocation, dashOffset);
    }

    /// <summary>Gap length between dashes. 0 (the default) draws a plain solid line.</summary>
    public void SetGapLength(float gapLength)
    {
        Bind();
        GL.Uniform1(_gapLengthLocation, gapLength);
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

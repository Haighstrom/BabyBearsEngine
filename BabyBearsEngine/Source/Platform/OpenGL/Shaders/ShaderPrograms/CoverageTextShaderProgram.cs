using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-aware shader for single-channel grayscale coverage text. Samples the R8 coverage atlas
/// produced by <see cref="Worlds.Graphics.Text.FreeTypeFontAtlasGenerator"/> and tints it by the
/// glyph colour (see <c>r8_texture.frag</c>). Unlike <see cref="SdfTextShaderProgram"/> the atlas
/// stores alpha coverage rather than distance, so no edge reconstruction happens in the shader —
/// the FreeType rasteriser already authored the antialiased edge at the target pixel size.
/// </summary>
public sealed class CoverageTextShaderProgram : ShaderProgramBase, IMatrixShaderProgram
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    public CoverageTextShaderProgram()
        : base(VertexShaders.Default, FragmentShaders.R8Texture)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        var mvMatrix = Matrix3.Identity;
        SetModelViewMatrix(ref mvMatrix);
    }

    public void SetModelViewMatrix(ref Matrix3 matrix)
    {
        Bind();

        unsafe
        {
            fixed (float* valuePointer = matrix.Values)
            {
                GL.UniformMatrix3(_mvMatrixLocation, 1, false, valuePointer);
            }
        }
    }

    public void SetProjectionMatrix(ref Matrix3 matrix)
    {
        Bind();

        unsafe
        {
            fixed (float* valuePointer = matrix.Values)
            {
                GL.UniformMatrix3(_pMatrixLocation, 1, false, valuePointer);
            }
        }
    }
}

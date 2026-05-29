using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Shader program used by <see cref="Worlds.Particles.ParticleSystem"/> when a texture is supplied.
/// Combines <see cref="VertexShaders.Billboard"/> + <see cref="GeometryShaders.BillboardPointsToQuads"/>
/// + <see cref="FragmentShaders.Default"/> to expand each particle point into a camera-facing
/// textured quad whose pixel size is the per-particle <c>Size</c> attribute and whose fragments
/// sample the bound texture multiplied by the per-particle colour.
/// </summary>
public sealed class TexturedParticleShaderProgram : ShaderProgramBase, IMatrixShaderProgram
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    public TexturedParticleShaderProgram()
        : base(VertexShaders.Billboard, GeometryShaders.BillboardPointsToQuads, FragmentShaders.Default)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        Matrix3 mvMatrix = Matrix3.Identity;
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

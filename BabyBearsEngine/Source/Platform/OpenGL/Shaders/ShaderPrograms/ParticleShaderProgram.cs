using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Shader program used by <see cref="Worlds.Particles.ParticleSystem"/>. Combines
/// <see cref="VertexShaders.Billboard"/> + <see cref="GeometryShaders.BillboardPointsToQuads"/>
/// + <see cref="FragmentShaders.Point"/> to expand each particle point into a camera-facing
/// coloured quad whose pixel size is the per-particle <c>Size</c> attribute.
/// </summary>
public sealed class ParticleShaderProgram : ShaderProgramBase, IMatrixShaderProgram
{
    private static Lazy<ParticleShaderProgram> s_instance = new(() => new ParticleShaderProgram());

    public static ParticleShaderProgram Instance => s_instance.Value;

    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<ParticleShaderProgram>(() => new ParticleShaderProgram());
    }

    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    private ParticleShaderProgram()
        : base(VertexShaders.Billboard, GeometryShaders.BillboardPointsToQuads, FragmentShaders.Point)
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

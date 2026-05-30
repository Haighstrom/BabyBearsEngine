using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public sealed class SolidColourShaderProgramMatrix : ShaderProgramBase, IMatrixShaderProgram
{
    private static Lazy<SolidColourShaderProgramMatrix> s_instance = new(() => new SolidColourShaderProgramMatrix());

    public static SolidColourShaderProgramMatrix Instance => s_instance.Value;

    /// <summary>
    /// Drop the cached instance so the next access reconstructs the GL shader program. Called
    /// between game runs (in <c>GameLauncher.Run</c>'s teardown) so the next run doesn't reuse a
    /// shader handle from a destroyed GL context. The previous instance's GL resources are
    /// effectively leaked — the context is being torn down anyway.
    /// </summary>
    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<SolidColourShaderProgramMatrix>(() => new SolidColourShaderProgramMatrix());
    }

    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    private SolidColourShaderProgramMatrix()
        :base(VertexShaders.SolidColour, FragmentShaders.SolidColour)
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

    public void SetProjectionMatrix(int width, int height)
    {
        var pMatrix = Matrix3.CreateOrtho(width, height);
        SetProjectionMatrix(ref pMatrix);
    }
}

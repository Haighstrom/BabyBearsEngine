using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Base class for shader programs that expose <c>MVMatrix</c> and <c>PMatrix</c> uniforms.
/// Looks up both locations at construction, sets the model-view matrix to identity, and provides
/// <see cref="SetModelViewMatrix"/> and <see cref="SetProjectionMatrix(ref Matrix3)"/> so subclasses
/// only need to add their own uniforms on top.
/// </summary>
public abstract class MatrixShaderProgramBase : ShaderProgramBase, IMatrixShaderProgram
{
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    protected MatrixShaderProgramBase(VertexShaderPath vertexShaderPath, FragmentShaderPath fragmentShaderPath)
        : base(vertexShaderPath, fragmentShaderPath)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        Matrix3 identity = Matrix3.Identity;
        SetModelViewMatrix(ref identity);
    }

    protected MatrixShaderProgramBase(VertexShaderPath vertexShaderPath, GeometryShaderPath geometryShaderPath, FragmentShaderPath fragmentShaderPath)
        : base(vertexShaderPath, geometryShaderPath, fragmentShaderPath)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        Matrix3 identity = Matrix3.Identity;
        SetModelViewMatrix(ref identity);
    }

    public void SetModelViewMatrix(ref Matrix3 matrix)
    {
        Bind();

        unsafe
        {
            Span<float> values = stackalloc float[9];
            matrix.WriteTo(values);
            fixed (float* valuePointer = values)
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
            Span<float> values = stackalloc float[9];
            matrix.WriteTo(values);
            fixed (float* valuePointer = values)
            {
                GL.UniformMatrix3(_pMatrixLocation, 1, false, valuePointer);
            }
        }
    }

    /// <summary>Convenience overload that builds an orthographic projection from window dimensions and uploads it.</summary>
    public void SetProjectionMatrix(int width, int height)
    {
        var pMatrix = Matrix3.CreateOrtho(width, height);
        SetProjectionMatrix(ref pMatrix);
    }
}

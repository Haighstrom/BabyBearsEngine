using BabyBearsEngine.Geometry;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-transformed textured-quad shader that applies a 9×9 box blur in texture space.
/// <see cref="BlurSize"/> controls the kernel spread; <c>0</c> reduces to a passthrough,
/// values around 2-10 give visible blur. Assign to
/// <see cref="Worlds.Graphics.TextureGraphic.Shader"/> or
/// <see cref="Worlds.Graphics.Sprite.Shader"/> to apply.
/// </summary>
public sealed class BlurShaderProgram : ShaderProgramBase, IMatrixShaderProgram
{
    private readonly int _blurSizeLocation;
    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;
    private float _blurSize = 2.0f;

    public BlurShaderProgram(float blurSize = 2.0f)
        : base(VertexShaders.Default, FragmentShaders.Blur)
    {
        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");
        _blurSizeLocation = GL.GetUniformLocation(Handle, "BlurSize");

        var mvMatrix = Matrix3.Identity;
        SetModelViewMatrix(ref mvMatrix);
        BlurSize = blurSize;
    }

    /// <summary>
    /// Kernel spread in texture-space units (internally scaled by <c>/400</c>, so values in
    /// the 1-10 range read naturally). <c>0</c> reduces to a passthrough.
    /// </summary>
    public float BlurSize
    {
        get => _blurSize;
        set
        {
            _blurSize = value;
            Bind();
            GL.Uniform1(_blurSizeLocation, value);
        }
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

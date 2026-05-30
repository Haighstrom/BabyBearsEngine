using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Matrix-transformed textured-quad shader that multiplies RGB by a scalar
/// <see cref="DarkenValue"/>; alpha is left untouched. <c>1.0</c> = original, <c>0.0</c> = black.
/// Assign to <see cref="Worlds.Graphics.TextureGraphic.Shader"/> or
/// <see cref="Worlds.Graphics.Sprite.Shader"/> to apply.
/// </summary>
public sealed class DarkenShaderProgram : MatrixShaderProgramBase
{
    private readonly int _darkenValueLocation;
    private float _darkenValue = 1.0f;

    public DarkenShaderProgram(float darkenValue = 1.0f)
        : base(VertexShaders.Default, FragmentShaders.Darken)
    {
        _darkenValueLocation = GL.GetUniformLocation(Handle, "DarkenValue");
        DarkenValue = darkenValue;
    }

    /// <summary>
    /// Scalar multiplied onto RGB before output. <c>1.0</c> leaves the colour unchanged;
    /// <c>0.0</c> produces pure black. Values outside [0, 1] are allowed but rarely useful.
    /// </summary>
    public float DarkenValue
    {
        get => _darkenValue;
        set
        {
            _darkenValue = value;
            Bind();
            GL.Uniform1(_darkenValueLocation, value);
        }
    }
}

using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;

/// <summary>
/// A shader program that supports model-view and projection matrix uniforms — the contract
/// implemented by every shader bound to a matrix-transformed graphic in the engine. Games can
/// implement this to wire custom shaders into <see cref="Worlds.Graphics.TextureGraphic.Shader"/>
/// or <see cref="Worlds.Graphics.Sprite.Shader"/> without engine-side changes.
/// </summary>
public interface IMatrixShaderProgram : IShaderProgram
{
    void SetProjectionMatrix(ref Matrix3 matrix);

    void SetModelViewMatrix(ref Matrix3 matrix);
}

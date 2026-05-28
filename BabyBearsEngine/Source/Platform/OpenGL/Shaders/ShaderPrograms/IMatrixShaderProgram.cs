using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;

/// <summary>
/// A shader program that supports model-view and projection matrix uniforms.
/// Extends <see cref="IShaderProgram"/> (Bind / Handle / Dispose) with matrix
/// setters, so consumers can hold a single reference rather than juggling two
/// interfaces or casting between them.
/// </summary>
internal interface IMatrixShaderProgram : IShaderProgram
{
    void SetProjectionMatrix(ref Matrix3 matrix);

    void SetModelViewMatrix(ref Matrix3 matrix);
}

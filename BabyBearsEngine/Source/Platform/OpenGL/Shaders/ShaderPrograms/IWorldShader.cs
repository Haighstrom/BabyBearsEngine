using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;

internal interface IWorldShader
{
    void SetProjectionMatrix(Matrix3 matrix);
    void SetModelViewMatrix(Matrix3 matrix);
}

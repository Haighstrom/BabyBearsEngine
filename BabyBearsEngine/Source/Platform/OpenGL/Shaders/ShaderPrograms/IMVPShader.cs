using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;

internal interface IMVPShader
{
    void SetProjectionMatrix(ref Matrix3 matrix);
    void SetModelViewMatrix(ref Matrix3 matrix);
}

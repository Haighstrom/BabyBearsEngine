using Matrix3 = BabyBearsEngine.Geometry.Matrix3;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Mathematics;

namespace BabyBearsEngine.OpenGL;

public sealed class SolidColourShaderProgram : ShaderProgramBase, IMVPShader
{
    private static readonly Lazy<SolidColourShaderProgram> s_instance = new(() => new SolidColourShaderProgram());

    internal static SolidColourShaderProgram Instance => s_instance.Value;

    private readonly int _windowSizeLocation;

    private SolidColourShaderProgram()
        :base(VertexShaders.NoMatrixSolidColour, FragmentShaders.SolidColour)
    {
        _windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");
    }

    public void SetWindowSize(int width, int height)
    {
        Bind();
        GL.Uniform2(_windowSizeLocation, new Vector2(width, height));
    }

    public void SetProjectionMatrix(ref Matrix3 matrix)
    {
        throw new NotImplementedException();
    }

    public void SetModelViewMatrix(ref Matrix3 matrix)
    {
        throw new NotImplementedException();
    }
}

using Matrix3 = BabyBearsEngine.Geometry.Matrix3;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public sealed class SolidColourShaderProgram : ShaderProgramBase, IMVPShader
{
    private static Lazy<SolidColourShaderProgram> s_instance = new(() => new SolidColourShaderProgram());

    internal static SolidColourShaderProgram Instance => s_instance.Value;

    /// <summary>
    /// Drop the cached instance so the next access reconstructs the GL shader program. Called
    /// between game runs (in <c>GameLauncher.Run</c>'s teardown) so the next run doesn't reuse a
    /// shader handle from a destroyed GL context. The previous instance's GL resources are
    /// effectively leaked — the context is being torn down anyway.
    /// </summary>
    internal static void ResetForNextRun()
    {
        s_instance = new Lazy<SolidColourShaderProgram>(() => new SolidColourShaderProgram());
    }

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

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.OpenGL;

public sealed class DefaultShaderProgram : ShaderProgramBase
{
    private readonly int _windowSizeLocation;

    public DefaultShaderProgram()
        :base(VertexShaders.Shader, FragmentShaders.Shader)
    {
        _windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");

        SetWindowSize(Window.Width, Window.Height);

        Window.Resize += OnWindowResize;
    }

    private void OnWindowResize(WindowResizeEventArgs args) => SetWindowSize(args.Width, args.Height);

    private void SetWindowSize(int width, int height)
    {
        Bind();
        // Explicit Vector2(float, float) avoids the target-typed-new() ambiguity that resolves
        // to GL.Uniform2(int, Vector2i) and triggers GL_INVALID_OPERATION against a vec2 (float)
        // uniform.
        GL.Uniform2(_windowSizeLocation, new Vector2(width, height));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Window.Resize -= OnWindowResize;
        }

        base.Dispose(disposing);
    }
}

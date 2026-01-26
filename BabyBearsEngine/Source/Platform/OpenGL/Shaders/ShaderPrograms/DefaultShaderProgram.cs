namespace BabyBearsEngine.OpenGL;

public class DefaultShaderProgram : ShaderProgramBase
{
    private readonly int _windowSizeLocation;

    public DefaultShaderProgram()
        :base(VertexShaders.Shader, FragmentShaders.Shader)
    {
        _windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");

        SetWindowSize(Window.Width, Window.Height);

        Window.Resize += args => SetWindowSize(args.Width, args.Height);
    }

    private void SetWindowSize(int width, int height)
    {
        Bind();
        GL.Uniform2(_windowSizeLocation, new(width, height));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Window.Resize -= args => SetWindowSize(args.Width, args.Height);
        }

        base.Dispose(disposing);
    }
}

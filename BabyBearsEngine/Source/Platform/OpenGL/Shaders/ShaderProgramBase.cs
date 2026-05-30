using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public abstract class ShaderProgramBase : IShaderProgram
{
    internal static IShaderSourceProvider ShaderSourceProvider { get; set; } = new FileShaderSourceProvider();

    private bool _disposed = false;

    public ShaderProgramBase(VertexShaderPath vertexShaderPath, FragmentShaderPath fragmentShaderPath)
    {
        string vsSource = ShaderSourceProvider.GetVertexSource(vertexShaderPath);
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = ShaderSourceProvider.GetFragmentSource(fragmentShaderPath);
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);
    }

    public ShaderProgramBase(VertexShaderPath vertexShaderPath, GeometryShaderPath geometryShaderPath, FragmentShaderPath fragmentShaderPath)
    {
        string vsSource = ShaderSourceProvider.GetVertexSource(vertexShaderPath);
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string gsSource = ShaderSourceProvider.GetGeometrySource(geometryShaderPath);
        int geometryShader = OpenGLHelper.CreateShader(gsSource, ShaderType.GeometryShader);

        string fsSource = ShaderSourceProvider.GetFragmentSource(fragmentShaderPath);
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, geometryShader, fragmentShader);
    }

    public int Handle { get; }

    public void Bind() => OpenGLHelper.BindShader(Handle);

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GPUResourceDeletion.TryRequestDeleteShader(Handle);
        _disposed = true;
    }
}

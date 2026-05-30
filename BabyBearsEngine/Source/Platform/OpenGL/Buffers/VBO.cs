using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public sealed class VBO() : IDisposable
{
    private bool _disposed = false;

    public int Handle { get; } = GL.GenBuffer();

    public void Bind() => OpenGLHelper.BindVBO(Handle);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GPUResourceDeletion.TryRequestDeleteVBO(Handle);
        _disposed = true;
    }
}

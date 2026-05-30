using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public sealed class VAO() : IDisposable
{
    private bool _disposed = false;

    public int Handle { get; } = GL.GenVertexArray();

    public void Bind() => OpenGLHelper.BindVAO(Handle);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GPUResourceDeletion.TryRequestDeleteVAO(Handle);
        _disposed = true;
    }
}

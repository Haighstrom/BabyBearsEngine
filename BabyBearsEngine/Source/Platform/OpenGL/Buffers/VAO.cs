using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public sealed class VAO : IDisposable
{
    private bool _disposed = false;

    public VAO()
    {
        GLThread.Ensure();
        Handle = GL.GenVertexArray();
    }

    public int Handle { get; }

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

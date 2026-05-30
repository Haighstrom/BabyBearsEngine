using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public sealed class EBO : IDisposable
{
    private readonly BufferUsageHint _bufferUsageHint;
    private bool _disposed = false;

    public EBO(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw)
    {
        GLThread.Ensure();
        _bufferUsageHint = bufferUsageHint;
        Handle = GL.GenBuffer();
    }

    public int Handle { get; }

    public void Bind() => OpenGLHelper.BindEBO(Handle);

    public void BufferData(uint[] indices)
    {
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, _bufferUsageHint);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GPUResourceDeletion.TryRequestDeleteEBO(Handle);
        _disposed = true;
    }
}

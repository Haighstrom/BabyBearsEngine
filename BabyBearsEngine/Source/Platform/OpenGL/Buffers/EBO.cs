using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

public sealed class EBO(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw) : IDisposable
{
    private bool _disposed = false;

    public int Handle { get; } = GL.GenBuffer();

    public void Bind() => OpenGLHelper.BindEBO(Handle);

    public void BufferData(uint[] indices)
    {
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, bufferUsageHint);
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

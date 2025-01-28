using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics;

internal class EBO(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw)
{
    private static int s_lastBoundHandle = 0;

    public int Handle { get; } = GL.GenBuffer();

    public void Use()
    {
        if (s_lastBoundHandle != Handle)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
            s_lastBoundHandle = Handle;
        }
    }

    public void Unbind()
    {
        if (s_lastBoundHandle != 0)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            s_lastBoundHandle = 0;
        }
    }

    public void BufferData(uint[] indices)
    {
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, bufferUsageHint);
    }
}

using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics;

internal class VBO(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw)
{
    private static int s_lastBoundHandle = 0;

    public int Handle { get; } = GL.GenBuffer();

    public void BufferData<TVertex>(TVertex[] vertices) where TVertex : struct, IVertex 
    {
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * TVertex.Stride, vertices, bufferUsageHint);
    }

    public void Use()
    {
        if (s_lastBoundHandle != Handle)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
            s_lastBoundHandle = Handle;
        }
    }

    public void Unbind()
    {
        if (s_lastBoundHandle != 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            s_lastBoundHandle = 0;
        }
    }
}

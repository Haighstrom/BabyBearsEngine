
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics;

public class VAO()
{
    private static int s_lastBoundHandle = 0;

    public static void DefineStandardVertexFormats()
    {
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 2 * sizeof(float));

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 6 * sizeof(float));
    }

    public int Handle { get; } = GL.GenVertexArray();

    public void Use()
    {
        if (s_lastBoundHandle != Handle)
        {
            GL.BindVertexArray(Handle);
            s_lastBoundHandle = Handle;
        }
    }

    public void Unbind()
    {
        if (s_lastBoundHandle != 0)
        {
            GL.BindVertexArray(0);
            s_lastBoundHandle = 0;
        }
    }
}

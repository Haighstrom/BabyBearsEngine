
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics;

public class VAO
{
    public VAO()
    {
        Handle = GL.GenVertexArray();
    }

    public int Handle { get; }

    public void Bind()
    {
        GL.BindVertexArray(Handle);
    }
}

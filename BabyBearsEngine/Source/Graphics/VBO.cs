using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics;

internal class VBO
{
    public VBO()
    {
        Handle = GL.GenBuffer();
    }

    public int Handle { get; }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
    }
}

using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics;

internal class EBO
{
    public EBO()
    {
        Handle = GL.GenBuffer();
    }

    public int Handle { get; }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
    }
}

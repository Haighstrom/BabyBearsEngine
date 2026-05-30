using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Platform.OpenGL.Buffers;

/// <summary>
/// A non-MSAA-enabled framebuffer with corresponding texture
/// </summary>
/// <param name="width"></param>
/// <param name="height"></param>
public sealed class FBO(int width, int height) : IDisposable
{
    private static Texture GetTexture(int width, int height)
    {
        int handle = GL.GenTexture();
        OpenGLHelper.BindTexture(handle);
        GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, width, height);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return new Texture(handle, width, height);
    }

    private bool _disposed = false;

    public int Handle { get; } = GL.GenFramebuffer();

    public ITexture Texture { get; } = GetTexture(width, height);

    public void Bind()
    {
        OpenGLHelper.BindFBO(Handle);

        //does this need doing every time?
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture.Handle, 0);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Texture.Dispose();
        GPUResourceDeletion.TryRequestDeleteFBO(Handle);
        _disposed = true;
    }
}

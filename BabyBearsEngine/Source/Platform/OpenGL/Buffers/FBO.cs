using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Platform.OpenGL.Buffers;

/// <summary>
/// A non-MSAA-enabled framebuffer with a colour-attached <see cref="Texture"/>.
/// </summary>
public sealed class FBO : IDisposable
{
    private bool _disposed = false;

    public int Handle { get; }

    public ITexture Texture { get; }

    public FBO(int width, int height)
    {
        GLThread.Ensure();
        Handle = GL.GenFramebuffer();
        Texture = CreateTexture(width, height);

        // Attach the colour texture once at construction time and verify completeness, so an
        // incomplete FBO surfaces immediately here rather than silently failing at first draw
        // with GL_INVALID_FRAMEBUFFER_OPERATION. Restore the framebuffer binding to 0
        // afterwards so callers that snapshot OpenGLHelper.LastBoundFBO before binding this
        // FBO see the backbuffer (not this newly-constructed FBO) as the "previous" state.
        OpenGLHelper.BindFBO(Handle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture.Handle, 0);
        AssertFramebufferComplete(Handle);
        OpenGLHelper.UnbindFBO();
    }

    public void Bind() => OpenGLHelper.BindFBO(Handle);

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

    private static Texture CreateTexture(int width, int height)
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

    private static void AssertFramebufferComplete(int handle)
    {
        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"FBO {handle} is incomplete after attachment: {status}");
        }
    }
}

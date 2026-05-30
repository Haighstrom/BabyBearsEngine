using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Platform.OpenGL.Buffers;

/// <summary>
/// An MSAA-enabled framebuffer with a multisample colour <see cref="Texture"/>.
/// </summary>
public sealed class MsaaFBO : IDisposable
{
    private bool _disposed = false;

    public int Handle { get; }

    public ITexture Texture { get; }

    public MsaaFBO(int width, int height, int samples)
    {
        Handle = GL.GenFramebuffer();
        Texture = CreateTexture(width, height, samples);

        // Attach the multisample texture once at construction time and verify completeness, so an
        // incomplete FBO surfaces immediately here rather than silently failing at first draw with
        // GL_INVALID_FRAMEBUFFER_OPERATION. Restore the framebuffer binding to 0 afterwards so
        // callers that snapshot OpenGLHelper.LastBoundFBO before binding this FBO see the
        // backbuffer (not this newly-constructed FBO) as the "previous" state.
        OpenGLHelper.BindFBO(Handle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, Texture.Handle, 0);
        AssertFramebufferComplete(Handle);
        OpenGLHelper.UnbindFBO();
    }

    public void Bind() => OpenGLHelper.BindFBO(Handle);

    public void BindTexture()
    {
        OpenGLHelper.BindTexture(Texture.Handle, TextureTarget.Texture2DMultisample);
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

    private static Texture CreateTexture(int width, int height, int samples)
    {
        int handle = GL.GenTexture();
        OpenGLHelper.BindTexture(handle, TextureTarget.Texture2DMultisample);
        GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgb8, width, height, false);
        return new Texture(handle, width, height);
    }

    private static void AssertFramebufferComplete(int handle)
    {
        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"MsaaFBO {handle} is incomplete after attachment: {status}");
        }
    }
}

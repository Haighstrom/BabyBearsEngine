using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Source.Platform.OpenGL.Buffers;

/// <summary>
/// A non-MSAA-enabled framebuffer with corresponding texture
/// </summary>
/// <param name="width"></param>
/// <param name="height"></param>
public class MsaaFBO(int width, int height, int samples) : IDisposable
{
    private static Texture GetTexture(int width, int height, int samples)
    {
        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DMultisample, handle);
        GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgb8, width, height, false);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return new Texture(handle, width, height);
    }

    private bool _disposed;

    public int Handle { get; } = GL.GenFramebuffer();

    public ITexture Texture { get; } = GetTexture(width, height, samples);

    public void Bind()
    {
        OpenGLHelper.BindFBO(Handle);

        //does this need doing every time?
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture.Handle, 0);
    }

    public void BindTexture()
    {
        GL.BindTexture(TextureTarget.Texture2DMultisample, Texture.Handle);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            GPUResourceDeletion.TryRequestDeleteFBO(Handle);

            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~MsaaFBO()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
        //todo: logging of the bad dispose
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

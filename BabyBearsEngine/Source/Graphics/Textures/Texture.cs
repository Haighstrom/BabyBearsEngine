namespace BabyBearsEngine.Source.Graphics.Textures;

internal class Texture(int handle, int width, int height) : ITexture, IDisposable
{
    private bool _disposed;

    public int Handle { get; } = handle;

    public int Width { get; } = width;

    public int Height { get; } = height;

    public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0) => OpenGLHelper.BindTexture(Handle, textureTarget, textureUnit);

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

            TextureDeleter.RequestDelete(this);

            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Texture()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

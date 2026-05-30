using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

internal sealed class Texture(int handle, int width, int height) : ITexture, IDisposable
{
    private bool _disposed = false;

    public int Handle { get; } = handle;

    public int Width { get; } = width;

    public int Height { get; } = height;

    public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0) => OpenGLHelper.BindTexture(Handle, textureTarget, textureUnit);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GPUResourceDeletion.TryRequestDeleteTexture(Handle);
        _disposed = true;
    }
}

using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

internal sealed class UpdatableTexture(Texture texture) : IUpdatableTexture
{
    private bool _disposed = false;

    public int Handle => texture.Handle;

    public int Width => texture.Width;

    public int Height => texture.Height;

    public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0) => texture.Bind(textureTarget, textureUnit);

    public void UpdatePixels(ReadOnlySpan<byte> rgbaData)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        int expectedLength = Width * Height * 4;
        if (rgbaData.Length != expectedLength)
        {
            throw new ArgumentException(
                $"rgbaData length ({rgbaData.Length}) does not match width*height*4 ({expectedLength}).",
                nameof(rgbaData));
        }

        GLThread.Ensure();
        OpenGLHelper.BindTexture(Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, PixelFormat.Rgba, PixelType.UnsignedByte, ref MemoryMarshal.GetReference(rgbaData));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        texture.Dispose();
        _disposed = true;
    }
}

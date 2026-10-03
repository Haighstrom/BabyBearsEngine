namespace BabyBearsEngine.OpenGL;

/// <summary>
/// A fixed-size RGBA8 texture whose pixels can be overwritten after creation — used for content
/// that changes every frame, such as decoded video. Must be updated on the GL thread.
/// </summary>
public interface IUpdatableTexture : ITexture
{
    /// <summary>
    /// Replaces the whole texture with <paramref name="rgbaData"/>: tightly-packed RGBA8, row-major,
    /// no padding, top row first. The data is uploaded as-is and must already be premultiplied
    /// (opaque content, alpha = 255, trivially is).
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="rgbaData"/>'s length is not exactly <c>Width * Height * 4</c>.</exception>
    /// <exception cref="ObjectDisposedException">If the texture has been disposed.</exception>
    void UpdatePixels(ReadOnlySpan<byte> rgbaData);
}

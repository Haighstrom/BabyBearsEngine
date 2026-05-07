namespace BabyBearsEngine;

public sealed class WindowIcon(int width, int height, byte[] pixels)
{
    public WindowIcon() : this(0, 0, []) { }

    public int Width { get; } = width;
    public int Height { get; } = height;
    public bool IsEmpty => Pixels.Length == 0;
    public byte[] Pixels { get; } = pixels;
}

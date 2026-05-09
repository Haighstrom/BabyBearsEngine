namespace BabyBearsEngine;

public sealed class WindowResizeEventArgs(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
}

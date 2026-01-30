using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine;

public interface IWindow
{
    event Action<ResizeEventArgs>? Resize;

    int Width { get; }
    int Height { get; }
}


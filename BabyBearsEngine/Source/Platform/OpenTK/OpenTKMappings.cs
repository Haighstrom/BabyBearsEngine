using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

internal static class OpenTkMappings
{
    public static GameWindowSettings ToOpenTK(this GameLoopSettings settings) => new()
    {
        UpdateFrequency = settings.TargetFramesPerSecond
    };

    public static NativeWindowSettings ToOpenTK(this WindowSettings settings) => new()
    {
        ClientSize = (settings.Width, settings.Height),
        Title = settings.Title,
        WindowBorder = settings.Border,
        WindowState = settings.State,
        // null lets GLFW choose position; CenterWindow() is called in OnLoad when Centre = true
        Location = settings.Centre ? null : new Vector2i(settings.X, settings.Y),
        Icon = settings.Icon,
        MinimumClientSize = settings.MinClientSize.IsEmpty ? null : new Vector2i(settings.MinClientSize.X, settings.MinClientSize.Y),
        MaximumClientSize = settings.MaxClientSize.IsEmpty ? null : new Vector2i(settings.MaxClientSize.X, settings.MaxClientSize.Y),
        APIVersion = new Version(settings.OpenGLVersion.major, settings.OpenGLVersion.minor),
        Vsync = settings.VSync ? VSyncMode.On : VSyncMode.Off,
    };

    public static CursorState ToCursorState(this WindowSettings settings)
    {
        if (settings.CursorLockedToWindow)
        {
            return CursorState.Grabbed;
        }
        if (!settings.CursorVisible)
        {
            return CursorState.Hidden;
        }
        return CursorState.Normal;
    }

    public static MouseCursor ToOpenTK(this CursorShape shape) => shape switch
    {
        CursorShape.Default      => MouseCursor.Default,
        CursorShape.Hidden       => MouseCursor.Empty,
        CursorShape.Crosshair    => MouseCursor.Crosshair,
        CursorShape.IBeam        => MouseCursor.IBeam,
        CursorShape.NotAllowed   => MouseCursor.NotAllowed,
        CursorShape.PointingHand => MouseCursor.PointingHand,
        CursorShape.ResizeAll    => MouseCursor.ResizeAll,
        CursorShape.ResizeEW     => MouseCursor.ResizeEW,
        CursorShape.ResizeNESW   => MouseCursor.ResizeNESW,
        CursorShape.ResizeNS     => MouseCursor.ResizeNS,
        CursorShape.ResizeNWSE   => MouseCursor.ResizeNWSE,
        _                        => MouseCursor.Default,
    };
}

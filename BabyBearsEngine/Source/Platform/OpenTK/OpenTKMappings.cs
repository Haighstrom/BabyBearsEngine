using BabyBearsEngine.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;

using OpenTKColor4 = OpenTK.Mathematics.Color4;
using OpenTKCursorState = OpenTK.Windowing.Common.CursorState;
using OpenTKImage = OpenTK.Windowing.Common.Input.Image;
using OpenTKKeys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using OpenTKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using OpenTKVSyncMode = OpenTK.Windowing.Common.VSyncMode;
using OpenTKWindowBorder = OpenTK.Windowing.Common.WindowBorder;
using OpenTKWindowIcon = OpenTK.Windowing.Common.Input.WindowIcon;
using OpenTKWindowState = OpenTK.Windowing.Common.WindowState;

namespace BabyBearsEngine.Platform.OpenTK;

internal static class OpenTkMappings
{
    public static GameWindowSettings ToOpenTK(this GameLoopSettings settings) => new()
    {
        UpdateFrequency = settings.TargetFramesPerSecond
    };

    public static NativeWindowSettings ToOpenTK(this WindowSettings settings)
    {
        GLFWProvider.CheckForMainThread = settings.CheckForMainThread;
        return new NativeWindowSettings
        {
            ClientSize = (settings.Width, settings.Height),
            Title = settings.Title,
            WindowBorder = settings.Border.ToOpenTK(),
            WindowState = settings.State.ToOpenTK(),
            // null lets GLFW choose position; CenterWindow() is called in OnLoad when Centre = true
            Location = settings.Centre ? null : new Vector2i(settings.X, settings.Y),
            Icon = settings.Icon.ToOpenTK(),
            MinimumClientSize = settings.MinClientSize.IsEmpty ? null : new Vector2i(settings.MinClientSize.X, settings.MinClientSize.Y),
            MaximumClientSize = settings.MaxClientSize.IsEmpty ? null : new Vector2i(settings.MaxClientSize.X, settings.MaxClientSize.Y),
            APIVersion = new Version(settings.OpenGLVersion.major, settings.OpenGLVersion.minor),
            Vsync = settings.VSync ? OpenTKVSyncMode.On : OpenTKVSyncMode.Off,
            StartVisible = false,
        };
    }

    public static OpenTKCursorState ToCursorState(this WindowSettings settings)
    {
        if (settings.CursorLockedToWindow)
        {
            return OpenTKCursorState.Grabbed;
        }
        if (!settings.CursorVisible)
        {
            return OpenTKCursorState.Hidden;
        }
        return OpenTKCursorState.Normal;
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

    public static Colour ToColour(this OpenTKColor4 colour) => new(
        (byte)Math.Round(colour.R * 255),
        (byte)Math.Round(colour.G * 255),
        (byte)Math.Round(colour.B * 255),
        (byte)Math.Round(colour.A * 255));

    public static OpenTKKeys ToOpenTK(this Keys key) => (OpenTKKeys)key;
    public static Keys ToKeys(this OpenTKKeys key) => (Keys)key;

    public static OpenTKMouseButton ToOpenTK(this MouseButton button) => (OpenTKMouseButton)button;
    public static MouseButton ToMouseButton(this OpenTKMouseButton button) => (MouseButton)button;

    public static OpenTKWindowBorder ToOpenTK(this WindowBorder border) => border switch
    {
        WindowBorder.Resizable => OpenTKWindowBorder.Resizable,
        WindowBorder.Fixed     => OpenTKWindowBorder.Fixed,
        WindowBorder.Hidden    => OpenTKWindowBorder.Hidden,
        _ => throw new ArgumentOutOfRangeException(nameof(border), border, "Unknown WindowBorder."),
    };

    public static WindowBorder ToWindowBorder(this OpenTKWindowBorder border) => border switch
    {
        OpenTKWindowBorder.Resizable => WindowBorder.Resizable,
        OpenTKWindowBorder.Fixed     => WindowBorder.Fixed,
        OpenTKWindowBorder.Hidden    => WindowBorder.Hidden,
        _ => throw new ArgumentOutOfRangeException(nameof(border), border, "Unknown OpenTK WindowBorder."),
    };

    public static OpenTKWindowState ToOpenTK(this WindowState state) => (OpenTKWindowState)state;
    public static WindowState ToWindowState(this OpenTKWindowState state) => (WindowState)state;

    public static OpenTKWindowIcon ToOpenTK(this WindowIcon icon)
    {
        if (icon.IsEmpty)
        {
            return new OpenTKWindowIcon();
        }
        return new OpenTKWindowIcon(new OpenTKImage(icon.Width, icon.Height, icon.Pixels));
    }

    public static WindowIcon ToWindowIcon(this OpenTKWindowIcon icon)
    {
        if (icon.Images.Length == 0)
        {
            return new WindowIcon();
        }
        var first = icon.Images[0];
        return new WindowIcon(first.Width, first.Height, first.Data);
    }
}

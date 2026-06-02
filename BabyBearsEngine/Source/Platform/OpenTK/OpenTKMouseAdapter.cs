using BabyBearsEngine.Input;

using OpenTKMouseState = OpenTK.Windowing.GraphicsLibraryFramework.MouseState;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKMouseAdapter(OpenTKGameEngine engine) : IMouse
{
    // The mouse-state instance is owned by the GameWindow and is reused each frame; we look it
    // up freshly each access rather than caching, matching how the engine itself reads it.
    private OpenTKMouseState _mouseState => engine.MouseState;

    public bool ButtonDown(MouseButton button) => _mouseState.IsButtonDown(button.ToOpenTK());
    public bool ButtonPressed(MouseButton button) => _mouseState.IsButtonPressed(button.ToOpenTK());
    public bool ButtonReleased(MouseButton button) => _mouseState.IsButtonReleased(button.ToOpenTK());

    public bool AnyButtonDown(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonDown);
    public bool AnyButtonDown(params MouseButton[] buttons) => buttons.Any(ButtonDown);

    public bool AnyButtonPressed(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonPressed);
    public bool AnyButtonPressed(params MouseButton[] buttons) => buttons.Any(ButtonPressed);

    public bool AnyButtonReleased(IEnumerable<MouseButton> buttons) => buttons.Any(ButtonReleased);
    public bool AnyButtonReleased(params MouseButton[] buttons) => buttons.Any(ButtonReleased);

    public bool AllButtonsDown(IEnumerable<MouseButton> buttons) => buttons.All(ButtonDown);
    public bool AllButtonsDown(params MouseButton[] buttons) => buttons.All(ButtonDown);

    public bool AllButtonsPressed(IEnumerable<MouseButton> buttons) => buttons.All(ButtonPressed);
    public bool AllButtonsPressed(params MouseButton[] buttons) => buttons.All(ButtonPressed);

    public bool AllButtonsReleased(IEnumerable<MouseButton> buttons) => buttons.All(ButtonReleased);
    public bool AllButtonsReleased(params MouseButton[] buttons) => buttons.All(ButtonReleased);

    public bool LeftDown => ButtonDown(MouseButton.Left);
    public bool MiddleDown => ButtonDown(MouseButton.Middle);
    public bool RightDown => ButtonDown(MouseButton.Right);

    public bool LeftUp => !LeftDown;
    public bool MiddleUp => !MiddleDown;
    public bool RightUp => !RightDown;

    public bool LeftPressed => ButtonPressed(MouseButton.Left);
    public bool MiddlePressed => ButtonPressed(MouseButton.Middle);
    public bool RightPressed => ButtonPressed(MouseButton.Right);

    public bool LeftReleased => ButtonReleased(MouseButton.Left);
    public bool MiddleReleased => ButtonReleased(MouseButton.Middle);
    public bool RightReleased => ButtonReleased(MouseButton.Right);

    // OpenTK / GLFW report mouse position and delta in logical client coordinates, but the
    // engine renders into the framebuffer and OnResize sets the GL viewport in framebuffer
    // pixels. On HiDPI displays the framebuffer is larger than the client area (e.g. 2× on a
    // typical retina display), so the unscaled mouse coords land below/right of where a UI
    // element rendered at the same logical position actually sits. Scale into framebuffer
    // space here so hit-testing matches what was drawn. On non-HiDPI displays
    // FramebufferSize == ClientSize and the scale is identity.
    private float ScaleX => engine.ClientSize.X > 0 ? (float)engine.FramebufferSize.X / engine.ClientSize.X : 1f;
    private float ScaleY => engine.ClientSize.Y > 0 ? (float)engine.FramebufferSize.Y / engine.ClientSize.Y : 1f;

    public int ClientX => (int)(_mouseState.X * ScaleX);
    public int ClientY => (int)(_mouseState.Y * ScaleY);

    public float WheelDelta => _mouseState.ScrollDelta.Y;
    public int XDelta => (int)(_mouseState.Delta.X * ScaleX);
    public int YDelta => (int)(_mouseState.Delta.Y * ScaleY);
}

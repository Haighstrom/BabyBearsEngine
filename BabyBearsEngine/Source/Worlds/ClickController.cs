using BabyBearsEngine.Input;

namespace BabyBearsEngine.Source.Worlds;

internal sealed class ClickController(IClickable clickTarget, double hoverDelay = 0.5)
{
    private readonly IClickable _clickTarget = clickTarget;
    private readonly double _hoverDelay = hoverDelay;

    private bool _mouseWasOver = false; // tracks if the mouse was over in the previous frame
    private bool _mouseIsOver = false;

    private double _hoverTime;
    private bool _isHovering;
    private bool _isPressed;

    public void SetMouseOver(bool isMouseOver) //todo: check this fn is called before Update and not after, otherwise we might have a frame of delay on the hover/enter/exit events
    {
        _mouseIsOver = isMouseOver;
    }

    public void Update(double elapsed)
    {
        // --- ENTER / EXIT ---
        if (_mouseIsOver && !_mouseWasOver)
        {
            _clickTarget.TriggerMouseEntered();
        }
        else if (!_mouseIsOver && _mouseWasOver)
        {
            _clickTarget.TriggerMouseExited();
        }

        // --- HOVER ---
        if (_mouseIsOver && !_isHovering)
        {
            _hoverTime += elapsed;

            if (!_isHovering && _hoverTime >= _hoverDelay)
            {
                _isHovering = true;
                _clickTarget.TriggerHover();
            }
        }
        else if (!_mouseIsOver)
        {
            if (_isHovering)
            {
                _clickTarget.TriggerStopHover();
                _isHovering = false;
            }

            _hoverTime = 0;
        }

        // --- PRESS & RELEASE ---
        if (_mouseIsOver && Mouse.LeftPressed)
        {
            _clickTarget.TriggerLeftPressed();
            _isPressed = true;
        }
        else if (_isPressed && _mouseIsOver && Mouse.LeftReleased)
        {
            _clickTarget.TriggerLeftReleased();
        }

        _mouseWasOver = _mouseIsOver;
    }
}

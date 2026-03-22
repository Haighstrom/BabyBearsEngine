namespace BabyBearsEngine.Source.Worlds;

internal sealed class ButtonInteractionHandler(IButtonTrigger trigger, double hoverDelay = 0.5)
{
    private readonly IButtonTrigger _trigger = trigger;
    private readonly double _hoverDelay = hoverDelay;

    private bool _wasMouseInside;
    private bool _wasLeftPressed;
    private bool _isPressedInside;
    private double _hoverTime;
    private bool _hasTriggeredHover;

    public void Update(double elapsed, bool isInside, bool isPressed)
    {
        // --- ENTER / EXIT ---
        if (isInside && !_wasMouseInside) _trigger.TriggerMouseEntered();
        if (!isInside && _wasMouseInside) _trigger.TriggerMouseExited();

        // --- HOVER ---
        if (isInside)
        {
            _hoverTime += elapsed;
            if (!_hasTriggeredHover && _hoverTime >= _hoverDelay)
            {
                _hasTriggeredHover = true;
                _trigger.TriggerMouseHovered();
            }
        }
        else
        {
            _hoverTime = 0;
            _hasTriggeredHover = false;
        }

        // --- PRESS ---
        if (isPressed && !_wasLeftPressed && isInside)
        {
            _isPressedInside = true;
            _trigger.TriggerLeftPressed();
        }

        // --- RELEASE ---
        if (!isPressed && _wasLeftPressed)
        {
            if (_isPressedInside && isInside) _trigger.TriggerLeftClicked();
            _trigger.TriggerLeftReleased();
            _isPressedInside = false;
        }

        _wasMouseInside = isInside;
        _wasLeftPressed = isPressed;
    }
}

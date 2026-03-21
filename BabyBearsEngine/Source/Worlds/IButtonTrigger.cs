namespace BabyBearsEngine.Source.Worlds;

internal interface IButtonTrigger
{
    void TriggerLeftClicked();
    void TriggerLeftPressed();
    void TriggerLeftReleased();
    void TriggerMouseEntered();
    void TriggerMouseExited();
    void TriggerMouseHovered();
}

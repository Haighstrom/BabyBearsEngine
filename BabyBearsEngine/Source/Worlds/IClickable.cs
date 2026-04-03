namespace BabyBearsEngine.Source.Worlds;

internal interface IClickable
{
    void TriggerLeftClicked();
    void TriggerLeftPressed();
    void TriggerLeftReleased();
    void TriggerMouseEntered();
    void TriggerMouseExited();
    void TriggerHover();
    void TriggerStopHover();
}

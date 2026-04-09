namespace BabyBearsEngine.Source.Worlds;

internal interface IClickable
{
    void TriggerLeftPressed();
    void TriggerLeftReleased();
    void TriggerMouseEntered();
    void TriggerMouseExited();
    void TriggerHover();
    void TriggerCancelHover();
}

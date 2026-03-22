namespace BabyBearsEngine.Tests.System.Source.BearSpinner3000;

internal class BearSpinnerButton(int x, int y) 
    : Button(x, y, 120, 60, Colour.White, "Bear Spinner 3000")
{
    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();

        Engine.ChangeWorld(new BearSpinnerWorld());
    }
}

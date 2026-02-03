namespace BabyBearsEngine.Tests.System.Source.BearSpinner3000;

internal class BearSpinnerButton(int x, int y) 
    : Button(x, y, 120, 60, Color4.White, "Bear Spinner 3000")
{
    public override void OnClicked()
    {
        base.OnClicked();

        Engine.ChangeWorld(new BearSpinnerWorld());
    }
}

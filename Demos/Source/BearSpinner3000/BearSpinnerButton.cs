namespace BabyBearsEngine.Demos.Source.BearSpinner3000;

internal class BearSpinnerButton(int x, int y) 
    : Button(x, y, 120, 60, Colour.White, "Bear Spinner 3000")
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        Engine.ChangeWorld(new BearSpinnerWorld());

        Window.Close();
    }
}

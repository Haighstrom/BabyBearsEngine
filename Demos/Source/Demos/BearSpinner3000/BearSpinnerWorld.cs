using System;

namespace BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;

internal class BearSpinnerWorld : DemoWorld
{
    public override string Name => "Bear Spinner 3000";

    public BearSpinnerWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Window.Resize += Window_Resize;
        AddBears();
    }

    private void Window_Resize(WindowResizeEventArgs obj)
    {
        RemoveAll();
        AddBears();
        AddCommonControls();
    }

    private void AddBears()
    {
        Repeat.CallMethod(
            () => Add(new BearEntity(Randomisation.Int(Window.Width), Randomisation.Int(Window.Height))),
            Window.Width * Window.Height / 3000);
    }
}

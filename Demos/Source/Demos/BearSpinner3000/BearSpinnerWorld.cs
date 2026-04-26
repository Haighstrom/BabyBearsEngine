using System;
using BabyBearsEngine.Demos.Source;

namespace BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;

internal class BearSpinnerWorld : DemoWorld
{
    public BearSpinnerWorld()
    {
        Window.Resize += Window_Resize;
        AddBears();
    }

    private void Window_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
    {
        RemoveAll();
        AddBears();
        AddCommonControls();
    }

    private void AddBears()
    {
        Random random = new();

        Repeat.CallMethod(() => Add(new BearEntity(random.Next(Window.Width), random.Next(Window.Height))), Window.Width * Window.Height / 3000);
    }
}

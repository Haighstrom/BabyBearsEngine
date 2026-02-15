using System;

namespace BabyBearsEngine.Tests.System.Source.BearSpinner3000;

internal class BearSpinnerWorld : World
{
    public BearSpinnerWorld()
    {
        Window.Resize += Window_Resize;
        AddBears();
    }

    private void Window_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
    {
        Clear();
        AddBears();
    }

    private void AddBears()
    {

        Random random = new();

        Repeat.CallMethod(() => Add(new BearEntity(random.Next(Window.Width), random.Next(Window.Height))), Window.Width * Window.Height / 3000);
    }
}

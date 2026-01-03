using System;
using BabyBearsEngine.Source.Core;
using BabyBearsEngine.Source.Tools;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Tests.System.Source.BearSpinner3000;

internal class BearSpinnerWorld : World
{
    public BearSpinnerWorld()
    {
        //Repeat.CallMethod(() => _camera.Add(new Bear(Randomisation.Rand((int)window.ClientSize.X), Randomisation.Rand((int)window.ClientSize.Y))), window.WindowWidth * window.WindowHeight / 3000);
        
        Random random = new();

        Repeat.CallMethod(() => AddEntity(new BearEntity(random.Next(Window.Width), random.Next(Window.Height))), Window.Width * Window.Height / 3000);
    }
}

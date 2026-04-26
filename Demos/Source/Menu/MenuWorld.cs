using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;

namespace BabyBearsEngine.Demos.Source.Menu;

internal class MenuWorld : World
{
    public MenuWorld()
    {
        Add(new BearSpinnerButton(20, 20));
        Add(new TextDemoButton(20, 85));
    }
}

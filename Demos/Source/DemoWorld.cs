using BabyBearsEngine.Demos.Source.Menu;

namespace BabyBearsEngine.Demos.Source;

internal abstract class DemoWorld : World
{
    protected DemoWorld()
    {
        AddCommonControls();
    }

    protected void AddCommonControls()
    {
        Add(new ReturnToMainMenuButton());
    }
}

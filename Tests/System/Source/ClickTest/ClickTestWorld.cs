namespace BabyBearsEngine.Tests.System.Source.ClickTest;

internal class ClickTestWorld : World
{
    public ClickTestWorld()
    {
        Add(new ClickerBox(50, 50, 100, 100));
    }
}

namespace BabyBearsEngine.Tests.System.Source.ClickTest;

internal class ClickTestWorld : World
{
    public ClickTestWorld()
    {
        Add(new ClickerBox(75, 75, 150, 150, Colour.Orange));
        Add(new ClickerBox(50, 50, 100, 100, Colour.Green));
        Add(new ClickerBox(100, 100, 100, 100, Colour.Fuchsia));
    }
}

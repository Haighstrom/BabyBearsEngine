namespace BabyBearsEngine.Tests.System.Source.ClickTest;

internal class ClickTestWorld : World
{
    public ClickTestWorld()
    {
        Add(new Button(75, 75, 150, 150, Colour.Orange));
        Add(new Button(50, 50, 100, 100, Colour.Green));
        Add(new Button(100, 100, 100, 100, Colour.Fuchsia));
    }
}

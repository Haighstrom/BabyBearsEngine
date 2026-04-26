using BabyBearsEngine.Demos.Source.ShadingLanguageCookbook.Menu;

namespace BabyBearsEngine.Demos.Source.ShadingLanguageCookbook.Chapter1;

internal class Chapter1World : World
{
    public Chapter1World()
    {
        Add(new Triangle());
        Add(new ReturnToMainMenuButton());
    }
}

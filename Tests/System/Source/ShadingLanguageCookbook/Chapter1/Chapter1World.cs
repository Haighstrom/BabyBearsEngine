using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;

internal class Chapter1World : World
{
    public Chapter1World()
    {
        AddGraphic(new Triangle());
    }
}

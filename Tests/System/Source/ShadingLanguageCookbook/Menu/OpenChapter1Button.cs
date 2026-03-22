using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class OpenChapter1Button() 
    : Button(10, 10, 80, 40, Colour.Blue, "Chapter 1")
{
    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();

        Engine.ChangeWorld(new Chapter1World());
    }
}

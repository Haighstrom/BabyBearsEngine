using BabyBearsEngine.Demos.Source.ShadingLanguageCookbook.Chapter1;

namespace BabyBearsEngine.Demos.Source.ShadingLanguageCookbook.Menu;

internal class OpenChapter1Button() 
    : Button(10, 10, 80, 40, Colour.Blue, "Chapter 1")
{
    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        Engine.ChangeWorld(new Chapter1World());
    }
}

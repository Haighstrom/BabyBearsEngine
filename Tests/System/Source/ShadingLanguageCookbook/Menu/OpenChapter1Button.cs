using BabyBearsEngine.Source.Worlds.UI;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class OpenChapter1Button() 
    : Button(10, 10, 80, 40, Color4.Blue, "Chapter 1")
{
    public override void OnClicked()
    {
        Window.ChangeWorld(new Chapter1World());
    }
}

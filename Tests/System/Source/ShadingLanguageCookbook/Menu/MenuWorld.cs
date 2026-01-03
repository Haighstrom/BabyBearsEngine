using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Graphics.Text;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class MenuWorld : World
{
    public MenuWorld()
    {
        //AddEntity(new OpenChapter1Button());

        //AddGraphic(new Image("Assets/bear.png", 200, 200, 100, 100));

        FontDefinition fontDef = new("Times New Roman", 20, FontStyle.Regular, false);
        AddGraphic(new TextImage(fontDef, "Haigh!", Color4.White, 200, 200, 512, 128));
        //AddGraphic(new TextImage(fontDef, "Hi pretty girl", Color4.White, 150, 60, 512, 512));
    }
}

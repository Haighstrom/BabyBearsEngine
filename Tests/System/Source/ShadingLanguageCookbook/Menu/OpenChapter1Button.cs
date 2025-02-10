using System;
using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.UI;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class OpenChapter1Button(HaighWindow haighWindow, ShaderProgramLibrary shaderLibrary) 
    : Button(shaderLibrary, 10, 10, 80, 40, Color4.Blue)
{
    public override void OnClicked()
    {
        haighWindow.World = new Chapter1World(haighWindow, shaderLibrary);
    }
}

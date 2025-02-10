using System;
using BabyBearsEngine.Source;
using BabyBearsEngine.Source.Graphics.Shaders;
using BabyBearsEngine.Source.UI;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

internal class ReturnToMainMenuButton(HaighWindow haighWindow, ShaderProgramLibrary shaderLibrary)
    : Button(shaderLibrary, 10, 10, 80, 40, Color4.Yellow)
{
    public override void OnClicked()
    {
        haighWindow.World = new MenuWorld(haighWindow, shaderLibrary);
    }
}

using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class CyclingButtonDemoWorld : DemoWorld
{
    private const int LabelLeft = 200;
    private const int WidgetLeft = 420;
    private const int RowY = 260;

    public override string Name => "Cycling Button";

    public CyclingButtonDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(new TextGraphic(new FontDefinition("Times New Roman", 18), "Value:", Colour.Black,
            LabelLeft, RowY, 200, 50)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        CyclingValueButton<string> button = new(
            WidgetLeft, RowY, 180, 50,
            ButtonTheme.FromColour(new Colour(160, 200, 255)),
            ["Easy", "Normal", "Hard", "Nightmare"]);
        Add(button);

        TextGraphic result = new(new FontDefinition("Times New Roman", 16), $"Selected: {button.CurrentValue}",
            Colour.Black, 0f, RowY + 70, 800f, 30f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(result);

        button.ValueChanged += (_, e) => result.Text = $"Selected: {e.NewValue}";
    }
}

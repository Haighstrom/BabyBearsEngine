using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class CheckboxDemoWorld : DemoWorld
{
    private const int LabelLeft = 200;
    private const int WidgetLeft = 420;
    private const int RowHeight = 80;
    private const int FirstRowY = 160;

    public override string Name => "Checkbox";

    public CheckboxDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeLabel(LabelLeft, FirstRowY, "Unchecked:"));
        Add(new Checkbox(WidgetLeft, FirstRowY, 50, 50, CheckboxTheme.Default));

        Add(MakeLabel(LabelLeft, FirstRowY + RowHeight, "Checked:"));
        Add(new Checkbox(WidgetLeft, FirstRowY + RowHeight, 50, 50, CheckboxTheme.Default, isChecked: true));

        Add(MakeLabel(LabelLeft, FirstRowY + 2 * RowHeight, "Interactive:"));
        Add(new Checkbox(WidgetLeft, FirstRowY + 2 * RowHeight, 50, 50, CheckboxTheme.Default));
    }

    private static TextGraphic MakeLabel(int x, int y, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 18), text, Colour.Black, x, y, 200, 50)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }
}

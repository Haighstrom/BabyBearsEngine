using System;
using System.Collections.Generic;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class DropdownDemoWorld : DemoWorld
{
    private static readonly FontDefinition s_font = new("Times New Roman", 16);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 18);

    public DropdownDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(230, 230, 230);

        Add(new TextGraphic(s_titleFont, "Dropdown List Demo", Colour.DimGray, 0f, 50f, 800f, 26f)
        {
            HAlignment = HAlignment.Centred,
        });

        Add(new TextGraphic(s_font, "Click the button to open / close the list. Click an option to select it.",
            new Colour(100, 100, 100), 0f, 84f, 800f, 20f)
        {
            HAlignment = HAlignment.Centred,
        });

        // Difficulty dropdown
        Add(new TextGraphic(s_font, "Difficulty:", Colour.Black, 160f, 120f, 130f, 36f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        IReadOnlyList<string> difficulties = ["Easy", "Normal", "Hard", "Nightmare"];

        DropdownList<string> diffDropdown = new(
            300f, 120f, 200f, 36f,
            difficulties,
            DropdownListTheme.FromColours(new Colour(70, 110, 190), new Colour(55, 90, 165)),
            initialIndex: 1);

        Overlay.Add(diffDropdown);

        TextGraphic diffResult = new(s_font, $"Selected: {diffDropdown.CurrentValue}", Colour.Black,
            0f, 175f, 800f, 26f)
        {
            HAlignment = HAlignment.Centred,
        };
        Add(diffResult);

        diffDropdown.SelectionChanged += (_, e) =>
            diffResult.Text = $"Selected: {e.NewValue}";

        // Player class dropdown
        Add(new TextGraphic(s_font, "Class:", Colour.Black, 160f, 320f, 130f, 36f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        IReadOnlyList<string> classes = ["Warrior", "Mage", "Rogue"];

        DropdownList<string> classDropdown = new(
            300f, 320f, 200f, 36f,
            classes,
            DropdownListTheme.FromColours(new Colour(140, 80, 60), new Colour(120, 65, 50)),
            initialIndex: 0);

        Overlay.Add(classDropdown);

        TextGraphic classResult = new(s_font, $"Selected: {classDropdown.CurrentValue}", Colour.Black,
            0f, 376f, 800f, 26f)
        {
            HAlignment = HAlignment.Centred,
        };
        Add(classResult);

        classDropdown.SelectionChanged += (_, e) =>
            classResult.Text = $"Selected: {e.NewValue}";

        // Season dropdown — positioned near bottom so the list opens upward
        Add(new TextGraphic(s_font, "Season:", Colour.Black, 160f, 490f, 130f, 36f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        IReadOnlyList<string> seasons = ["Spring", "Summer", "Autumn", "Winter"];

        DropdownList<string> seasonDropdown = new(
            300f, 490f, 200f, 36f,
            seasons,
            DropdownListTheme.FromColours(new Colour(60, 140, 80), new Colour(50, 120, 65)));

        Overlay.Add(seasonDropdown);

        TextGraphic seasonResult = new(s_font, $"Selected: {seasonDropdown.CurrentValue}", Colour.Black,
            0f, 546f, 800f, 26f)
        {
            HAlignment = HAlignment.Centred,
        };
        Add(seasonResult);

        seasonDropdown.SelectionChanged += (_, e) =>
            seasonResult.Text = $"Selected: {e.NewValue}";
    }

    public override string Name => "Dropdown Demo";
}

using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

/// <summary>
/// Demonstrates TextInputBox (free text) and NumberInputBox (digits / decimals / negatives).
/// Click a field to focus it. Press Escape or click another field to blur.
/// Press Enter to confirm and update the display panel.
/// </summary>
internal class InputBoxDemoWorld : DemoWorld
{
    private static readonly FontDefinition LabelFont = new("Times New Roman", 16);
    private static readonly FontDefinition HeadingFont = new("Times New Roman", 20);
    private static readonly FontDefinition SmallFont = new("Times New Roman", 13);

    private const int LabelX = 60;
    private const int BoxX = 220;
    private const int BoxW = 300;
    private const int BoxH = 34;
    private const int Row1Y = 130;
    private const int Row2Y = 200;
    private const int Row3Y = 270;
    private const int Row4Y = 340;
    private const int ResultY = 430;

    private readonly List<TextInputBox> _allBoxes = [];
    private readonly TextGraphic _resultLabel;

    public override string Name => "Input Boxes";

    public InputBoxDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(new TextGraphic(HeadingFont, "Input Box demo", Colour.Black, 0, 30, 800, 30)
        {
            HAlignment = HAlignment.Centred,
        });

        Add(MakeInstruction(60, 75, 680, 20,
            "Click a box to focus it. Type to edit. Escape or click another box to blur. Enter to confirm."));

        InputBoxTheme theme = InputBoxTheme.FromColour(new Colour(248, 248, 255));

        // --- Text input
        Add(MakeLabel(LabelX, Row1Y, "Name:"));
        TextInputBox nameBox = new(BoxX, Row1Y, BoxW, BoxH, theme);
        RegisterBox(nameBox);

        // --- Integers only
        Add(MakeLabel(LabelX, Row2Y, "Age (int):"));
        NumberInputBox ageBox = new(BoxX, Row2Y, BoxW, BoxH, theme,
                                    allowDecimals: false, allowNegative: false);
        RegisterBox(ageBox);

        // --- Decimals, positive only
        Add(MakeLabel(LabelX, Row3Y, "Height (m):"));
        NumberInputBox heightBox = new(BoxX, Row3Y, BoxW, BoxH, theme,
                                       allowDecimals: true, allowNegative: false);
        RegisterBox(heightBox);

        // --- Decimals + negatives
        Add(MakeLabel(LabelX, Row4Y, "Score:"));
        NumberInputBox scoreBox = new(BoxX, Row4Y, BoxW, BoxH, theme,
                                      allowDecimals: true, allowNegative: true);
        RegisterBox(scoreBox);

        // Wire up Enter → update result panel for each box
        nameBox.Submitted += (_, _) => UpdateResult(nameBox, ageBox, heightBox, scoreBox);
        ageBox.Submitted  += (_, _) => UpdateResult(nameBox, ageBox, heightBox, scoreBox);
        heightBox.Submitted += (_, _) => UpdateResult(nameBox, ageBox, heightBox, scoreBox);
        scoreBox.Submitted  += (_, _) => UpdateResult(nameBox, ageBox, heightBox, scoreBox);

        // Result display
        Add(new ColourGraphic(new Colour(240, 245, 240), BoxX, ResultY, BoxW, 80));
        _resultLabel = new TextGraphic(SmallFont, "(press Enter in any box to confirm)",
                                       new Colour(100, 100, 100), BoxX + 8, ResultY + 4, BoxW - 16, 72)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
            Multiline = true,
        };
        Add(_resultLabel);
    }

    private void RegisterBox(TextInputBox box)
    {
        // Clicking this box blurs all others
        box.FocusGained += (_, _) =>
        {
            foreach (TextInputBox other in _allBoxes)
            {
                if (!ReferenceEquals(other, box))
                {
                    other.Blur();
                }
            }
        };

        _allBoxes.Add(box);
        Add(box);
    }

    private void UpdateResult(TextInputBox name, NumberInputBox age,
                               NumberInputBox height, NumberInputBox score)
    {
        string ageStr    = age.Value.HasValue    ? age.Value.Value.ToString("0")    : "?";
        string heightStr = height.Value.HasValue ? height.Value.Value.ToString("0.00") : "?";
        string scoreStr  = score.Value.HasValue  ? score.Value.Value.ToString("0.00")  : "?";

        _resultLabel.Text = $"Name: {name.Text}  Age: {ageStr}  Height: {heightStr} m  Score: {scoreStr}";
        _resultLabel.Colour = Colour.Black;
    }

    private static Entity MakeLabelledBorder(int x, int y, int w, int h)
    {
        Entity frame = new(x - 1, y - 1, w + 2, h + 2);
        frame.Add(new ColourGraphic(new Colour(180, 180, 200), 0, 0, w + 2, h + 2));
        return frame;
    }

    private TextGraphic MakeLabel(int x, int y, string text)
    {
        return new TextGraphic(LabelFont, text, Colour.Black, x, y, BoxX - x - 10, BoxH)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        };
    }

    private static TextGraphic MakeInstruction(int x, int y, int w, int h, string text)
    {
        return new TextGraphic(SmallFont, text, new Colour(80, 80, 80), x, y, w, h)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }
}

using System;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.TextDemo;

internal class TypewriterDemoWorld : DemoWorld
{
    private const int BoxH = 230;
    private const int BoxW = 640;
    private const int BoxX = 80;
    private const int BoxY = 90;
    // Coordinates for in-box text and graphics are relative to the dialogue box's origin —
    // they are added as children of the box, so Entity.Render translates them automatically.
    private const int ContinueHintH = 22;
    private const int ContinueHintY = BoxH - Padding - ContinueHintH;
    private const int ControlsY = BoxY + BoxH + 25;
    private const int DecreaseSpeedX = 225;
    private const int DialogueH = ContinueHintY - DialogueY - Padding;
    private const int DialogueY = NameY + NameH + 8;
    private const int FastButtonX = IncreaseSpeedX + 40 + 10;
    private const int IncreaseSpeedX = SpeedLabelX + 120 + 5;
    private const int NameH = 22;
    private const int NameY = Padding;
    private const int Padding = 10;
    private const int PortraitW = 100;
    private const int SpeedLabelX = DecreaseSpeedX + 40 + 5;
    private const int TextW = BoxW - PortraitW - Padding * 2;
    private const int TextX = PortraitW + Padding;

    private static readonly float[] s_speeds = [5f, 15f, 30f, 60f];
    private static readonly string[] s_speedLabels = ["Slow", "Normal", "Fast", "Very Fast"];
    private static readonly (string Speaker, string Text)[] s_pages =
    [
        ("Village Elder", "Welcome, stranger. You arrive at a strange hour — the lanterns have not burned these past three nights. Something has disturbed the old stones."),
        ("Village Elder", "Three relics were stolen from the Shrine of Embers at the new moon: the Hearthstone, the Lantern of Accord, and the Silver Bell. Without them, the cold does not leave this valley."),
        ("Village Elder", "I have heard that a traveller of your... reputation... has dealt with such matters before. Will you seek out what was taken and return it to us?"),
        ("You",           "..."),
        ("You",           "Where do I even start looking?"),
    ];

    private float _charsRevealed = 0f;
    private readonly TextGraphic _continueHint;
    private readonly TextGraphic _dialogueText;
    private int _pageIndex = 0;
    private readonly TextGraphic _speakerLabel;
    private int _speedIndex = 1;
    private readonly TextGraphic _speedLabel;
    private bool _speedingUp = false;

    public override string Name => "Typewriter Demo";

    public TypewriterDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 235, 220);

        Add(new TextGraphic(new FontDefinition("Times New Roman", 12),
            "Click the dialogue box to reveal all text, then click again to advance.",
            new Colour(80, 70, 60), BoxX, 55, BoxW, 25)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });

        Entity dialogueBox = new(BoxX, BoxY, BoxW, BoxH, clickable: true);
        dialogueBox.Add(new ColourGraphic(new Colour(250, 245, 230), 0, 0, BoxW, BoxH));
        dialogueBox.Add(new ColourGraphic(new Colour(140, 120, 100), 0, 0, PortraitW, BoxH));
        dialogueBox.LeftClicked += OnDialogueClicked;

        _speakerLabel = new TextGraphic(
            new FontDefinition("Times New Roman", 12),
            s_pages[0].Speaker, new Colour(110, 85, 60),
            TextX, NameY, TextW, NameH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        dialogueBox.Add(_speakerLabel);

        _dialogueText = new TextGraphic(
            new FontDefinition("Times New Roman", 14),
            s_pages[0].Text, Colour.Black,
            TextX, DialogueY, TextW, DialogueH)
        {
            Multiline = true,
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
            NumCharsToDraw = 0,
        };
        dialogueBox.Add(_dialogueText);

        _continueHint = new TextGraphic(
            new FontDefinition("Times New Roman", 12),
            "", new Colour(110, 85, 60),
            TextX, ContinueHintY, TextW, ContinueHintH)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        };
        dialogueBox.Add(_continueHint);

        Add(dialogueBox);

        _speedLabel = new TextGraphic(
            new FontDefinition("Times New Roman", 13),
            s_speedLabels[_speedIndex], Colour.Black,
            SpeedLabelX, ControlsY, 120, 40)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_speedLabel);

        Button decreaseSpeed = new(DecreaseSpeedX, ControlsY, 40, 40,
            ButtonTheme.FromColour(new Colour(200, 190, 180)), "<");
        decreaseSpeed.LeftClicked += (_, _) =>
        {
            _speedIndex = Math.Max(0, _speedIndex - 1);
            _speedLabel.Text = s_speedLabels[_speedIndex];
        };
        Add(decreaseSpeed);

        Button increaseSpeed = new(IncreaseSpeedX, ControlsY, 40, 40,
            ButtonTheme.FromColour(new Colour(200, 190, 180)), ">");
        increaseSpeed.LeftClicked += (_, _) =>
        {
            _speedIndex = Math.Min(s_speeds.Length - 1, _speedIndex + 1);
            _speedLabel.Text = s_speedLabels[_speedIndex];
        };
        Add(increaseSpeed);

        Button fastButton = new(FastButtonX, ControlsY, 130, 40,
            ButtonTheme.FromColour(new Colour(200, 170, 255)), "Hold: Fast x5");
        fastButton.LeftPressed += (_, _) => _speedingUp = true;
        fastButton.LeftClicked += (_, _) => _speedingUp = false;
        fastButton.MouseExited += (_, _) => _speedingUp = false;
        Add(fastButton);
    }

    private (string Speaker, string Text) CurrentPage => s_pages[_pageIndex];

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        int totalChars = CurrentPage.Text.Length;
        bool fullyRevealed = _charsRevealed >= totalChars;

        if (!fullyRevealed)
        {
            float rate = _speedingUp ? s_speeds[_speedIndex] * 5f : s_speeds[_speedIndex];
            _charsRevealed = Math.Min(_charsRevealed + (float)(elapsed * rate), totalChars);
            _dialogueText.NumCharsToDraw = (int)_charsRevealed;
            _continueHint.Text = "";
        }
        else
        {
            _dialogueText.NumCharsToDraw = int.MaxValue;
            _continueHint.Text = _pageIndex < s_pages.Length - 1
                ? "[ click to continue → ]"
                : "[ click to start again ]";
        }
    }

    private void OnDialogueClicked(object? sender, EventArgs e)
    {
        int totalChars = CurrentPage.Text.Length;

        if (_charsRevealed < totalChars)
        {
            _charsRevealed = totalChars;
        }
        else
        {
            _pageIndex = (_pageIndex + 1) % s_pages.Length;
            _charsRevealed = 0f;
            _dialogueText.Text = CurrentPage.Text;
            _dialogueText.NumCharsToDraw = 0;
            _speakerLabel.Text = CurrentPage.Speaker;
            _continueHint.Text = "";
        }
    }
}

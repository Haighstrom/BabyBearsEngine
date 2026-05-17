using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.MouseDemo;

internal class MouseDemoWorld : DemoWorld
{
    private const int MaxLogLines = 12;
    private const float LogLineH = 22f;
    private const float LogX = 420f;
    private const float LogY = 96f;

    // Mouse visual origin (top-left of chassis)
    private const float MX = 80f;
    private const float MY = 70f;

    private static readonly Colour s_buttonActive = new(80, 150, 220);
    private static readonly Colour s_buttonInactive = new(140, 140, 140);
    private static readonly FontDefinition s_font = new("Times New Roman", 13);
    private static readonly Colour s_scrollDown = new(220, 140, 50);
    private static readonly Colour s_scrollNeutral = new(90, 90, 90);
    private static readonly Colour s_scrollUp = new(80, 200, 100);
    private static readonly FontDefinition s_smallFont = new("Times New Roman", 11);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private readonly TextGraphic _deltaText;
    private readonly TextGraphic[] _eventLabels = new TextGraphic[MaxLogLines];
    private readonly ColourGraphic _lmbGraphic;
    private readonly ColourGraphic _mb3Graphic;
    private readonly ColourGraphic _mb4Graphic;
    private readonly ColourGraphic _mb5Graphic;
    private readonly TextGraphic _posText;
    private readonly ColourGraphic _rmbGraphic;
    private readonly ColourGraphic _scrollGraphic;
    private readonly TextGraphic _wheelText;

    private readonly string[] _eventLogLines = new string[MaxLogLines];
    private double _scrollFlashTimer = 0;
    private bool _scrollWasUp = false;

    public MouseDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 240, 240);

        BuildMouseVisual(out _lmbGraphic, out _rmbGraphic, out _mb3Graphic,
            out _scrollGraphic, out _mb4Graphic, out _mb5Graphic);

        // State section
        Add(new TextGraphic(s_titleFont, "State", Colour.DimGray, MX, 330f, 280f, 24f)
        {
            HAlignment = HAlignment.Left,
        });

        _posText = new TextGraphic(s_font, "", Colour.DimGray, MX, 360f, 280f, 20f)
        {
            HAlignment = HAlignment.Left,
        };
        _deltaText = new TextGraphic(s_font, "", Colour.DimGray, MX, 382f, 280f, 20f)
        {
            HAlignment = HAlignment.Left,
        };
        _wheelText = new TextGraphic(s_font, "", Colour.DimGray, MX, 404f, 280f, 20f)
        {
            HAlignment = HAlignment.Left,
        };
        Add(_posText);
        Add(_deltaText);
        Add(_wheelText);

        // Event log
        Add(new TextGraphic(s_titleFont, "Events", Colour.DimGray, LogX, 62f, 340f, 26f)
        {
            HAlignment = HAlignment.Left,
        });

        for (int i = 0; i < MaxLogLines; i++)
        {
            TextGraphic label = new(s_font, "", Colour.DimGray, LogX, LogY + i * LogLineH, 360f, LogLineH)
            {
                HAlignment = HAlignment.Left,
                VAlignment = VAlignment.Centred,
            };
            _eventLabels[i] = label;
            Add(label);
        }
    }

    public override string Name => "Mouse Demo";

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        // Button highlight
        _lmbGraphic.Colour = Mouse.LeftDown ? s_buttonActive : s_buttonInactive;
        _rmbGraphic.Colour = Mouse.RightDown ? s_buttonActive : s_buttonInactive;
        _mb3Graphic.Colour = Mouse.MiddleDown ? s_buttonActive : s_scrollNeutral;
        _mb4Graphic.Colour = Mouse.ButtonDown(MouseButton.Button4) ? s_buttonActive : s_buttonInactive;
        _mb5Graphic.Colour = Mouse.ButtonDown(MouseButton.Button5) ? s_buttonActive : s_buttonInactive;

        // Scroll wheel flash
        if (Mouse.WheelDelta != 0)
        {
            _scrollFlashTimer = 0.25;
            _scrollWasUp = Mouse.WheelDelta > 0;
        }
        else if (_scrollFlashTimer > 0)
        {
            _scrollFlashTimer = Math.Max(0, _scrollFlashTimer - elapsed);
        }

        _scrollGraphic.Colour = _scrollFlashTimer > 0
            ? (_scrollWasUp ? s_scrollUp : s_scrollDown)
            : s_scrollNeutral;

        // Log events
        if (Mouse.LeftPressed)                            LogEvent("LMB pressed");
        if (Mouse.LeftReleased)                           LogEvent("LMB released");
        if (Mouse.RightPressed)                           LogEvent("RMB pressed");
        if (Mouse.RightReleased)                          LogEvent("RMB released");
        if (Mouse.MiddlePressed)                          LogEvent("MB3 pressed");
        if (Mouse.MiddleReleased)                         LogEvent("MB3 released");
        if (Mouse.ButtonPressed(MouseButton.Button4))     LogEvent("MB4 pressed");
        if (Mouse.ButtonReleased(MouseButton.Button4))    LogEvent("MB4 released");
        if (Mouse.ButtonPressed(MouseButton.Button5))     LogEvent("MB5 pressed");
        if (Mouse.ButtonReleased(MouseButton.Button5))    LogEvent("MB5 released");
        if (Mouse.WheelDelta > 0)  LogEvent($"Scroll up  ({Mouse.WheelDelta:+0.0})");
        if (Mouse.WheelDelta < 0)  LogEvent($"Scroll down  ({Mouse.WheelDelta:0.0})");

        // Stats
        _posText.Text   = $"Position    {Mouse.ClientX}, {Mouse.ClientY}";
        _deltaText.Text = $"Movement  {Mouse.XDelta:+0;-0;0}, {Mouse.YDelta:+0;-0;0}";
        _wheelText.Text = $"Wheel        {Mouse.WheelDelta:+0.0;-0.0;0.0}";
    }

    private void BuildMouseVisual(
        out ColourGraphic lmb, out ColourGraphic rmb, out ColourGraphic mb3,
        out ColourGraphic scroll, out ColourGraphic mb4, out ColourGraphic mb5)
    {
        // Chassis (dark background the buttons sit on)
        Add(new ColourGraphic(new Colour(75, 75, 75), MX, MY, 200f, 240f));

        // Left button (MB1)
        lmb = new ColourGraphic(s_buttonInactive, MX + 3f, MY + 3f, 86f, 88f);
        Add(lmb);

        // Middle button (MB3) — top portion of center strip
        mb3 = new ColourGraphic(s_scrollNeutral, MX + 94f, MY + 3f, 12f, 55f);
        Add(mb3);

        // Scroll wheel indicator — lower portion of center strip
        scroll = new ColourGraphic(s_scrollNeutral, MX + 94f, MY + 60f, 12f, 30f);
        Add(scroll);

        // Right button (MB2)
        rmb = new ColourGraphic(s_buttonInactive, MX + 111f, MY + 3f, 86f, 88f);
        Add(rmb);

        // Lower body
        Add(new ColourGraphic(new Colour(105, 105, 105), MX + 3f, MY + 93f, 194f, 144f));

        // Side buttons (MB4, MB5) — extend left from the body
        mb4 = new ColourGraphic(s_buttonInactive, MX - 17f, MY + 120f, 22f, 28f);
        Add(mb4);

        mb5 = new ColourGraphic(s_buttonInactive, MX - 17f, MY + 153f, 22f, 28f);
        Add(mb5);

        // Labels
        float labelY = MY + 244f;
        Add(MakeSmallLabel("LMB", MX + 3f,   labelY, 86f));
        Add(MakeSmallLabel("MB3", MX + 88f,   labelY, 24f));
        Add(MakeSmallLabel("RMB", MX + 111f,  labelY, 86f));
        Add(MakeSmallLabel("MB4", MX - 44f,   MY + 120f, 28f));
        Add(MakeSmallLabel("MB5", MX - 44f,   MY + 153f, 28f));
    }

    private void LogEvent(string text)
    {
        Array.Copy(_eventLogLines, 0, _eventLogLines, 1, MaxLogLines - 1);
        _eventLogLines[0] = text;

        for (int i = 0; i < MaxLogLines; i++)
        {
            _eventLabels[i].Text = _eventLogLines[i] ?? "";
        }
    }

    private static TextGraphic MakeSmallLabel(string text, float x, float y, float w)
    {
        return new TextGraphic(s_smallFont, text, Colour.DimGray, x, y, w, 18f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
    }
}

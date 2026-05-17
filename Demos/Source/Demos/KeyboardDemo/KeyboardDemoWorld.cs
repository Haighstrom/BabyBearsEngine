using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.KeyboardDemo;

internal class KeyboardDemoWorld : DemoWorld
{
    private sealed class KeyChip(ColourGraphic background, TextGraphic label, int slotIndex)
    {
        public double FlashTimer = 0.0;
        public bool IsReleased = false;

        public ColourGraphic Background => background;
        public TextGraphic Label => label;
        public int SlotIndex => slotIndex;
    }

    private const float ChipGap = 6f;
    private const float ChipH = 48f;
    private const int ChipCols = 7;
    private const float ChipGridX = 20f;
    private const float ChipGridY = 90f;
    private const int ChipRows = 3;
    private const float ChipW = 58f;
    private const double FlashDuration = 0.3;
    private const float LogLineH = 22f;
    private const float LogX = 490f;
    private const float LogY = 96f;
    private const int MaxChips = ChipCols * ChipRows;
    private const int MaxLogLines = 12;

    private static readonly Keys[] s_allKeys = BuildAllKeys();
    private static readonly Colour s_chipColourDown = new(70, 130, 200);
    private static readonly Colour s_chipColourPressed = new(100, 210, 100);
    private static readonly Colour s_chipColourReleased = new(220, 150, 50);
    private static readonly FontDefinition s_chipFont = new("Times New Roman", 11);
    private static readonly FontDefinition s_font = new("Times New Roman", 13);
    private static readonly Dictionary<Keys, string> s_keyLabels = BuildKeyLabels();
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private readonly Dictionary<Keys, KeyChip> _chips = new();
    private readonly TextGraphic[] _eventLabels = new TextGraphic[MaxLogLines];
    private readonly string[] _eventLogLines = new string[MaxLogLines];
    private readonly List<Keys> _keysToRemove = [];
    private readonly bool[] _slotOccupied = new bool[MaxChips];

    public KeyboardDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 240, 240);

        Add(new TextGraphic(s_titleFont, "Active Keys", Colour.DimGray, ChipGridX, 62f, 440f, 24f)
        {
            HAlignment = HAlignment.Left,
        });

        Add(new TextGraphic(s_titleFont, "Events", Colour.DimGray, LogX, 62f, 290f, 24f)
        {
            HAlignment = HAlignment.Left,
        });

        for (int i = 0; i < MaxLogLines; i++)
        {
            TextGraphic label = new(s_font, "", Colour.DimGray, LogX, LogY + i * LogLineH, 295f, LogLineH)
            {
                HAlignment = HAlignment.Left,
                VAlignment = VAlignment.Centred,
            };
            _eventLabels[i] = label;
            Add(label);
        }
    }

    public override string Name => "Keyboard Demo";

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        foreach (Keys key in s_allKeys)
        {
            if (Keyboard.KeyPressed(key) && !_chips.ContainsKey(key))
            {
                SpawnChip(key);
                LogEvent($"{KeyDisplayName(key)} pressed");
            }
        }

        foreach (var (key, chip) in _chips)
        {
            if (!chip.IsReleased && Keyboard.KeyReleased(key))
            {
                chip.IsReleased = true;
                chip.FlashTimer = FlashDuration;
                LogEvent($"{KeyDisplayName(key)} released");
            }

            chip.FlashTimer = Math.Max(0, chip.FlashTimer - elapsed);

            if (chip.IsReleased)
            {
                chip.Background.Colour = s_chipColourReleased;
                if (chip.FlashTimer <= 0)
                    _keysToRemove.Add(key);
            }
            else
            {
                chip.Background.Colour = chip.FlashTimer > 0 ? s_chipColourPressed : s_chipColourDown;
            }
        }

        foreach (Keys key in _keysToRemove)
            RemoveChip(key);

        _keysToRemove.Clear();
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < MaxChips; i++)
        {
            if (!_slotOccupied[i])
                return i;
        }

        return -1;
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

    private void RemoveChip(Keys key)
    {
        if (!_chips.TryGetValue(key, out KeyChip? chip))
            return;

        chip.Background.Remove();
        chip.Label.Remove();
        _slotOccupied[chip.SlotIndex] = false;
        _chips.Remove(key);
    }

    private void SpawnChip(Keys key)
    {
        int slot = FindFreeSlot();
        if (slot < 0)
            return;

        float x = ChipGridX + (slot % ChipCols) * (ChipW + ChipGap);
        float y = ChipGridY + (slot / ChipCols) * (ChipH + ChipGap);

        ColourGraphic bg = new(s_chipColourPressed, x, y, ChipW, ChipH);
        TextGraphic label = new(s_chipFont, KeyDisplayName(key), Colour.White, x, y, ChipW, ChipH);
        Add(bg);
        Add(label);

        _chips[key] = new(bg, label, slot) { FlashTimer = FlashDuration };
        _slotOccupied[slot] = true;
    }

    private static Keys[] BuildAllKeys()
    {
        return [.. Enum.GetValues<Keys>().Where(k => k != Keys.Unknown).Distinct()];
    }

    private static Dictionary<Keys, string> BuildKeyLabels()
    {
        return new()
        {
            [Keys.Space] = "SPC",
            [Keys.Apostrophe] = "'",
            [Keys.Comma] = ",",
            [Keys.Minus] = "-",
            [Keys.Period] = ".",
            [Keys.Slash] = "/",
            [Keys.D0] = "0",   [Keys.D1] = "1",   [Keys.D2] = "2",
            [Keys.D3] = "3",   [Keys.D4] = "4",   [Keys.D5] = "5",
            [Keys.D6] = "6",   [Keys.D7] = "7",   [Keys.D8] = "8",   [Keys.D9] = "9",
            [Keys.Semicolon] = ";",
            [Keys.Equal] = "=",
            [Keys.LeftBracket] = "[",
            [Keys.Backslash] = "\\",
            [Keys.RightBracket] = "]",
            [Keys.GraveAccent] = "`",
            [Keys.Escape] = "Esc",
            [Keys.Enter] = "Ret",
            [Keys.Tab] = "Tab",
            [Keys.Backspace] = "BSp",
            [Keys.Insert] = "Ins",
            [Keys.Delete] = "Del",
            [Keys.Right] = "→",
            [Keys.Left] = "←",
            [Keys.Down] = "↓",
            [Keys.Up] = "↑",
            [Keys.PageUp] = "PgUp",
            [Keys.PageDown] = "PgDn",
            [Keys.Home] = "Home",
            [Keys.End] = "End",
            [Keys.CapsLock] = "Caps",
            [Keys.ScrollLock] = "SLck",
            [Keys.NumLock] = "NLck",
            [Keys.PrintScreen] = "Prnt",
            [Keys.Pause] = "Paus",
            [Keys.LeftShift] = "LSft",
            [Keys.RightShift] = "RSft",
            [Keys.LeftControl] = "LCtl",
            [Keys.RightControl] = "RCtl",
            [Keys.LeftAlt] = "LAlt",
            [Keys.RightAlt] = "RAlt",
            [Keys.LeftSuper] = "LWin",
            [Keys.RightSuper] = "RWin",
            [Keys.Menu] = "Menu",
            [Keys.KeyPad0] = "KP 0", [Keys.KeyPad1] = "KP 1", [Keys.KeyPad2] = "KP 2",
            [Keys.KeyPad3] = "KP 3", [Keys.KeyPad4] = "KP 4", [Keys.KeyPad5] = "KP 5",
            [Keys.KeyPad6] = "KP 6", [Keys.KeyPad7] = "KP 7", [Keys.KeyPad8] = "KP 8",
            [Keys.KeyPad9] = "KP 9",
            [Keys.KeyPadDecimal] = "KP .",
            [Keys.KeyPadDivide] = "KP /",
            [Keys.KeyPadMultiply] = "KP *",
            [Keys.KeyPadSubtract] = "KP -",
            [Keys.KeyPadAdd] = "KP +",
            [Keys.KeyPadEnter] = "KPRet",
            [Keys.KeyPadEqual] = "KP =",
        };
    }

    private static string KeyDisplayName(Keys key)
    {
        return s_keyLabels.TryGetValue(key, out string? label) ? label : key.ToString();
    }
}

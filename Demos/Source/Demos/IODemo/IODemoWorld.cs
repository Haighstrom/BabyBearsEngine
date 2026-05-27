using System;
using System.IO;
using System.Linq;
using BabyBearsEngine.IO;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.IODemo;

internal class IODemoWorld : DemoWorld
{
    private const float SwatchGap = 20f;
    private const float SwatchH = 80f;
    private const int SwatchCount = 5;
    private const float SwatchW = 100f;
    private const float SwatchY = 110f;

    private static readonly FontDefinition s_labelFont = new("Times New Roman", 12);
    private static readonly string s_savePath = Path.Combine(Files.AppDataDirectory("BabyBearsEngine"), "demo_palette.json");
    private static readonly FontDefinition s_statusFont = new("Times New Roman", 14);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 22);

    private readonly TextGraphic[] _hexLabels = new TextGraphic[SwatchCount];
    private readonly TextGraphic _statusLabel;
    private readonly ColourSwatch[] _swatches = new ColourSwatch[SwatchCount];

    public IODemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(245, 245, 245);

        Add(new TextGraphic(s_titleFont, "Colour Palette Editor", Colour.Black, 0f, 20f, 800f, 40f));
        Add(new TextGraphic(s_labelFont, "Click a swatch to cycle its colour.", Colour.DimGray, 0f, 68f, 800f, 22f));

        float swatchTotalW = SwatchCount * SwatchW + (SwatchCount - 1) * SwatchGap;
        float swatchStartX = (800f - swatchTotalW) / 2f;

        for (int i = 0; i < SwatchCount; i++)
        {
            float x = swatchStartX + i * (SwatchW + SwatchGap);

            ColourSwatch swatch = new(x, SwatchY, SwatchW, SwatchH, i * 2);
            int capturedIndex = i;
            swatch.ColourChanged += (_, _) => UpdateHexLabel(capturedIndex);
            _swatches[i] = swatch;
            Add(swatch);

            TextGraphic hexLabel = new(s_labelFont, "", Colour.DimGray, x, SwatchY + SwatchH + 5f, SwatchW, 18f);
            _hexLabels[i] = hexLabel;
            Add(hexLabel);
        }

        for (int i = 0; i < SwatchCount; i++)
        {
            UpdateHexLabel(i);
        }

        float saveX = 800f / 2f - 145f;
        Button saveBtn = new(saveX, 280f, 140f, 40f, ButtonTheme.FromColour(new Colour(80, 160, 80)), "Save Palette");
        saveBtn.LeftClicked += (_, _) => SavePalette();
        Add(saveBtn);

        Button loadBtn = new(saveX + 150f, 280f, 140f, 40f, ButtonTheme.FromColour(new Colour(80, 120, 200)), "Load Palette");
        loadBtn.LeftClicked += (_, _) => LoadPalette();
        Add(loadBtn);

        _statusLabel = new TextGraphic(s_statusFont, "", Colour.DimGray, 0f, 340f, 800f, 28f);
        Add(_statusLabel);
    }

    public override string Name => "IO Demo";

    private void LoadPalette()
    {
        Colour[]? loaded = Files.TryReadJsonFile<Colour[]>(s_savePath);

        if (loaded is null)
        {
            SetStatus("No saved palette found.", new Colour(160, 100, 40));
            return;
        }

        for (int i = 0; i < Math.Min(loaded.Length, SwatchCount); i++)
        {
            _swatches[i].Colour = loaded[i];
            UpdateHexLabel(i);
        }

        SetStatus("Palette loaded.", new Colour(50, 120, 50));
    }

    private void SavePalette()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(s_savePath)!);
            Colour[] colours = [.. _swatches.Select(s => s.Colour)];
            Files.WriteJsonFile(s_savePath, colours);
            SetStatus("Palette saved.", new Colour(50, 120, 50));
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}", new Colour(180, 50, 50));
        }
    }

    private void SetStatus(string text, Colour colour)
    {
        _statusLabel.Text = text;
        _statusLabel.Colour = colour;
    }

    private void UpdateHexLabel(int index)
    {
        _hexLabels[index].Text = _swatches[index].Colour.ToHex()[..7];
    }
}

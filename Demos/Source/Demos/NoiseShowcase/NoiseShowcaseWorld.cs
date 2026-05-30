using System;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Utilities.Noise;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Demos.Source.Demos.NoiseShowcase;

/// <summary>
/// Showcases procedural noise. Toggle between Simplex (single octave), Simplex with fBm
/// (octave-summed), Voronoi F1 (distance to nearest feature point) and Voronoi cell edges
/// (F2 - F1). Sliders for scale, octaves, persistence, and seed regenerate the preview
/// texture each click.
/// </summary>
internal sealed class NoiseShowcaseWorld : DemoWorld
{
    private enum NoiseMode
    {
        Simplex,
        SimplexFbm,
        VoronoiF1,
        VoronoiEdges,
    }

    // Preview is sampled at this resolution then drawn into a larger on-screen square.
    private const int PreviewResolution = 256;
    private const float PreviewX = 380f;
    private const float PreviewY = 70f;
    private const float PreviewSize = 400f;

    private const float PanelX = 20f;
    private const float PanelStartY = 70f;
    private const float ButtonWidth = 160f;
    private const float ButtonHeight = 28f;
    private const float LabelWidth = 100f;
    private const float ValueWidth = 60f;
    private const float SmallButton = 28f;
    private const float RowGap = 6f;
    private const float SectionGap = 14f;

    private static readonly FontDefinition s_titleFont = new("Times New Roman", 18);
    private static readonly FontDefinition s_bodyFont = new("Times New Roman", 14);
    private static readonly Colour s_uiText = new(40, 40, 40);
    private static readonly Colour s_modeButton = new(180, 200, 230);
    private static readonly Colour s_minusButton = new(180, 90, 90);
    private static readonly Colour s_plusButton = new(70, 150, 70);
    private static readonly Colour s_regenButton = new(120, 60, 160);

    private NoiseMode _mode = NoiseMode.Simplex;
    private float _scale = 8f;        // cells of noise across the preview's width
    private int _octaves = 4;         // used by Simplex+FBM
    private float _persistence = 0.5f; // used by Simplex+FBM
    private int _seed = 12345;

    private TextGraphic _modeLabel = null!;
    private TextGraphic _scaleValue = null!;
    private TextGraphic _octavesValue = null!;
    private TextGraphic _persistenceValue = null!;
    private TextGraphic _seedValue = null!;

    private ITexture? _previewTexture = null;
    private TextureGraphic? _previewGraphic = null;

    public NoiseShowcaseWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(245, 245, 250);

        Add(new TextGraphic(s_titleFont, "Procedural Noise Showcase", s_uiText, 0f, 15f, Window.Width, 28f)
        {
            HAlignment = HAlignment.Centred,
        });

        // Preview frame — drawn behind the texture so the preview always sits on something
        // visually defined, even before the first generate.
        Add(new ColourGraphic(new Colour(20, 20, 30), PreviewX - 2f, PreviewY - 2f, PreviewSize + 4f, PreviewSize + 4f, layer: 5));

        BuildControlsPanel();
        Regenerate();
    }

    public override string Name => "Noise Showcase";

    private void BuildControlsPanel()
    {
        float y = PanelStartY;

        AddSectionLabel("Mode", y);
        y += ButtonHeight + RowGap;

        _modeLabel = new TextGraphic(s_bodyFont, "", new Colour(40, 80, 140), PanelX, y, ButtonWidth, ButtonHeight)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_modeLabel);
        y += ButtonHeight + RowGap;

        (NoiseMode mode, string label)[] modes =
        [
            (NoiseMode.Simplex, "Simplex"),
            (NoiseMode.SimplexFbm, "Simplex (fBm)"),
            (NoiseMode.VoronoiF1, "Voronoi F1"),
            (NoiseMode.VoronoiEdges, "Voronoi Edges"),
        ];
        foreach ((NoiseMode mode, string label) in modes)
        {
            Button modeButton = new(PanelX, y, ButtonWidth, ButtonHeight, ButtonTheme.FromColour(s_modeButton), label);
            modeButton.LeftClicked += (_, _) =>
            {
                _mode = mode;
                Regenerate();
            };
            Add(modeButton);
            y += ButtonHeight + RowGap;
        }
        y += SectionGap;

        _scaleValue = AddAdjuster(y, "Scale:", () => _scaleValue!, () => _scale.ToString("0.0"),
            decrease: () => _scale = Math.Max(1f, _scale - 1f),
            increase: () => _scale = Math.Min(64f, _scale + 1f));
        y += ButtonHeight + RowGap;

        _octavesValue = AddAdjuster(y, "Octaves:", () => _octavesValue!, () => _octaves.ToString(),
            decrease: () => _octaves = Math.Max(1, _octaves - 1),
            increase: () => _octaves = Math.Min(10, _octaves + 1));
        y += ButtonHeight + RowGap;

        _persistenceValue = AddAdjuster(y, "Persistence:", () => _persistenceValue!, () => _persistence.ToString("0.00"),
            decrease: () => _persistence = Math.Max(0.05f, _persistence - 0.05f),
            increase: () => _persistence = Math.Min(1f, _persistence + 0.05f));
        y += ButtonHeight + RowGap;

        _seedValue = AddAdjuster(y, "Seed:", () => _seedValue!, () => _seed.ToString(),
            decrease: () => _seed--,
            increase: () => _seed++);
        y += ButtonHeight + SectionGap;

        Button regenerate = new(PanelX, y, ButtonWidth, ButtonHeight + 6f, ButtonTheme.FromColour(s_regenButton), "Regenerate");
        regenerate.LeftClicked += (_, _) => Regenerate();
        Add(regenerate);
    }

    private TextGraphic AddAdjuster(float y, string label, Func<TextGraphic> valueGetter, Func<string> valueText, Action decrease, Action increase)
    {
        Add(new TextGraphic(s_bodyFont, label, s_uiText, PanelX, y, LabelWidth, ButtonHeight)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        float minusX = PanelX + LabelWidth;
        float valueX = minusX + SmallButton + 4f;
        float plusX = valueX + ValueWidth + 4f;

        Button minus = new(minusX, y, SmallButton, ButtonHeight, ButtonTheme.FromColour(s_minusButton), "-");
        minus.LeftClicked += (_, _) =>
        {
            decrease();
            valueGetter().Text = valueText();
            Regenerate();
        };
        Add(minus);

        TextGraphic value = new(s_bodyFont, valueText(), s_uiText, valueX, y, ValueWidth, ButtonHeight)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(value);

        Button plus = new(plusX, y, SmallButton, ButtonHeight, ButtonTheme.FromColour(s_plusButton), "+");
        plus.LeftClicked += (_, _) =>
        {
            increase();
            valueGetter().Text = valueText();
            Regenerate();
        };
        Add(plus);

        return value;
    }

    private void AddSectionLabel(string label, float y)
    {
        Add(new TextGraphic(s_bodyFont, label, s_uiText, PanelX, y, ButtonWidth, ButtonHeight)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
    }

    private void Regenerate()
    {
        if (_previewGraphic is not null)
        {
            Remove(_previewGraphic);
            _previewGraphic.Dispose();
            _previewGraphic = null;
        }
        _previewTexture?.Dispose();
        _previewTexture = null;

        Colour[,] pixels = RenderNoiseToPixels();
        _previewTexture = Textures.GenTexture(pixels);
        _previewGraphic = new TextureGraphic(_previewTexture, PreviewX, PreviewY, PreviewSize, PreviewSize, layer: 4);
        Add(_previewGraphic);

        _modeLabel.Text = _mode switch
        {
            NoiseMode.Simplex => "Single-octave gradient noise",
            NoiseMode.SimplexFbm => "fBm — octave-summed Simplex",
            NoiseMode.VoronoiF1 => "Distance to nearest feature point",
            NoiseMode.VoronoiEdges => "Cell-edge distance (F2 - F1)",
            _ => "",
        };
    }

    private Colour[,] RenderNoiseToPixels()
    {
        var pixels = new Colour[PreviewResolution, PreviewResolution];

        switch (_mode)
        {
            case NoiseMode.Simplex:
                {
                    SimplexNoise noise = new(_seed);
                    for (int pixelY = 0; pixelY < PreviewResolution; pixelY++)
                    {
                        for (int pixelX = 0; pixelX < PreviewResolution; pixelX++)
                        {
                            float sampleX = pixelX / (float)PreviewResolution * _scale;
                            float sampleY = pixelY / (float)PreviewResolution * _scale;
                            float value = noise.Sample(sampleX, sampleY);
                            pixels[pixelX, pixelY] = ValueToGrey(NormaliseFromSigned(value));
                        }
                    }
                    break;
                }

            case NoiseMode.SimplexFbm:
                {
                    SimplexNoise noise = new(_seed);
                    for (int pixelY = 0; pixelY < PreviewResolution; pixelY++)
                    {
                        for (int pixelX = 0; pixelX < PreviewResolution; pixelX++)
                        {
                            float sampleX = pixelX / (float)PreviewResolution * _scale;
                            float sampleY = pixelY / (float)PreviewResolution * _scale;
                            float value = Fbm.Sample(noise, sampleX, sampleY, octaves: _octaves, persistence: _persistence);
                            pixels[pixelX, pixelY] = ValueToGrey(NormaliseFromSigned(value));
                        }
                    }
                    break;
                }

            case NoiseMode.VoronoiF1:
                {
                    VoronoiNoise noise = new(_seed);
                    for (int pixelY = 0; pixelY < PreviewResolution; pixelY++)
                    {
                        for (int pixelX = 0; pixelX < PreviewResolution; pixelX++)
                        {
                            float sampleX = pixelX / (float)PreviewResolution * _scale;
                            float sampleY = pixelY / (float)PreviewResolution * _scale;
                            // F1 spans roughly [0, √2] in cell units; map to [0, 1] for display.
                            float normalised = Math.Clamp(noise.Sample(sampleX, sampleY).F1 / 1.4142f, 0f, 1f);
                            pixels[pixelX, pixelY] = ValueToGrey(normalised);
                        }
                    }
                    break;
                }

            case NoiseMode.VoronoiEdges:
                {
                    VoronoiNoise noise = new(_seed);
                    for (int pixelY = 0; pixelY < PreviewResolution; pixelY++)
                    {
                        for (int pixelX = 0; pixelX < PreviewResolution; pixelX++)
                        {
                            float sampleX = pixelX / (float)PreviewResolution * _scale;
                            float sampleY = pixelY / (float)PreviewResolution * _scale;
                            // EdgeDistance is small near cell borders; invert so edges show as bright lines.
                            float edge = noise.Sample(sampleX, sampleY).EdgeDistance;
                            float brightness = Math.Clamp(1f - (edge * 4f), 0f, 1f);
                            pixels[pixelX, pixelY] = ValueToGrey(brightness);
                        }
                    }
                    break;
                }
        }

        return pixels;
    }

    private static float NormaliseFromSigned(float signedValue)
    {
        return Math.Clamp((signedValue * 0.5f) + 0.5f, 0f, 1f);
    }

    private static Colour ValueToGrey(float normalised)
    {
        byte intensity = (byte)Math.Round(normalised * 255f);
        return new Colour(intensity, intensity, intensity, 255);
    }
}

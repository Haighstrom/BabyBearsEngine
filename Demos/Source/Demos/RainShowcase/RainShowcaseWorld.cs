using System;
using System.IO;
using BabyBearsEngine.AudioSystem;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.Particles;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Demos.Source.Demos.RainShowcase;

/// <summary>
/// A showcase scene: three parallax rain layers, occasional lightning flashes, and looping rain
/// audio. The control panel in the bottom-right lets you scrub wind, downpour intensity, music
/// volume, and toggle automatic lightning. The "Lightning!" button fires a manual strike.
/// </summary>
/// <remarks>
/// Optional asset paths under <c>Demos/Assets/RainShowcase/</c> — when present, they take over
/// from the procedural fallbacks: <c>background.png</c> replaces the flat dark sky, and
/// <c>raindrop.png</c> is sampled across every particle quad (giving real streak silhouettes
/// instead of solid-coloured rectangles). Missing assets are logged and silently fall back so
/// the demo still runs end-to-end.
/// </remarks>
internal sealed class RainShowcaseWorld : DemoWorld
{
    private const string BackgroundPath = "Assets/RainShowcase/background.png";
    private const string RaindropPath = "Assets/RainShowcase/raindrop.png";
    private const string RainMusicPath = "Assets/Audio/Music/rain-loop.wav";

    private const float DefaultIntensity = 1f;
    private const float MaxWindSpeed = 220f;
    private const float PanelW = 280f;
    private const float PanelH = 200f;
    private const float PanelPadding = 14f;
    private const float RowGap = 6f;
    private const float SliderH = 18f;

    private static readonly Colour s_skyTop = new(8, 12, 26);
    private static readonly Colour s_skyBottom = new(18, 24, 44);
    private static readonly Colour s_panelFill = new(0, 0, 0, 165);
    private static readonly Colour s_panelAccent = new(110, 150, 220);
    private static readonly Colour s_panelText = new(225, 230, 245);
    private static readonly Colour s_panelDim = new(170, 180, 200);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 22);
    private static readonly FontDefinition s_headerFont = new("Times New Roman", 15);
    private static readonly FontDefinition s_labelFont = new("Times New Roman", 13);

    private readonly Random _random = new();
    private readonly RainLayer[] _layers;
    private readonly LightningController _lightning;
    private readonly ColourGraphic _lightningOverlay;
    private IMusicClip? _rainClip = null;
    private float _wind = 0f;
    private float _intensity = DefaultIntensity;

    private TextGraphic _windValue = null!;
    private TextGraphic _intensityValue = null!;
    private TextGraphic _musicValue = null!;

    public RainShowcaseWorld(Func<World> menuWorldFactory)
        : base(WithAudioCleanup(menuWorldFactory))
    {
        BackgroundColour = s_skyBottom;

        ITexture? backgroundTexture = TryLoadTexture(BackgroundPath);
        ITexture? raindropTexture = TryLoadTexture(RaindropPath);

        AddBackdrop(backgroundTexture);
        _layers = BuildRainLayers(raindropTexture);
        foreach (RainLayer layer in _layers)
        {
            Add(layer.System);
        }

        _lightningOverlay = new ColourGraphic(new Colour(255, 255, 255, 0),
            0, 0, Window.Width, Window.Height, layer: 1);
        Add(_lightningOverlay);

        _lightning = new LightningController(_lightningOverlay, _random);
        Add(_lightning);

        BuildTitle();
        BuildControlsPanel();
        LoadAndStartMusic();
        ApplyWind();
        ApplyIntensity();
    }

    public override string Name => "Rain Showcase";

    private static Func<World> WithAudioCleanup(Func<World> menuFactory) => () =>
    {
        // Music belongs to this scene — silence it when the player backs out instead of letting
        // it bleed into the next world. Safe even when audio is unavailable (no-op).
        Audio.StopMusic();
        return menuFactory();
    };

    private static ITexture? TryLoadTexture(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return Textures.CreateFromFile(path);
        }
        catch (Exception ex)
        {
            Logger.Warning($"RainShowcase: failed to load texture '{path}': {ex.Message}");
            return null;
        }
    }

    private void AddBackdrop(ITexture? backgroundTexture)
    {
        if (backgroundTexture is not null)
        {
            Add(new TextureGraphic(backgroundTexture, 0, 0, Window.Width, Window.Height, layer: int.MaxValue));
            return;
        }

        // Stack two flat fills to fake a vertical gradient — top half is darker than bottom,
        // suggesting a heavy stormy sky overhead. The window background colour fills the
        // bottom half (s_skyBottom); the ColourGraphic paints the top half.
        Add(new ColourGraphic(s_skyTop, 0, 0, Window.Width, Window.Height / 2f, layer: int.MaxValue));
    }

    private RainLayer[] BuildRainLayers(ITexture? raindropTexture)
    {
        // Layer order, back to front: far → mid → near. Far particles are smaller, slower, and
        // dimmer — they read as distance. Near particles are larger, faster, and brighter so
        // the foreground feels close. Rendering order is enforced via the Layer property.
        return
        [
            BuildRainLayer(
                size: 2.0f,
                speed: 280f,
                colour: new Colour(150, 175, 210, 165),
                maxRate: 280f,
                particleLayer: 6,
                texture: raindropTexture),
            BuildRainLayer(
                size: 3.0f,
                speed: 480f,
                colour: new Colour(190, 210, 240, 200),
                maxRate: 360f,
                particleLayer: 5,
                texture: raindropTexture),
            BuildRainLayer(
                size: 4.5f,
                speed: 780f,
                colour: new Colour(230, 240, 255, 235),
                maxRate: 420f,
                particleLayer: 4,
                texture: raindropTexture),
        ];
    }

    private RainLayer BuildRainLayer(float size, float speed, Colour colour, float maxRate,
        int particleLayer, ITexture? texture)
    {
        // Lifetime tuned to cover the screen height plus a small buffer at the chosen speed —
        // any longer and we burn particles drawing below the window; any shorter and they fade
        // before reaching the bottom.
        float lifetime = (Window.Height + 80f) / speed;

        // Spawn line sits just above the visible window and extends past the edges so wind
        // never reveals the spawn boundary at high values.
        LineSegmentEmitterShape shape = new(
            start: new Point(-MaxWindSpeed, -30),
            end: new Point(Window.Width + MaxWindSpeed, -30),
            velocity: new Point(0, speed));

        ParticleSystem system = new(shape, particleLayer, _random)
        {
            EmissionRate = maxRate,
            Lifetime = lifetime,
            StartSize = size,
            Colour = colour,
            MaxParticles = 4000,
            Texture = texture,
        };
        return new RainLayer(system, shape, maxRate, speed);
    }

    private void BuildTitle()
    {
        Add(new TextGraphic(s_titleFont, "Rain Showcase", s_panelText, 95, 12, 600, 32)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
    }

    private void BuildControlsPanel()
    {
        float panelX = Window.Width - PanelW - 16f;
        float panelY = Window.Height - PanelH - 16f;

        Add(new ColourGraphic(s_panelFill, panelX, panelY, PanelW, PanelH, layer: 0));

        Add(new TextGraphic(s_headerFont, "Storm Controls", s_panelText,
            panelX + PanelPadding, panelY + 6, PanelW - 2 * PanelPadding, 24)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        float rowX = panelX + PanelPadding;
        float rowY = panelY + 34;
        float contentW = PanelW - 2 * PanelPadding;

        rowY = AddSliderRow(rowX, rowY, contentW, "Wind", FormatWind(_wind),
            initial: 0.5f,
            valueLabelSetter: t => _windValue = t,
            onChanged: v =>
            {
                _wind = (v - 0.5f) * 2f;
                ApplyWind();
                _windValue.Text = FormatWind(_wind);
            });

        rowY = AddSliderRow(rowX, rowY, contentW, "Rain", FormatIntensity(_intensity),
            initial: _intensity,
            valueLabelSetter: t => _intensityValue = t,
            onChanged: v =>
            {
                _intensity = v;
                ApplyIntensity();
                _intensityValue.Text = FormatIntensity(_intensity);
            });

        rowY = AddSliderRow(rowX, rowY, contentW, "Music", FormatVolume(Audio.MusicVolume),
            initial: Audio.MusicVolume,
            valueLabelSetter: t => _musicValue = t,
            onChanged: v =>
            {
                Audio.MusicVolume = v;
                _musicValue.Text = FormatVolume(v);
            });

        // Bottom row: Lightning button + Auto checkbox, side-by-side.
        float buttonW = 130f;
        float buttonH = 28f;
        Button lightningButton = new(rowX, rowY + 4, buttonW, buttonH,
            ButtonTheme.FromColour(s_panelAccent), "Lightning!");
        lightningButton.LeftClicked += (_, _) => _lightning.Trigger();
        Add(lightningButton);

        float boxX = rowX + buttonW + 14;
        Checkbox autoBox = new(boxX, rowY + 9, 18, 18, CheckboxTheme.Default, isChecked: false);
        autoBox.Checked += (_, _) => _lightning.AutoFlash = true;
        autoBox.Unchecked += (_, _) => _lightning.AutoFlash = false;
        Add(autoBox);
        Add(new TextGraphic(s_labelFont, "Auto", s_panelText, boxX + 24, rowY + 4, 70, buttonH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
    }

    private float AddSliderRow(float x, float y, float contentWidth, string label, string initialValueText,
        float initial, Action<TextGraphic> valueLabelSetter, Action<float> onChanged)
    {
        const float labelW = 48f;
        const float valueW = 50f;
        float sliderW = contentWidth - labelW - valueW - 12f;

        Add(new TextGraphic(s_labelFont, label, s_panelText, x, y, labelW, SliderH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Scrollbar slider = new(x + labelW, y, sliderW, SliderH, ScrollbarDirection.Horizontal,
            ScrollbarTheme.FromColours(new Colour(40, 50, 70), s_panelAccent),
            thumbProportion: 0.12f,
            amountFilled: Math.Clamp(initial, 0f, 1f));
        slider.ScrollChanged += (_, args) => onChanged(args.NewValue);
        Add(slider);

        TextGraphic valueLabel = new(s_labelFont, initialValueText, s_panelDim,
            x + labelW + sliderW + 6, y, valueW, SliderH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(valueLabel);
        valueLabelSetter(valueLabel);

        return y + SliderH + RowGap;
    }

    private void LoadAndStartMusic()
    {
        if (!File.Exists(RainMusicPath))
        {
            Logger.Warning($"RainShowcase: rain music file not found at '{RainMusicPath}' — demo runs silent.");
            return;
        }

        try
        {
            _rainClip = Audio.LoadMusic(RainMusicPath, "Rain Loop");
        }
        catch (Exception ex)
        {
            Logger.Warning($"RainShowcase: failed to load rain music: {ex.Message}");
            return;
        }

        Audio.Playlist.SetTracks(_rainClip);
        Audio.Playlist.Loop = true;
        Audio.Playlist.Play();
    }

    private void ApplyWind()
    {
        float horizontal = _wind * MaxWindSpeed;
        foreach (RainLayer layer in _layers)
        {
            // Wind affects only the horizontal component; vertical speed stays fixed so the
            // rain doesn't slow down as it blows sideways. Each layer keeps its own vertical
            // speed for the parallax depth effect.
            layer.Shape.Velocity = new Point(horizontal, layer.VerticalSpeed);
        }
    }

    private void ApplyIntensity()
    {
        foreach (RainLayer layer in _layers)
        {
            layer.System.EmissionRate = layer.MaxRate * _intensity;
            layer.System.Emitting = _intensity > 0.001f;
        }
    }

    private static string FormatWind(float wind)
    {
        if (Math.Abs(wind) < 0.02f)
        {
            return "still";
        }
        string direction = wind > 0 ? "→" : "←";
        return $"{direction} {Math.Abs(wind) * 100f:0}%";
    }

    private static string FormatIntensity(float intensity)
    {
        return $"{intensity * 100f:0}%";
    }

    private static string FormatVolume(float volume)
    {
        return $"{(int)Math.Round(volume * 100)}%";
    }

    private sealed class RainLayer(
        ParticleSystem system,
        LineSegmentEmitterShape shape,
        float maxRate,
        float verticalSpeed)
    {
        public ParticleSystem System { get; } = system;

        public LineSegmentEmitterShape Shape { get; } = shape;

        public float MaxRate { get; } = maxRate;

        public float VerticalSpeed { get; } = verticalSpeed;
    }
}

using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.Particles;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Demos.Source.Demos.ParticleDemo;

/// <summary>
/// Showcases the particle system: four emitter shapes (point, circle, line segment, rect) with
/// a sliders/buttons panel for switching shape, toggling emission, bursting, and tweaking
/// emission rate / particle lifetime. The visual effect uses a shrink-to-nothing size curve and
/// a fade-to-transparent colour curve so the four shapes look distinct at a glance.
/// </summary>
internal sealed class ParticleDemoWorld : DemoWorld
{
    private const float PanelX = 20f;
    private const float PanelY = 60f;
    private const float ButtonW = 130f;
    private const float ButtonH = 30f;
    private const float RowGap = 8f;
    private const float SectionGap = 18f;
    private const float LabelW = 110f;
    private const float ValueW = 60f;

    private static readonly FontDefinition s_titleFont = new("Times New Roman", 18);
    private static readonly FontDefinition s_bodyFont = new("Times New Roman", 14);
    private static readonly Colour s_accent = new(120, 60, 160);
    private static readonly Colour s_minusButton = new(180, 90, 90);
    private static readonly Colour s_plusButton = new(70, 150, 70);

    private readonly ParticleSystem _particles;
    private readonly PointEmitterShape _pointShape;
    private readonly CircleEmitterShape _circleShape;
    private readonly LineSegmentEmitterShape _lineShape;
    private readonly RectEmitterShape _rectShape;

    private TextGraphic _shapeLabel = null!;
    private TextGraphic _emissionRateValue = null!;
    private TextGraphic _lifetimeValue = null!;
    private TextGraphic _countValue = null!;
    private Button _toggleButton = null!;

    public ParticleDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(10, 10, 20);

        float windowCentreX = Window.Width / 2f;
        float windowCentreY = Window.Height / 2f;

        _pointShape = new PointEmitterShape(
            origin: new Point(windowCentreX, windowCentreY),
            velocity: new Point(0, -90));
        _circleShape = new CircleEmitterShape(
            centre: new Point(windowCentreX, windowCentreY),
            radius: 40f,
            speed: 80f);
        _lineShape = new LineSegmentEmitterShape(
            start: new Point(windowCentreX - 150, 100),
            end: new Point(windowCentreX + 150, 100),
            velocity: new Point(0, 110));
        _rectShape = new RectEmitterShape(
            area: new Rect(windowCentreX - 120, windowCentreY - 60, 240, 120),
            velocity: new Point(0, -50));

        _particles = new ParticleSystem(_pointShape)
        {
            EmissionRate = 120f,
            Lifetime = 1.8f,
            StartSize = new Point(14f, 14f),
            Colour = new Colour(255, 200, 120),
            MaxParticles = 4000,
            SizeOverLife = (t, startSize) => startSize * (1f - 0.6f * t),
            ColourOverLife = (t, startColour) => new Colour(
                startColour.R,
                startColour.G,
                startColour.B,
                (byte)Math.Round(startColour.A * (1f - t))),
        };
        Add(_particles);

        BuildControlsPanel();
        UpdateShapeLabel("Point");
        RefreshEmissionRateLabel();
        RefreshLifetimeLabel();
    }

    public override string Name => "Particle Demo";

    public override void Update(double elapsed)
    {
        base.Update(elapsed);
        _countValue.Text = _particles.ParticleCount.ToString();
    }

    private void BuildControlsPanel()
    {
        Add(new TextGraphic(s_titleFont, "Particle System Demo", Colour.White, 95, 20, 685, 28)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        float y = PanelY;

        Add(new TextGraphic(s_bodyFont, "Emitter Shape:", Colour.White, PanelX, y, LabelW, ButtonH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        _shapeLabel = new TextGraphic(s_bodyFont, "", new Colour(220, 220, 255),
            PanelX + LabelW + 10, y, 140, ButtonH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_shapeLabel);
        y += ButtonH + RowGap;

        AddShapeButton(PanelX, y, "Point", () =>
        {
            _particles.EmitterShape = _pointShape;
            _particles.Colour = new Colour(255, 200, 120);
            UpdateShapeLabel("Point");
        });
        AddShapeButton(PanelX + ButtonW + 5, y, "Circle", () =>
        {
            _particles.EmitterShape = _circleShape;
            _particles.Colour = new Colour(120, 220, 255);
            UpdateShapeLabel("Circle");
        });
        y += ButtonH + RowGap;
        AddShapeButton(PanelX, y, "Line Segment", () =>
        {
            _particles.EmitterShape = _lineShape;
            _particles.Colour = new Colour(200, 230, 255);
            UpdateShapeLabel("Line Segment");
        });
        AddShapeButton(PanelX + ButtonW + 5, y, "Rect", () =>
        {
            _particles.EmitterShape = _rectShape;
            _particles.Colour = new Colour(200, 255, 180);
            UpdateShapeLabel("Rect");
        });
        y += ButtonH + SectionGap;

        Add(new TextGraphic(s_bodyFont, "Emission rate:", Colour.White, PanelX, y, LabelW, ButtonH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Button rateMinus = new(PanelX + LabelW + 10, y, ButtonH, ButtonH, ButtonTheme.FromColour(s_minusButton), "-");
        rateMinus.LeftClicked += (_, _) =>
        {
            _particles.EmissionRate = Math.Max(0f, _particles.EmissionRate - 20f);
            RefreshEmissionRateLabel();
        };
        Add(rateMinus);
        _emissionRateValue = new TextGraphic(s_bodyFont, "", Colour.White,
            PanelX + LabelW + 10 + ButtonH + 5, y, ValueW, ButtonH)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_emissionRateValue);
        Button ratePlus = new(PanelX + LabelW + 10 + ButtonH + 5 + ValueW + 5, y, ButtonH, ButtonH,
            ButtonTheme.FromColour(s_plusButton), "+");
        ratePlus.LeftClicked += (_, _) =>
        {
            _particles.EmissionRate = Math.Min(1000f, _particles.EmissionRate + 20f);
            RefreshEmissionRateLabel();
        };
        Add(ratePlus);
        y += ButtonH + RowGap;

        Add(new TextGraphic(s_bodyFont, "Lifetime (s):", Colour.White, PanelX, y, LabelW, ButtonH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        Button lifetimeMinus = new(PanelX + LabelW + 10, y, ButtonH, ButtonH, ButtonTheme.FromColour(s_minusButton), "-");
        lifetimeMinus.LeftClicked += (_, _) =>
        {
            _particles.Lifetime = Math.Max(0.1f, _particles.Lifetime - 0.25f);
            RefreshLifetimeLabel();
        };
        Add(lifetimeMinus);
        _lifetimeValue = new TextGraphic(s_bodyFont, "", Colour.White,
            PanelX + LabelW + 10 + ButtonH + 5, y, ValueW, ButtonH)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_lifetimeValue);
        Button lifetimePlus = new(PanelX + LabelW + 10 + ButtonH + 5 + ValueW + 5, y, ButtonH, ButtonH,
            ButtonTheme.FromColour(s_plusButton), "+");
        lifetimePlus.LeftClicked += (_, _) =>
        {
            _particles.Lifetime = Math.Min(10f, _particles.Lifetime + 0.25f);
            RefreshLifetimeLabel();
        };
        Add(lifetimePlus);
        y += ButtonH + SectionGap;

        _toggleButton = new Button(PanelX, y, 2 * ButtonW + 5, ButtonH,
            ButtonTheme.FromColour(s_accent), "Pause Emission");
        _toggleButton.LeftClicked += (_, _) =>
        {
            _particles.Emitting = !_particles.Emitting;
            _toggleButton.Text = _particles.Emitting ? "Pause Emission" : "Resume Emission";
        };
        Add(_toggleButton);
        y += ButtonH + RowGap;

        Button burstButton = new(PanelX, y, ButtonW, ButtonH, ButtonTheme.FromColour(s_accent), "Burst 200");
        burstButton.LeftClicked += (_, _) => _particles.EmitBurst(200);
        Add(burstButton);
        Button clearButton = new(PanelX + ButtonW + 5, y, ButtonW, ButtonH, ButtonTheme.FromColour(s_accent), "Clear");
        clearButton.LeftClicked += (_, _) => _particles.Clear();
        Add(clearButton);
        y += ButtonH + SectionGap;

        Add(new TextGraphic(s_bodyFont, "Live particles:", Colour.White, PanelX, y, LabelW, ButtonH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
        _countValue = new TextGraphic(s_bodyFont, "0", new Colour(220, 220, 255),
            PanelX + LabelW + 10, y, 100, ButtonH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_countValue);
    }

    private void AddShapeButton(float x, float y, string label, Action onClick)
    {
        Button button = new(x, y, ButtonW, ButtonH, ButtonTheme.FromColour(s_accent), label);
        button.LeftClicked += (_, _) => onClick();
        Add(button);
    }

    private void UpdateShapeLabel(string shape)
    {
        _shapeLabel.Text = shape;
    }

    private void RefreshEmissionRateLabel()
    {
        _emissionRateValue.Text = $"{_particles.EmissionRate:0}/s";
    }

    private void RefreshLifetimeLabel()
    {
        _lifetimeValue.Text = $"{_particles.Lifetime:0.00}";
    }
}

using System;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.PanelDemo;

internal class PanelDemoWorld : DemoWorld
{
    private const float MaxHeight = 280f;
    private const float MaxWidth = 280f;
    private const float MinHeight = 60f;
    private const float MinWidth = 60f;
    private const float SizeStep = 10f;
    private const float SourceSize = 150f;

    private static readonly FontDefinition s_font = new("Times New Roman", 13);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private readonly PanelGraphic _ninePatchPanel;
    private readonly TextureGraphic _stretchedPanel;
    private readonly TextGraphic _sizeLabel;
    private float _panelWidth = 220f;
    private float _panelHeight = 180f;

    public PanelDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 240, 240);

        Add(new TextGraphic(s_titleFont, "9-Slice Panel Demo", Colour.DimGray, 0f, 50f, Window.Width, 24f)
        {
            HAlignment = HAlignment.Centred,
        });

        ITexture texture = Textures.CreateFromFile("Assets/Graphics/ExamplePanelGraphic.png");

        // Column 1: source image at native size.
        const float sourceX = 40f;
        const float panelY = 120f;
        AddColumnLabel(sourceX, 95f, SourceSize, $"Source ({SourceSize:0}x{SourceSize:0})");
        Add(new TextureGraphic(texture, sourceX, panelY, SourceSize, SourceSize));

        // Column 2: PanelGraphic — corners pinned, edges and centre stretched.
        const float ninePatchX = 210f;
        AddColumnLabel(ninePatchX, 95f, MaxWidth, "9-slice (corners pinned)");
        _ninePatchPanel = new PanelGraphic(texture, borderSize: 50f, ninePatchX, panelY, _panelWidth, _panelHeight);
        Add(_ninePatchPanel);

        // Column 3: plain TextureGraphic at the same size for contrast.
        const float stretchX = 510f;
        AddColumnLabel(stretchX, 95f, MaxWidth, "Uniform stretch (for contrast)");
        _stretchedPanel = new TextureGraphic(texture, stretchX, panelY, _panelWidth, _panelHeight);
        Add(_stretchedPanel);

        // Size controls.
        const float controlsY = 470f;
        Add(new TextGraphic(s_font, "Width:", Colour.DimGray, 130f, controlsY, 80f, 28f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        Button widthMinus = new(220f, controlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(180, 90, 90)), "-");
        widthMinus.LeftClicked += (_, _) => AdjustWidth(-SizeStep);
        Add(widthMinus);

        Button widthPlus = new(262f, controlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(70, 150, 70)), "+");
        widthPlus.LeftClicked += (_, _) => AdjustWidth(+SizeStep);
        Add(widthPlus);

        Add(new TextGraphic(s_font, "Height:", Colour.DimGray, 330f, controlsY, 80f, 28f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
        });

        Button heightMinus = new(420f, controlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(180, 90, 90)), "-");
        heightMinus.LeftClicked += (_, _) => AdjustHeight(-SizeStep);
        Add(heightMinus);

        Button heightPlus = new(462f, controlsY, 36f, 28f, ButtonTheme.FromColour(new Colour(70, 150, 70)), "+");
        heightPlus.LeftClicked += (_, _) => AdjustHeight(+SizeStep);
        Add(heightPlus);

        _sizeLabel = new TextGraphic(s_font, FormatSize(), Colour.DimGray, 540f, controlsY, 160f, 28f)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_sizeLabel);
    }

    public override string Name => "Panel";

    private void AddColumnLabel(float x, float y, float width, string text)
    {
        Add(new TextGraphic(s_font, text, Colour.DimGray, x, y, width, 20f)
        {
            HAlignment = HAlignment.Left,
        });
    }

    private void AdjustHeight(float delta)
    {
        _panelHeight = Math.Clamp(_panelHeight + delta, MinHeight, MaxHeight);
        _ninePatchPanel.Height = _panelHeight;
        _stretchedPanel.Height = _panelHeight;
        _sizeLabel.Text = FormatSize();
    }

    private void AdjustWidth(float delta)
    {
        _panelWidth = Math.Clamp(_panelWidth + delta, MinWidth, MaxWidth);
        _ninePatchPanel.Width = _panelWidth;
        _stretchedPanel.Width = _panelWidth;
        _sizeLabel.Text = FormatSize();
    }

    private string FormatSize() => $"Size: {_panelWidth:0} x {_panelHeight:0}";
}

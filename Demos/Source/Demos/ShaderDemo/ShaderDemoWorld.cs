using System;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.ShaderDemo;

internal sealed class ShaderDemoWorld : DemoWorld
{
    private const float TitleY = 15f;
    private const float SelectorY = 55f;
    private const float SelectorH = 32f;
    private const float SelectorButtonW = 130f;
    private const float SelectorGap = 10f;
    private const float BearX = 225f;
    private const float BearY = 105f;
    private const float BearSize = 330f;
    private const float ControlsY = 460f;
    private const float DescriptionY = 510f;

    private static readonly FontDefinition s_titleFont = new("Times New Roman", 20);
    private static readonly FontDefinition s_labelFont = new("Times New Roman", 14);
    private static readonly FontDefinition s_buttonFont = new("Times New Roman", 14);
    private static readonly Colour s_uiText = new(40, 40, 40);
    private static readonly Colour s_effectButton = new(180, 200, 230);
    private static readonly Colour s_minusButton = new(180, 90, 90);
    private static readonly Colour s_plusButton = new(70, 150, 70);

    private readonly TextureGraphic _bear;

    // One instance of each effect's shader program, swapped onto the bear's Shader property
    // as the user clicks an effect button. Holding them as fields lets us mutate per-effect
    // uniforms (DarkenValue, BlurSize) without rebuilding the shader.
    private readonly StandardMatrixShaderProgram _defaultShader = Shaders.Standard;
    private readonly GreyscaleShaderProgram _greyscaleShader = Shaders.Greyscale;
    private readonly DarkenShaderProgram _darkenShader = Shaders.NewDarken(darkenValue: 0.5f);
    private readonly BlurShaderProgram _blurShader = Shaders.NewBlur(blurSize: 5f);

    private ShaderEffect _currentEffect = ShaderEffect.None;

    // Per-effect parameter row state. The minus/plus buttons stay in place; the labels show
    // only while the matching effect is active. Single shared row keeps the layout simple.
    private readonly TextGraphic _paramLabel;
    private readonly TextGraphic _paramValue;
    private readonly Button _paramMinus;
    private readonly Button _paramPlus;
    private readonly TextGraphic _description;

    public ShaderDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(245, 245, 250);

        Add(new TextGraphic(s_titleFont, "Shader Effect Demo", s_uiText, 0f, TitleY, Window.Width, 28f)
        {
            HAlignment = HAlignment.Centred,
        });

        _bear = new TextureGraphic(Textures.CreateFromFile("Assets/bear.png"), BearX, BearY, BearSize, BearSize);
        Add(_bear);

        // Effect selector row — four buttons evenly distributed and centred horizontally.
        float totalSelectorWidth = (SelectorButtonW * 4) + (SelectorGap * 3);
        float selectorStartX = (Window.Width - totalSelectorWidth) / 2f;
        (string label, ShaderEffect effect)[] entries =
        [
            ("None", ShaderEffect.None),
            ("Greyscale", ShaderEffect.Greyscale),
            ("Darken", ShaderEffect.Darken),
            ("Blur", ShaderEffect.Blur),
        ];
        for (int i = 0; i < entries.Length; i++)
        {
            ShaderEffect effect = entries[i].effect;
            float buttonX = selectorStartX + (i * (SelectorButtonW + SelectorGap));
            Button button = new(buttonX, SelectorY, SelectorButtonW, SelectorH, ButtonTheme.FromColour(s_effectButton), entries[i].label);
            button.LeftClicked += (_, _) => SetEffect(effect);
            Add(button);
        }

        // Parameter row (label + value + minus + plus). Initially hidden — shown when an
        // effect with adjustable params is selected.
        _paramLabel = new TextGraphic(s_labelFont, "", s_uiText, 250f, ControlsY, 200f, 28f)
        {
            HAlignment = HAlignment.Right,
            VAlignment = VAlignment.Centred,
            Visible = false,
        };
        Add(_paramLabel);

        _paramMinus = new Button(465f, ControlsY, 32f, 28f, ButtonTheme.FromColour(s_minusButton), "-") { Visible = false };
        _paramMinus.LeftClicked += (_, _) => AdjustCurrentParam(-1);
        Add(_paramMinus);

        _paramValue = new TextGraphic(s_labelFont, "", s_uiText, 505f, ControlsY, 50f, 28f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
            Visible = false,
        };
        Add(_paramValue);

        _paramPlus = new Button(563f, ControlsY, 32f, 28f, ButtonTheme.FromColour(s_plusButton), "+") { Visible = false };
        _paramPlus.LeftClicked += (_, _) => AdjustCurrentParam(+1);
        Add(_paramPlus);

        _description = new TextGraphic(s_buttonFont, "", new Colour(110, 110, 110), 0f, DescriptionY, Window.Width, 24f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
        Add(_description);

        SetEffect(ShaderEffect.None);
    }

    public override string Name => "Shader";

    private void SetEffect(ShaderEffect effect)
    {
        _currentEffect = effect;
        _bear.Shader = effect switch
        {
            ShaderEffect.Greyscale => _greyscaleShader,
            ShaderEffect.Darken => _darkenShader,
            ShaderEffect.Blur => _blurShader,
            _ => _defaultShader,
        };

        switch (effect)
        {
            case ShaderEffect.None:
                ShowParameter(visible: false);
                _description.Text = "[None] default.frag - straight texture passthrough with alpha discard";
                break;
            case ShaderEffect.Greyscale:
                ShowParameter(visible: false);
                _description.Text = "[Greyscale] greyscale.frag - luminance via dot(rgb, (0.299, 0.587, 0.114))";
                break;
            case ShaderEffect.Darken:
                ShowParameter(visible: true, label: "Darken:");
                RefreshParameterValue();
                _description.Text = "[Darken] darken.frag - multiplies rgb by DarkenValue, leaves alpha";
                break;
            case ShaderEffect.Blur:
                ShowParameter(visible: true, label: "Blur radius:");
                RefreshParameterValue();
                _description.Text = "[Blur] blur.frag - 9x9 box blur, BlurSize controls kernel spread";
                break;
        }
    }

    private void ShowParameter(bool visible, string label = "")
    {
        _paramLabel.Visible = visible;
        _paramValue.Visible = visible;
        _paramMinus.Visible = visible;
        _paramPlus.Visible = visible;
        if (visible)
        {
            _paramLabel.Text = label;
        }
    }

    private void AdjustCurrentParam(int delta)
    {
        switch (_currentEffect)
        {
            case ShaderEffect.Darken:
                _darkenShader.DarkenValue = Math.Clamp(_darkenShader.DarkenValue + (delta * 0.1f), 0f, 1f);
                break;
            case ShaderEffect.Blur:
                _blurShader.BlurSize = Math.Clamp(_blurShader.BlurSize + delta, 0f, 20f);
                break;
        }
        RefreshParameterValue();
    }

    private void RefreshParameterValue()
    {
        _paramValue.Text = _currentEffect switch
        {
            ShaderEffect.Darken => _darkenShader.DarkenValue.ToString("0.0"),
            ShaderEffect.Blur => ((int)_blurShader.BlurSize).ToString(),
            _ => "",
        };
    }
}

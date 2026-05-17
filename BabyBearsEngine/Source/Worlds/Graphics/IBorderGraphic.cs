namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>A bordered rectangle renderable that can be added to a container.</summary>
internal interface IBorderGraphic : IRenderable
{
    /// <summary>Colour of the border.</summary>
    Colour BorderColour { get; set; }

    /// <summary>Controls whether the border draws inside, outside, or centred on the stated bounds.</summary>
    BorderPosition BorderPosition { get; set; }

    /// <summary>Border width in pixels on each side.</summary>
    float BorderThickness { get; set; }

    /// <summary>Colour of the interior fill.</summary>
    Colour FillColour { get; set; }
}

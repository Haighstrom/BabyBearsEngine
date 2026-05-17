namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>A border-frame renderable that can be added to a container.</summary>
internal interface IBorderGraphic : IRenderable
{
    /// <summary>Colour of the border.</summary>
    Colour BorderColour { get; set; }

    /// <summary>Controls whether the border draws inside, outside, or centred on the stated bounds.</summary>
    BorderPosition BorderPosition { get; set; }

    /// <summary>Border width in pixels on each side.</summary>
    float BorderThickness { get; set; }
}

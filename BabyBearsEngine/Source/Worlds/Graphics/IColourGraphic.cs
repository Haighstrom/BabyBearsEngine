namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>A solid-colour renderable rectangle that can be added to a container.</summary>
internal interface IColourGraphic : IRenderable
{
    /// <summary>Fill colour.</summary>
    Colour Colour { get; set; }
}

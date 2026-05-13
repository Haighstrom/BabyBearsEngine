namespace BabyBearsEngine.Graphics;

/// <summary>
/// A renderable that can be tinted via a <see cref="Colour"/>. All graphics in the engine
/// should expose this — for solid-colour graphics the colour is the fill, for textured
/// graphics it multiplies the texture sample (so <see cref="Colour.White"/> means "no tint"),
/// and UI widgets mutate it to apply hover / pressed / disabled state.
/// </summary>
public interface IGraphic : IRenderable
{
    /// <summary>
    /// Tint colour. For solid-colour graphics this is the fill colour; for textured graphics
    /// it multiplies the texture sample (so <see cref="Colour.White"/> means "no tint").
    /// </summary>
    Colour Colour { get; set; }
}

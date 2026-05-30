namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A renderable, layered, rectangular thing that can be tinted via a <see cref="Colour"/>.
/// All graphics in the engine implement this — geometry (X, Y, Width, Height) comes from
/// <see cref="IRectAddable"/>; rendering and visibility from <see cref="IRenderable"/>;
/// layer ordering from <see cref="ILayered"/>; and <see cref="Colour"/> is the per-graphic
/// tint (fill for solid graphics, texture multiplier for textured ones, with
/// <see cref="Colour.White"/> meaning "no tint").
/// </summary>
public interface IGraphic : IRenderable, ILayered, IRectAddable
{
    /// <summary>
    /// Tint colour. For solid-colour graphics this is the fill colour; for textured graphics
    /// it multiplies the texture sample (so <see cref="Colour.White"/> means "no tint").
    /// </summary>
    Colour Colour { get; set; }

    /// <summary>
    /// Convenience accessor for the alpha component of <see cref="Colour"/>, expressed as a
    /// normalised float (0–1) on set; raw byte (0–255) on get.
    /// </summary>
    float Alpha
    {
        get => Colour.A;
        set => Colour = new(Colour.R, Colour.G, Colour.B, (byte)Math.Round(value * 255f));
    }

    /// <summary>
    /// Rotation angle in degrees, applied around the graphic's centre during rendering.
    /// Defaults to 0 (no rotation). Positive values rotate clockwise.
    /// </summary>
    float Angle { get; set; }
}

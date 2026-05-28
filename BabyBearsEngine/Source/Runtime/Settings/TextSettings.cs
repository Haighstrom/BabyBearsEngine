using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine;

/// <summary>
/// Configuration for the text subsystem — currently just the choice of atlas backend.
/// </summary>
public record class TextSettings()
{
    /// <summary>The default text settings — GDI+ bitmap rasterisation.</summary>
    public static TextSettings Default => new();

    /// <summary>
    /// The text rendering backend used to build font atlases. Defaults to
    /// <see cref="TextRenderer.Gdi"/>. Resolved at startup; cannot be changed
    /// after the engine has begun rendering.
    /// </summary>
    public TextRenderer Renderer { get; set; } = TextRenderer.Gdi;
}

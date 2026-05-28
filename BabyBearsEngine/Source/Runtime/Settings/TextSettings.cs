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
    /// The default text rendering backend used to build font atlases for fonts that don't pin one
    /// themselves. Defaults to <see cref="TextRenderer.Gdi"/>. Individual fonts can override it per
    /// atlas via <see cref="FontDefinition.Renderer"/> — e.g. crisp GDI+ text for fixed-size UI and
    /// scalable SDF text for the world. Resolved at startup; the default cannot be changed after the
    /// engine has begun rendering.
    /// </summary>
    public TextRenderer Renderer { get; set; } = TextRenderer.Gdi;
}

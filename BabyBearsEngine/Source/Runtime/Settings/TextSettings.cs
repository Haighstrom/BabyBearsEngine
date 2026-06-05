using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine;

/// <summary>
/// Configuration for the text subsystem — currently just the choice of atlas backend.
/// </summary>
public record class TextSettings()
{
    /// <summary>The default text settings — FreeType rasterisation (cross-platform, sharp at native size).</summary>
    public static TextSettings Default => new();

    /// <summary>
    /// The default text rendering backend used to build font atlases for fonts that don't pin one
    /// themselves. Defaults to <see cref="TextRenderer.FreeType"/> — pixel-accurate hinted glyphs
    /// that look sharp at the font's chosen size, the right pick for most UI text. Switch to
    /// <see cref="TextRenderer.Sdf"/> when you need text that scales smoothly (signed-distance-field
    /// glyphs stay crisp at any scale but read softer at small sizes). <see cref="TextRenderer.Gdi"/>
    /// is Windows-only and throws <see cref="PlatformNotSupportedException"/> when used elsewhere.
    /// Individual fonts can override the choice per atlas via <see cref="FontDefinition.Renderer"/>
    /// — e.g. SDF for a zoomable world map and FreeType for static UI. Resolved at startup; the
    /// default cannot be changed after the engine has begun rendering.
    /// </summary>
    public TextRenderer Renderer { get; set; } = TextRenderer.FreeType;
}

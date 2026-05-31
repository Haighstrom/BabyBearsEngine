using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine;

/// <summary>
/// Configuration for the text subsystem — currently just the choice of atlas backend.
/// </summary>
public record class TextSettings()
{
    /// <summary>The default text settings — SDF rasterisation (cross-platform).</summary>
    public static TextSettings Default => new();

    /// <summary>
    /// The default text rendering backend used to build font atlases for fonts that don't pin one
    /// themselves. Defaults to <see cref="TextRenderer.Sdf"/> — the SDF and FreeType backends are
    /// cross-platform; GDI+ is Windows-only and throws <see cref="PlatformNotSupportedException"/>
    /// when used elsewhere. Individual fonts can override the choice per atlas via
    /// <see cref="FontDefinition.Renderer"/> — e.g. crisp GDI+ text for fixed-size UI on Windows
    /// and scalable SDF text for the world. Resolved at startup; the default cannot be changed
    /// after the engine has begun rendering.
    /// </summary>
    public TextRenderer Renderer { get; set; } = TextRenderer.Sdf;
}

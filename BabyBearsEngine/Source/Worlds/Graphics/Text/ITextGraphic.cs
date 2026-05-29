using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public interface ITextGraphic : IRenderable, IAddable
{
    /// <summary>Rotation angle in degrees, applied around the graphic's centre.</summary>
    float Angle { get; set; }

    /// <summary>The colour used to tint rendered glyphs.</summary>
    Colour Colour { get; set; }

    /// <summary>Additional horizontal space added after each non-space character, in local units.</summary>
    float ExtraCharacterSpacing { get; set; }

    /// <summary>Additional vertical space inserted between lines when <see cref="Multiline"/> is true.</summary>
    float ExtraLineSpacing { get; set; }

    /// <summary>Additional width added to space characters beyond their natural glyph width.</summary>
    float ExtraSpaceWidth { get; set; }

    /// <summary>Zero-based index of the first character to render; earlier characters are skipped.</summary>
    int FirstCharToDraw { get; set; }

    /// <summary>The font definition used to render <see cref="Text"/>.</summary>
    FontDefinition Font { get; set; }

    /// <summary>
    /// Optional font used to render spans marked with <c>&lt;b&gt;...&lt;/b&gt;</c>. When <see cref="Font"/>
    /// is assigned, a companion <c>{FontName}_b.ttf</c> in <c>Assets/Fonts/</c> (if present) is picked up
    /// automatically; assign this property to override. When null and a <c>&lt;b&gt;</c> span is rendered,
    /// the base font is used and a warning is logged.
    /// </summary>
    FontDefinition? BoldFont { get; set; }

    /// <summary>
    /// Optional font used to render spans marked with <c>&lt;i&gt;...&lt;/i&gt;</c>. When <see cref="Font"/>
    /// is assigned, a companion <c>{FontName}_i.ttf</c> in <c>Assets/Fonts/</c> (if present) is picked up
    /// automatically; assign this property to override. When null and an <c>&lt;i&gt;</c> span is rendered,
    /// the base font is used and a warning is logged.
    /// </summary>
    FontDefinition? ItalicFont { get; set; }

    /// <summary>
    /// Optional font used to render spans marked with both <c>&lt;b&gt;</c> and <c>&lt;i&gt;</c> at the same
    /// time. When <see cref="Font"/> is assigned, a companion <c>{FontName}_bi.ttf</c> in
    /// <c>Assets/Fonts/</c> (if present) is picked up automatically; assign this property to override.
    /// When null and a bold-italic span is rendered, the renderer falls back to <see cref="BoldFont"/>,
    /// then <see cref="ItalicFont"/>, then the base font, logging a warning the first time.
    /// </summary>
    FontDefinition? BoldItalicFont { get; set; }

    /// <summary>When true, text wraps to multiple lines within the graphic's width.</summary>
    bool Multiline { get; set; }

    /// <summary>Maximum number of characters to render starting from <see cref="FirstCharToDraw"/>.</summary>
    int NumCharsToDraw { get; set; }

    /// <summary>Horizontal scale factor applied to all glyph geometry before rendering.</summary>
    float ScaleX { get; set; }

    /// <summary>Vertical scale factor applied to all glyph geometry before rendering.</summary>
    float ScaleY { get; set; }

    /// <summary>Optional strikethrough decoration drawn across rendered characters, or <see langword="null"/> for none.</summary>
    TextDecoration? Strikethrough { get; set; }

    /// <summary>The string to render.</summary>
    string Text { get; set; }

    /// <summary>Optional underline decoration drawn beneath rendered characters, or <see langword="null"/> for none.</summary>
    TextDecoration? Underline { get; set; }

    /// <summary>When true, inline markup tags (e.g. <c>&lt;colour=#FF0000&gt;</c>, <c>&lt;u&gt;</c>, <c>&lt;s&gt;</c>, <c>&lt;b&gt;</c>, <c>&lt;i&gt;</c>) are parsed and applied during rendering. When false, tag characters are rendered literally.</summary>
    bool UseInlineTags { get; set; }

    /// <summary>Returns the rendered size of <paramref name="text"/> in pixels at the current scale.</summary>
    Point MeasureString(string text);

    /// <summary>Returns the rendered size of <see cref="Text"/> in pixels, respecting <see cref="Multiline"/> layout.</summary>
    Point MeasureString();

    /// <summary>Sets <see cref="ScaleX"/> and <see cref="ScaleY"/> so that one font pixel maps to one screen pixel inside <paramref name="camera"/>.</summary>
    void ScaleForCamera(ICamera camera);

    /// <summary>Sets <see cref="ScaleX"/> and <see cref="ScaleY"/> so that one font pixel maps to one screen pixel for <paramref name="view"/>.</summary>
    void ScaleForCamera(ICameraView view);
}

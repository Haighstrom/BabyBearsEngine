using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Cameras;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Tests.Unit;

/// <summary>
/// Shared fake <see cref="ITextGraphic"/> for UI-control tests. Lets a control's internal (test)
/// constructor be wired with a working text graphic so text assignments actually take effect,
/// rather than the control leaving the field null and silently no-opping the setter.
/// </summary>
internal sealed class StubTextGraphic : AddableBase, ITextGraphic
{
    public float X { get; set; } = 0f;
    public float Y { get; set; } = 0f;
    public float Width { get; set; } = 0f;
    public float Height { get; set; } = 0f;
    public float Angle { get; set; } = 0f;
    public Colour Colour { get; set; } = Colour.Black;
    public float ExtraCharacterSpacing { get; set; } = 0f;
    public float ExtraLineSpacing { get; set; } = 0f;
    public float ExtraSpaceWidth { get; set; } = 0f;
    public int FirstCharToDraw { get; set; } = 0;
    public FontDefinition Font { get; set; } = new("Arial", 12);
    public FontDefinition? BoldFont { get; set; } = null;
    public FontDefinition? ItalicFont { get; set; } = null;
    public FontDefinition? BoldItalicFont { get; set; } = null;
    public bool Multiline { get; set; } = false;
    public int NumCharsToDraw { get; set; } = int.MaxValue;
    public float ScaleX { get; set; } = 1f;
    public float ScaleY { get; set; } = 1f;
    public TextDecoration? Strikethrough { get; set; } = null;
    public string Text { get; set; } = "";
    public TextDecoration? Underline { get; set; } = null;
    public bool UseInlineTags { get; set; } = false;
    public bool Visible { get; set; } = true;

    public Point MeasureString(string text) => new(0f, 0f);
    public Point MeasureString() => new(0f, 0f);
    public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    public void ScaleForCamera(ICamera camera) { }
    public void ScaleForCamera(ICameraView view) { }
}

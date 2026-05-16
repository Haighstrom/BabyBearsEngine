namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Describes a text decoration line (underline or strikethrough): its colour and pixel thickness.
/// Set a property to <c>null</c> to disable the decoration.
/// </summary>
/// <param name="Colour">Line colour.</param>
/// <param name="Thickness">Line thickness in pixels. Defaults to 1.</param>
public readonly record struct TextDecoration(Colour Colour, float Thickness = 1f);

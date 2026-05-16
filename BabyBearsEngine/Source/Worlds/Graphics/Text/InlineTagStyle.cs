namespace BabyBearsEngine.Worlds.Graphics.Text;

internal readonly record struct InlineTagStyle(Colour? ColourOverride, bool Underline, bool Strikethrough)
{
    internal static readonly InlineTagStyle Default = new(null, false, false);
}

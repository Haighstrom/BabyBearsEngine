using System.Text;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Identifies a font atlas to build: family, size, style, the characters to rasterise, and
/// optionally which rendering backend to use. Serves as the <see cref="FontTextureCache"/> key,
/// so two definitions that differ only by <see cref="Renderer"/> are cached independently.
/// A null <see cref="Renderer"/> falls back to the engine-wide default backend
/// (see <see cref="TextSettings.Renderer"/>).
/// </summary>
public record class FontDefinition(
    string FontName,
    float FontSize,
    FontStyle FontStyle = FontStyle.Regular,
    string ExtraCharactersToLoad = "",
    bool IncludeDefaultCharacters = true,
    TextRenderer? Renderer = null)
{
    private const string DefaultCharsToLoad = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 !£$%^&*()-=_+[]{};'#:@~,./<>?|`€—–…•←→↑↓‘’“”\"\\";

    private static string BuildCharactersToLoad(bool includeDefaultCharacters, string extraCharactersToLoad)
    {
        extraCharactersToLoad ??= string.Empty;

        string combinedCharactersToLoad = includeDefaultCharacters 
            ? DefaultCharsToLoad + extraCharactersToLoad 
            : extraCharactersToLoad;

        return RemoveDuplicateCharacters(combinedCharactersToLoad);
    }

    private static string RemoveDuplicateCharacters(string charactersToLoad)
    {
        if (string.IsNullOrEmpty(charactersToLoad))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(charactersToLoad.Length);
        HashSet<char> seen = [];

        foreach (char c in charactersToLoad)
        {
            // .Add() returns false if the item already exists
            if (seen.Add(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public string CharactersToLoad => BuildCharactersToLoad(IncludeDefaultCharacters, ExtraCharactersToLoad);
}

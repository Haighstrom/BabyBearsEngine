using System.Collections.Generic;
using System.Text;

namespace BabyBearsEngine.Source.Graphics.Text;

public record class FontDefinition(string FontName, float FontSize, FontStyle FontStyle, bool AntiAliased, string ExtraCharactersToLoad = "", bool IncludeDefaultCharacters = true)
{
    private const string DEFAULT_CHARS_TO_LOAD = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 !£$%^&*()-=_+[]{};'#:@~,./<>?|`¬¦€\"\\";

    private static string BuildCharactersToLoad(bool includeDefaultCharacters, string extraCharactersToLoad)
    {
        extraCharactersToLoad ??= string.Empty;

        string combinedCharactersToLoad = includeDefaultCharacters 
            ? DEFAULT_CHARS_TO_LOAD + extraCharactersToLoad 
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

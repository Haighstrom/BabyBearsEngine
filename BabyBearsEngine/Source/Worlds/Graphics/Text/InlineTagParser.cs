using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

internal static class InlineTagParser
{
    private readonly record struct StyleOverride(string TagName, Colour? ColourValue);

    public static StyledChar[] Parse(string raw, bool useInlineTags)
    {
        if (!useInlineTags)
        {
            return ToDefaultStyle(raw);
        }

        if (!raw.Contains('<') && !raw.Contains('\\'))
        {
            return ToDefaultStyle(raw);
        }

        List<StyledChar> result = [];
        List<StyleOverride> activeOverrides = [];
        InlineTagStyle currentStyle = InlineTagStyle.Default;

        int i = 0;
        while (i < raw.Length)
        {
            if (raw[i] == '\\' && i + 1 < raw.Length && raw[i + 1] == '<')
            {
                result.Add(new StyledChar('<', currentStyle));
                i += 2;
                continue;
            }

            if (raw[i] == '<')
            {
                int closeAngle = raw.IndexOf('>', i + 1);

                if (closeAngle < 0)
                {
                    result.Add(new StyledChar('<', currentStyle));
                    i++;
                    continue;
                }

                string tagContent = raw[(i + 1)..closeAngle].Trim();

                if (tagContent.Length > 0 && tagContent[0] == '/')
                {
                    ProcessCloseTag(tagContent[1..].Trim().ToLowerInvariant(), activeOverrides, ref currentStyle);
                }
                else
                {
                    ProcessOpenTag(tagContent, activeOverrides, ref currentStyle);
                }

                i = closeAngle + 1;
                continue;
            }

            result.Add(new StyledChar(raw[i], currentStyle));
            i++;
        }

        return [..result];
    }

    private static void ProcessOpenTag(string tagContent, List<StyleOverride> activeOverrides, ref InlineTagStyle currentStyle)
    {
        string lower = tagContent.ToLowerInvariant();

        if (lower == "u")
        {
            activeOverrides.Add(new StyleOverride("u", null));
            currentStyle = ComputeStyle(activeOverrides);
        }
        else if (lower == "s")
        {
            activeOverrides.Add(new StyleOverride("s", null));
            currentStyle = ComputeStyle(activeOverrides);
        }
        else if (lower.StartsWith("colour="))
        {
            string colourStr = tagContent[7..].Trim();

            if (TryParseColour(colourStr, out Colour colour))
            {
                activeOverrides.Add(new StyleOverride("colour", colour));
                currentStyle = ComputeStyle(activeOverrides);
            }
            else
            {
                Logger.Warning($"InlineTagParser: could not parse colour value '{colourStr}' — tag ignored.");
            }
        }
        else
        {
            Logger.Warning($"InlineTagParser: unknown tag '<{tagContent}>' — stripped.");
        }
    }

    private static void ProcessCloseTag(string closeName, List<StyleOverride> activeOverrides, ref InlineTagStyle currentStyle)
    {
        if (closeName == "")
        {
            // </> — close the last opened tag
            if (activeOverrides.Count > 0)
            {
                activeOverrides.RemoveAt(activeOverrides.Count - 1);
                currentStyle = ComputeStyle(activeOverrides);
            }
            else
            {
                Logger.Warning("InlineTagParser: '</>' found but no open tag to close.");
            }
        }
        else if (closeName is "u" or "s" or "colour")
        {
            int lastIdx = FindLastIndex(activeOverrides, closeName);

            if (lastIdx >= 0)
            {
                activeOverrides.RemoveAt(lastIdx);
                currentStyle = ComputeStyle(activeOverrides);
            }
            else
            {
                Logger.Warning($"InlineTagParser: '</{closeName}>' found but no matching open tag.");
            }
        }
        else
        {
            Logger.Warning($"InlineTagParser: unknown close tag '</{closeName}>' — stripped.");
        }
    }

    private static int FindLastIndex(List<StyleOverride> overrides, string tagName)
    {
        for (int i = overrides.Count - 1; i >= 0; i--)
        {
            if (overrides[i].TagName == tagName)
            {
                return i;
            }
        }

        return -1;
    }

    private static InlineTagStyle ComputeStyle(List<StyleOverride> overrides)
    {
        Colour? colour = null;
        bool underline = false;
        bool strikethrough = false;

        foreach (StyleOverride o in overrides)
        {
            switch (o.TagName)
            {
                case "colour":
                    colour = o.ColourValue;
                    break;
                case "u":
                    underline = true;
                    break;
                case "s":
                    strikethrough = true;
                    break;
            }
        }

        return new InlineTagStyle(colour, underline, strikethrough);
    }

    private static bool TryParseColour(string value, out Colour colour)
    {
        if (value.StartsWith('#'))
        {
            string hex = value[1..];

            if (hex.Length == 6
                && TryParseHexByte(hex[0..2], out byte r)
                && TryParseHexByte(hex[2..4], out byte g)
                && TryParseHexByte(hex[4..6], out byte b))
            {
                colour = new Colour(r, g, b);
                return true;
            }

            if (hex.Length == 8
                && TryParseHexByte(hex[0..2], out r)
                && TryParseHexByte(hex[2..4], out g)
                && TryParseHexByte(hex[4..6], out b)
                && TryParseHexByte(hex[6..8], out byte a))
            {
                colour = new Colour(r, g, b, a);
                return true;
            }
        }

        colour = default;
        return false;
    }

    private static bool TryParseHexByte(string hex2, out byte value)
    {
        try
        {
            value = Convert.ToByte(hex2, 16);
            return true;
        }
        catch
        {
            value = 0;
            return false;
        }
    }

    private static StyledChar[] ToDefaultStyle(string s)
    {
        var result = new StyledChar[s.Length];

        for (int i = 0; i < s.Length; i++)
        {
            result[i] = new StyledChar(s[i], InlineTagStyle.Default);
        }

        return result;
    }
}

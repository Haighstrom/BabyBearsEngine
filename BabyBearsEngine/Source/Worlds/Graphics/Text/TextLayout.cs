namespace BabyBearsEngine.Worlds.Graphics.Text;

internal static class TextLayout
{
    public static IReadOnlyList<LineInfo> ComputeLines(
        StyledChar[] chars,
        GeneratedFontStruct fontStruct,
        float maxWidth,
        float scaleX,
        float extraSpaceWidth,
        float extraCharSpacing)
    {
        List<LineInfo> lines = [];
        int segStart = 0;

        while (true)
        {
            int newlineIdx = FindNewline(chars, segStart);
            int segEnd = newlineIdx >= 0 ? newlineIdx : chars.Length;

            WrapSegment(chars, segStart, segEnd, fontStruct, maxWidth, scaleX, extraSpaceWidth, extraCharSpacing, lines);

            if (newlineIdx < 0)
            {
                break;
            }

            segStart = newlineIdx + 1;
        }

        return lines;
    }

    public static float MeasureLine(
        StyledChar[] chars,
        GeneratedFontStruct fontStruct,
        float scaleX,
        float extraSpaceWidth,
        float extraCharSpacing)
    {
        float width = 0f;

        foreach (StyledChar sc in chars)
        {
            char c = sc.Char;
            width += fontStruct.GetCharPosition(c).Size.X * scaleX;
            width += c == ' ' ? extraSpaceWidth : extraCharSpacing;
        }

        return width;
    }

    public static float MeasureLine(
        string line,
        GeneratedFontStruct fontStruct,
        float scaleX,
        float extraSpaceWidth,
        float extraCharSpacing)
    {
        float width = 0f;

        foreach (char c in line)
        {
            width += fontStruct.GetCharPosition(c).Size.X * scaleX;
            width += c == ' ' ? extraSpaceWidth : extraCharSpacing;
        }

        return width;
    }

    private static int FindNewline(StyledChar[] chars, int from)
    {
        for (int i = from; i < chars.Length; i++)
        {
            if (chars[i].Char == '\n')
            {
                return i;
            }
        }

        return -1;
    }

    private static float CharWidth(
        char c,
        GeneratedFontStruct fontStruct,
        float scaleX,
        float extraSpaceWidth,
        float extraCharSpacing)
        => fontStruct.GetCharPosition(c).Size.X * scaleX + (c == ' ' ? extraSpaceWidth : extraCharSpacing);

    private static void WrapSegment(
        StyledChar[] chars,
        int segStart,
        int segEnd,
        GeneratedFontStruct fontStruct,
        float maxWidth,
        float scaleX,
        float extraSpaceWidth,
        float extraCharSpacing,
        List<LineInfo> output)
    {
        if (segStart == segEnd)
        {
            output.Add(new LineInfo([], segStart, segEnd));
            return;
        }

        int lineStart = segStart;

        while (lineStart < segEnd)
        {
            float lineWidth = 0f;
            int lastSpaceIdx = -1;
            int i = lineStart;

            while (i < segEnd)
            {
                char c = chars[i].Char;
                float cw = CharWidth(c, fontStruct, scaleX, extraSpaceWidth, extraCharSpacing);

                if (lineWidth + cw > maxWidth && i > lineStart)
                {
                    break;
                }

                lineWidth += cw;

                if (c == ' ')
                {
                    lastSpaceIdx = i;
                }

                i++;
            }

            if (i == segEnd)
            {
                output.Add(new LineInfo(chars[lineStart..segEnd], lineStart, segEnd));
                break;
            }

            int breakAt;
            int nextLineStart;

            if (lastSpaceIdx >= 0)
            {
                breakAt = lastSpaceIdx;
                nextLineStart = lastSpaceIdx + 1;
            }
            else if (chars[i].Char == ' ')
            {
                breakAt = i;
                nextLineStart = i + 1;
            }
            else
            {
                breakAt = i;
                nextLineStart = i;
            }

            output.Add(new LineInfo(chars[lineStart..breakAt], lineStart, breakAt));
            lineStart = nextLineStart;
        }
    }
}

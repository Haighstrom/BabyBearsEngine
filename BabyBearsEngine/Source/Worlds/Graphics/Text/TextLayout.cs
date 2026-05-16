using System.Collections.Generic;

namespace BabyBearsEngine.Worlds.Graphics.Text;

internal static class TextLayout
{
    public static IReadOnlyList<LineInfo> ComputeLines(
        string text,
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
            int newlineIdx = text.IndexOf('\n', segStart);
            int segEnd = newlineIdx >= 0 ? newlineIdx : text.Length;

            WrapSegment(text, segStart, segEnd, fontStruct, maxWidth, scaleX, extraSpaceWidth, extraCharSpacing, lines);

            if (newlineIdx < 0)
            {
                break;
            }

            segStart = newlineIdx + 1;
        }

        return lines;
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
            width += fontStruct.CharPositions[c].Size.X * scaleX;
            width += c == ' ' ? extraSpaceWidth : extraCharSpacing;
        }

        return width;
    }

    private static float CharWidth(
        char c,
        GeneratedFontStruct fontStruct,
        float scaleX,
        float extraSpaceWidth,
        float extraCharSpacing)
        => fontStruct.CharPositions[c].Size.X * scaleX + (c == ' ' ? extraSpaceWidth : extraCharSpacing);

    private static void WrapSegment(
        string text,
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
            output.Add(new LineInfo("", segStart, segEnd));
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
                float cw = CharWidth(text[i], fontStruct, scaleX, extraSpaceWidth, extraCharSpacing);

                if (lineWidth + cw > maxWidth && i > lineStart)
                {
                    break;
                }

                lineWidth += cw;

                if (text[i] == ' ')
                {
                    lastSpaceIdx = i;
                }

                i++;
            }

            if (i == segEnd)
            {
                output.Add(new LineInfo(text[lineStart..segEnd], lineStart, segEnd));
                break;
            }

            int breakAt;
            int nextLineStart;

            if (lastSpaceIdx >= 0)
            {
                breakAt = lastSpaceIdx;
                nextLineStart = lastSpaceIdx + 1;
            }
            else if (text[i] == ' ')
            {
                // The overflowing character is a space — consume it as the break
                breakAt = i;
                nextLineStart = i + 1;
            }
            else
            {
                // Character wrap — no space found before overflow
                breakAt = i;
                nextLineStart = i;
            }

            output.Add(new LineInfo(text[lineStart..breakAt], lineStart, breakAt));
            lineStart = nextLineStart;
        }
    }
}

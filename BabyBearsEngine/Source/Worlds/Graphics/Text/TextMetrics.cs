using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Standalone text-measurement entry point: computes the rendered size of word-wrapped, multi-line
/// text exactly as <see cref="TextGraphic.MeasureString()"/> would, without needing a GL context or a
/// constructed <see cref="TextGraphic"/>. Useful for tooling (e.g. checking a UI text box's content
/// against its fixed pixel budget before it ever runs in-game).
/// </summary>
public static class TextMetrics
{
    /// <summary>
    /// Rasterises <paramref name="font"/>'s metrics directly via <see cref="FreeTypeFontAtlasGenerator"/>
    /// (the same backend <see cref="TextGraphic"/> uses at runtime), word-wraps <paramref name="text"/>
    /// to <paramref name="maxWidth"/> pixels at native (1:1) scale, and returns the widest wrapped line
    /// and the total block height — including <paramref name="extraParagraphLineSpacing"/> applied the
    /// same way <see cref="TextGraphic.ExtraParagraphLineSpacing"/> is (once per manual paragraph break,
    /// skipping the second+ break of a consecutive run — see <see cref="TextLayout.ShouldApplyParagraphSpacing"/>).
    /// </summary>
    /// <param name="font">The font to measure in. Only its family/size/style/character set matter here — no texture is ever created.</param>
    /// <param name="text">The text to measure, exactly as it would be passed to a <see cref="TextGraphic"/>'s <c>Text</c>.</param>
    /// <param name="maxWidth">The box width, in pixels, text wraps against.</param>
    /// <param name="extraParagraphLineSpacing">Extra vertical gap added before each wrapped paragraph break, matching <see cref="TextGraphic.ExtraParagraphLineSpacing"/>. Defaults to 0.</param>
    /// <param name="useInlineTags">Whether <c>&lt;colour=#RRGGBB&gt;</c>/<c>&lt;b&gt;</c>/etc. tags are parsed out of <paramref name="text"/> rather than measured as literal characters, matching <see cref="TextGraphic.UseInlineTags"/>. Defaults to true.</param>
    /// <returns>X: the widest wrapped line's width in pixels. Y: the total block height in pixels.</returns>
    public static Point MeasureWrappedText(FontDefinition font, string text, float maxWidth, float extraParagraphLineSpacing = 0f, bool useInlineTags = true)
    {
        FontAtlasMetrics metrics = new FreeTypeFontAtlasGenerator().RasteriseAtlas(font).Metrics;

        StyledChar[] chars = InlineTagParser.Parse(text, useInlineTags);
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(chars, metrics, maxWidth, scaleX: 1f, extraSpaceWidth: 0f, extraCharSpacing: 0f);

        float maxLineWidth = 0f;
        int paragraphBreakCount = 0;

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            float lineWidth = TextLayout.MeasureLine(lines[lineIndex].Chars, metrics, scaleX: 1f, extraSpaceWidth: 0f, extraCharSpacing: 0f);

            if (lineWidth > maxLineWidth)
            {
                maxLineWidth = lineWidth;
            }

            if (TextLayout.ShouldApplyParagraphSpacing(lines, lineIndex, suppressOnConsecutiveBreaks: true))
            {
                paragraphBreakCount++;
            }
        }

        float totalHeight = lines.Count * metrics.HighestChar + paragraphBreakCount * extraParagraphLineSpacing;

        return new Point(maxLineWidth, totalHeight);
    }
}

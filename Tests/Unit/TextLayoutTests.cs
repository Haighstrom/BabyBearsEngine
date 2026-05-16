using System.Collections.Generic;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TextLayoutTests
{
    private const float Delta = 1e-4f;

    private static StyledChar[] Chars(string s) => InlineTagParser.Parse(s, false);

    private static GeneratedFontStruct MakeFontStruct(string charsNeeded, int charWidth = 10, int charHeight = 20)
    {
        Dictionary<char, Box2i> positions = [];
        Dictionary<char, Box2> normPositions = [];

        foreach (char c in charsNeeded)
        {
            if (!positions.ContainsKey(c))
            {
                positions[c] = new Box2i(0, 0, charWidth, charHeight);
                normPositions[c] = new Box2(0f, 0f, 1f, 1f);
            }
        }

        return new GeneratedFontStruct(null!, charWidth, charHeight, positions, normPositions);
    }

    // ComputeLines — single line

    [TestMethod]
    public void ComputeLines_TextFitsOnOneLine_ReturnsSingleLine()
    {
        GeneratedFontStruct fs = MakeFontStruct("hello");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello"), fs, 1000f, 1f, 0f, 0f);

        Assert.HasCount(1, lines);
        Assert.AreEqual("hello", lines[0].Content);
        Assert.AreEqual(0, lines[0].StartIndex);
        Assert.AreEqual(5, lines[0].EndIndex);
    }

    [TestMethod]
    public void ComputeLines_EmptyString_ReturnsSingleEmptyLine()
    {
        GeneratedFontStruct fs = MakeFontStruct("a");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars(""), fs, 100f, 1f, 0f, 0f);

        Assert.HasCount(1, lines);
        Assert.AreEqual("", lines[0].Content);
        Assert.AreEqual(0, lines[0].StartIndex);
        Assert.AreEqual(0, lines[0].EndIndex);
    }

    [TestMethod]
    public void ComputeLines_TextExactlyFillsWidth_ReturnsSingleLine()
    {
        // "hello" = 5 chars × 10px = 50px = maxWidth exactly
        GeneratedFontStruct fs = MakeFontStruct("hello");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello"), fs, 50f, 1f, 0f, 0f);

        Assert.HasCount(1, lines);
        Assert.AreEqual("hello", lines[0].Content);
    }

    // ComputeLines — word wrap

    [TestMethod]
    public void ComputeLines_TwoWordsThatDontFit_BreakAtSpace()
    {
        // "hello" = 50px, " " = 10px, "world" = 50px; maxWidth = 55 so only "hello" fits
        GeneratedFontStruct fs = MakeFontStruct("hello world");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello world"), fs, 55f, 1f, 0f, 0f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("hello", lines[0].Content);
        Assert.AreEqual("world", lines[1].Content);
        Assert.AreEqual(0, lines[0].StartIndex);
        Assert.AreEqual(5, lines[0].EndIndex);
        Assert.AreEqual(6, lines[1].StartIndex);
        Assert.AreEqual(11, lines[1].EndIndex);
    }

    [TestMethod]
    public void ComputeLines_ThreeWords_BreaksIntoThreeLines()
    {
        // Each word = 30px; space = 10px; maxWidth = 35 — only one word fits per line
        GeneratedFontStruct fs = MakeFontStruct("foo bar baz");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("foo bar baz"), fs, 35f, 1f, 0f, 0f);

        Assert.HasCount(3, lines);
        Assert.AreEqual("foo", lines[0].Content);
        Assert.AreEqual("bar", lines[1].Content);
        Assert.AreEqual("baz", lines[2].Content);
    }

    [TestMethod]
    public void ComputeLines_SpaceExactlyFitsButNextWordDoesNot_BreakAtSpace()
    {
        // "hello" = 50px, " " = 10px → 60px = maxWidth exactly, 'w' pushes to 70 → break at space
        GeneratedFontStruct fs = MakeFontStruct("hello world");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello world"), fs, 60f, 1f, 0f, 0f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("hello", lines[0].Content);
        Assert.AreEqual("world", lines[1].Content);
    }

    // ComputeLines — character wrap

    [TestMethod]
    public void ComputeLines_WordLongerThanWidth_CharacterWraps()
    {
        // "toolongword" = 110px, maxWidth = 60 → breaks after 6 chars
        GeneratedFontStruct fs = MakeFontStruct("toolngwrd");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("toolongword"), fs, 60f, 1f, 0f, 0f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("toolon", lines[0].Content);
        Assert.AreEqual("gword", lines[1].Content);
        Assert.AreEqual(0, lines[0].StartIndex);
        Assert.AreEqual(6, lines[0].EndIndex);
        Assert.AreEqual(6, lines[1].StartIndex);
        Assert.AreEqual(11, lines[1].EndIndex);
    }

    [TestMethod]
    public void ComputeLines_LongWordThenShortWords_CharWrapsThenWordWraps()
    {
        // "toolongword" char-wraps, then " and more" word-wraps
        // maxWidth = 60: "toolon"|"gword"|"and"|"more"
        GeneratedFontStruct fs = MakeFontStruct("toolngwrd a e m ");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("toolongword and more"), fs, 60f, 1f, 0f, 0f);

        Assert.HasCount(4, lines);
        Assert.AreEqual("toolon", lines[0].Content);
        Assert.AreEqual("gword", lines[1].Content);
        Assert.AreEqual("and", lines[2].Content);
        Assert.AreEqual("more", lines[3].Content);
    }

    // ComputeLines — explicit newlines

    [TestMethod]
    public void ComputeLines_ExplicitNewline_ProducesHardBreak()
    {
        GeneratedFontStruct fs = MakeFontStruct("hello\nworld");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello\nworld"), fs, 1000f, 1f, 0f, 0f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("hello", lines[0].Content);
        Assert.AreEqual("world", lines[1].Content);
        Assert.AreEqual(0, lines[0].StartIndex);
        Assert.AreEqual(5, lines[0].EndIndex);
        Assert.AreEqual(6, lines[1].StartIndex);
        Assert.AreEqual(11, lines[1].EndIndex);
    }

    [TestMethod]
    public void ComputeLines_ExplicitNewlineAtStart_ProducesLeadingEmptyLine()
    {
        GeneratedFontStruct fs = MakeFontStruct("hello\n");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("\nhello"), fs, 1000f, 1f, 0f, 0f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("", lines[0].Content);
        Assert.AreEqual("hello", lines[1].Content);
    }

    [TestMethod]
    public void ComputeLines_MultipleExplicitNewlines_EachProducesLine()
    {
        GeneratedFontStruct fs = MakeFontStruct("abc\n");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("a\nb\nc"), fs, 1000f, 1f, 0f, 0f);

        Assert.HasCount(3, lines);
        Assert.AreEqual("a", lines[0].Content);
        Assert.AreEqual("b", lines[1].Content);
        Assert.AreEqual("c", lines[2].Content);
    }

    [TestMethod]
    public void ComputeLines_ExplicitNewlineAndWordWrap_BothApply()
    {
        // "hello world" on line 1 wraps, "foo" on line 2 fits
        GeneratedFontStruct fs = MakeFontStruct("hello world\nfo");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello world\nfoo"), fs, 55f, 1f, 0f, 0f);

        Assert.HasCount(3, lines);
        Assert.AreEqual("hello", lines[0].Content);
        Assert.AreEqual("world", lines[1].Content);
        Assert.AreEqual("foo", lines[2].Content);
    }

    // ComputeLines — scale

    [TestMethod]
    public void ComputeLines_ScaleXDoubled_HalvesEffectiveMaxWidth()
    {
        // scaleX=2: each char effectively 20px wide; "hello"=100px, maxWidth=60
        // → "hel" (60px) | "lo" (40px) | ... actually "hel" = h(20)+e(20)+l(20)=60 exactly, then 'l' would be 80>60
        // So: "hel"|"lo" for "hello"
        GeneratedFontStruct fs = MakeFontStruct("helo");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello"), fs, 60f, 2f, 0f, 0f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("hel", lines[0].Content);
        Assert.AreEqual("lo", lines[1].Content);
    }

    // ComputeLines — extra spacing affecting wrap

    [TestMethod]
    public void ComputeLines_ExtraSpaceWidth_AffectsWrapPoint()
    {
        // "ab" = 20px, " " = 10+5 = 15px with extraSpaceWidth=5, "cd" = 20px
        // Without extra: "ab cd" = 50px ≤ 55 → one line
        // With extra: "ab cd" = 20+15+20 = 55 ≤ 55 → still one line
        // Use maxWidth=34: "ab"=20 ≤ 34, " "=15 → total 35 > 34 → break before space
        GeneratedFontStruct fs = MakeFontStruct("ab cd");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("ab cd"), fs, 34f, 1f, 5f, 0f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("ab", lines[0].Content);
        Assert.AreEqual("cd", lines[1].Content);
    }

    [TestMethod]
    public void ComputeLines_ExtraCharSpacing_AffectsWrapPoint()
    {
        // "ab cd": each non-space char = 10+2 = 12px, space = 10px
        // Without extra: "ab cd" = 50px ≤ 55 → one line
        // With extraCharSpacing=2: 12+12+10+12+12 = 58 > 55 → breaks at space → "ab"|"cd"
        GeneratedFontStruct fs = MakeFontStruct("ab cd");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("ab cd"), fs, 55f, 1f, 0f, 2f);

        Assert.HasCount(2, lines);
        Assert.AreEqual("ab", lines[0].Content);
        Assert.AreEqual("cd", lines[1].Content);
    }

    // ComputeLines — rendered char count

    [TestMethod]
    public void ComputeLines_WordWrap_BreakSpaceNotCountedInRenderedChars()
    {
        // "hello world" wraps at the space; the space becomes the break and is not in either line
        GeneratedFontStruct fs = MakeFontStruct("hello world");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello world"), fs, 55f, 1f, 0f, 0f);

        int totalRendered = 0;
        foreach (LineInfo line in lines) totalRendered += line.Content.Length;

        Assert.AreEqual(10, totalRendered); // "hello"(5) + "world"(5), space excluded
    }

    [TestMethod]
    public void ComputeLines_ExplicitNewline_NewlineNotCountedInRenderedChars()
    {
        // "hello\nworld": newline is a separator, not rendered
        GeneratedFontStruct fs = MakeFontStruct("hello world");
        IReadOnlyList<LineInfo> lines = TextLayout.ComputeLines(Chars("hello\nworld"), fs, 1000f, 1f, 0f, 0f);

        int totalRendered = 0;
        foreach (LineInfo line in lines) totalRendered += line.Content.Length;

        Assert.AreEqual(10, totalRendered); // "hello"(5) + "world"(5), newline excluded
    }

    // MeasureLine

    [TestMethod]
    public void MeasureLine_SimpleString_ReturnsSumOfCharWidths()
    {
        GeneratedFontStruct fs = MakeFontStruct("hello");
        float width = TextLayout.MeasureLine("hello", fs, 1f, 0f, 0f);

        Assert.AreEqual(50f, width, Delta);
    }

    [TestMethod]
    public void MeasureLine_EmptyString_ReturnsZero()
    {
        GeneratedFontStruct fs = MakeFontStruct("a");
        float width = TextLayout.MeasureLine("", fs, 1f, 0f, 0f);

        Assert.AreEqual(0f, width, Delta);
    }

    [TestMethod]
    public void MeasureLine_WithScaleX_ScalesCharWidths()
    {
        GeneratedFontStruct fs = MakeFontStruct("hello");
        float width = TextLayout.MeasureLine("hello", fs, 2f, 0f, 0f);

        Assert.AreEqual(100f, width, Delta);
    }

    [TestMethod]
    public void MeasureLine_WithExtraSpaceWidth_AddsToSpacesOnly()
    {
        // "hi i": h=10, i=10, ' '=10+5=15, i=10 → total=45
        GeneratedFontStruct fs = MakeFontStruct("hi ");
        float width = TextLayout.MeasureLine("hi i", fs, 1f, 5f, 0f);

        Assert.AreEqual(45f, width, Delta);
    }

    [TestMethod]
    public void MeasureLine_WithExtraCharSpacing_AddsToNonSpacesOnly()
    {
        // "abc": a=10+2=12, b=12, c=12 → total=36
        GeneratedFontStruct fs = MakeFontStruct("abc");
        float width = TextLayout.MeasureLine("abc", fs, 1f, 0f, 2f);

        Assert.AreEqual(36f, width, Delta);
    }

    [TestMethod]
    public void MeasureLine_StringWithSpace_SpaceUsesExtraSpaceWidthNotExtraCharSpacing()
    {
        // "a b": a=10+3=13, ' '=10+7=17, b=13 → total=43
        GeneratedFontStruct fs = MakeFontStruct("a b");
        float width = TextLayout.MeasureLine("a b", fs, 1f, 7f, 3f);

        Assert.AreEqual(43f, width, Delta);
    }
}

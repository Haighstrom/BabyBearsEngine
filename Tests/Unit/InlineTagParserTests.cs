using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class InlineTagParserTests
{
    private static Colour Red => new(255, 0, 0);
    private static Colour Blue => new(0, 0, 255);
    private static Colour SemiTransparent => new(255, 0, 0, 128);

    private static void AssertChars(StyledChar[] result, string expectedText)
    {
        Assert.AreEqual(expectedText.Length, result.Length);
        for (int i = 0; i < expectedText.Length; i++)
        {
            Assert.AreEqual(expectedText[i], result[i].Char, $"Char mismatch at index {i}");
        }
    }

    private static void AssertStyle(StyledChar sc, Colour? expectedColour = null, bool expectedUnderline = false, bool expectedStrikethrough = false, bool expectedBold = false, bool expectedItalic = false)
    {
        Assert.AreEqual(expectedColour, sc.Style.ColourOverride);
        Assert.AreEqual(expectedUnderline, sc.Style.Underline);
        Assert.AreEqual(expectedStrikethrough, sc.Style.Strikethrough);
        Assert.AreEqual(expectedBold, sc.Style.Bold);
        Assert.AreEqual(expectedItalic, sc.Style.Italic);
    }

    // No tags

    [TestMethod]
    public void Parse_NoTags_ReturnsDefaultStyle()
    {
        StyledChar[] result = InlineTagParser.Parse("hello", true);

        AssertChars(result, "hello");
        foreach (StyledChar sc in result)
        {
            AssertStyle(sc);
        }
    }

    [TestMethod]
    public void Parse_EmptyString_ReturnsEmpty()
    {
        StyledChar[] result = InlineTagParser.Parse("", true);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void Parse_UseInlineTagsFalse_IgnoresTags()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF0000>hi</colour>", false);

        AssertChars(result, "<colour=#FF0000>hi</colour>");
        foreach (StyledChar sc in result)
        {
            AssertStyle(sc);
        }
    }

    // Colour tag

    [TestMethod]
    public void Parse_ColourTag_SetsColourOverride()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF0000>hi</colour>", true);

        AssertChars(result, "hi");
        AssertStyle(result[0], expectedColour: Red);
        AssertStyle(result[1], expectedColour: Red);
    }

    [TestMethod]
    public void Parse_ColourTagWithAlpha_ParsesAlpha()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF000080>x</colour>", true);

        AssertChars(result, "x");
        AssertStyle(result[0], expectedColour: SemiTransparent);
    }

    [TestMethod]
    public void Parse_ColourTagClosed_RestoresDefaultColour()
    {
        StyledChar[] result = InlineTagParser.Parse("a<colour=#FF0000>b</colour>c", true);

        AssertChars(result, "abc");
        AssertStyle(result[0]);
        AssertStyle(result[1], expectedColour: Red);
        AssertStyle(result[2]);
    }

    [TestMethod]
    public void Parse_NestedColourTags_InnerWins()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF0000>a<colour=#0000FF>b</colour>c</colour>", true);

        AssertChars(result, "abc");
        AssertStyle(result[0], expectedColour: Red);
        AssertStyle(result[1], expectedColour: Blue);
        AssertStyle(result[2], expectedColour: Red);
    }

    // Underline tag

    [TestMethod]
    public void Parse_UnderlineTag_SetsUnderline()
    {
        StyledChar[] result = InlineTagParser.Parse("<u>hi</u>", true);

        AssertChars(result, "hi");
        AssertStyle(result[0], expectedUnderline: true);
        AssertStyle(result[1], expectedUnderline: true);
    }

    [TestMethod]
    public void Parse_UnderlineTagClosed_ClearsUnderline()
    {
        StyledChar[] result = InlineTagParser.Parse("a<u>b</u>c", true);

        AssertChars(result, "abc");
        AssertStyle(result[0]);
        AssertStyle(result[1], expectedUnderline: true);
        AssertStyle(result[2]);
    }

    // Strikethrough tag

    [TestMethod]
    public void Parse_StrikethroughTag_SetsStrikethrough()
    {
        StyledChar[] result = InlineTagParser.Parse("<s>hi</s>", true);

        AssertChars(result, "hi");
        AssertStyle(result[0], expectedStrikethrough: true);
        AssertStyle(result[1], expectedStrikethrough: true);
    }

    [TestMethod]
    public void Parse_StrikethroughTagClosed_ClearsStrikethrough()
    {
        StyledChar[] result = InlineTagParser.Parse("a<s>b</s>c", true);

        AssertChars(result, "abc");
        AssertStyle(result[0]);
        AssertStyle(result[1], expectedStrikethrough: true);
        AssertStyle(result[2]);
    }

    // Combined tags

    [TestMethod]
    public void Parse_ColourAndUnderlineCombined_BothApply()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF0000><u>x</u></colour>", true);

        AssertChars(result, "x");
        AssertStyle(result[0], expectedColour: Red, expectedUnderline: true);
    }

    [TestMethod]
    public void Parse_UnderlineAndStrikethroughCombined_BothApply()
    {
        StyledChar[] result = InlineTagParser.Parse("<u><s>x</s></u>", true);

        AssertChars(result, "x");
        AssertStyle(result[0], expectedUnderline: true, expectedStrikethrough: true);
    }

    // Close-last tag </>

    [TestMethod]
    public void Parse_CloseLastTag_ClosesLastOpened()
    {
        StyledChar[] result = InlineTagParser.Parse("<u>a</>b", true);

        AssertChars(result, "ab");
        AssertStyle(result[0], expectedUnderline: true);
        AssertStyle(result[1]);
    }

    [TestMethod]
    public void Parse_CloseLastTag_WhenMultipleOpen_ClosesInnermost()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF0000><u>a</>b</colour>", true);

        AssertChars(result, "ab");
        AssertStyle(result[0], expectedColour: Red, expectedUnderline: true);
        AssertStyle(result[1], expectedColour: Red);
    }

    [TestMethod]
    public void Parse_CloseLastTag_WithNothingOpen_DoesNotCrash()
    {
        StyledChar[] result = InlineTagParser.Parse("a</>b", true);

        AssertChars(result, "ab");
    }

    // Unclosed tags

    [TestMethod]
    public void Parse_UnclosedTag_StyleAppliesUntilEnd()
    {
        StyledChar[] result = InlineTagParser.Parse("a<u>bc", true);

        AssertChars(result, "abc");
        AssertStyle(result[0]);
        AssertStyle(result[1], expectedUnderline: true);
        AssertStyle(result[2], expectedUnderline: true);
    }

    // Escaping

    [TestMethod]
    public void Parse_EscapedLessThan_ProducesLiteralBracket()
    {
        StyledChar[] result = InlineTagParser.Parse(@"a\<b", true);

        AssertChars(result, "a<b");
        foreach (StyledChar sc in result)
        {
            AssertStyle(sc);
        }
    }

    // Unknown / malformed tags

    [TestMethod]
    public void Parse_UnknownTag_StrippedAndCharsUnaffected()
    {
        StyledChar[] result = InlineTagParser.Parse("a<bold>b</bold>c", true);

        AssertChars(result, "abc");
        foreach (StyledChar sc in result)
        {
            AssertStyle(sc);
        }
    }

    [TestMethod]
    public void Parse_MalformedColourValue_TagIgnored()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=notacolour>x</colour>", true);

        AssertChars(result, "x");
        AssertStyle(result[0]);
    }

    [TestMethod]
    public void Parse_UnclosedAngleBracket_TreatedAsLiteralChar()
    {
        StyledChar[] result = InlineTagParser.Parse("a<b", true);

        AssertChars(result, "a<b");
    }

    // Named close with no matching open

    [TestMethod]
    public void Parse_NamedCloseWithNoMatchingOpen_DoesNotCrash()
    {
        StyledChar[] result = InlineTagParser.Parse("a</u>b", true);

        AssertChars(result, "ab");
        foreach (StyledChar sc in result)
        {
            AssertStyle(sc);
        }
    }

    // Named close removes last matching, leaving earlier same-type active

    [TestMethod]
    public void Parse_NamedClose_RemovesLastMatchingOverride()
    {
        // Two nested colour tags; </colour> removes inner, outer red is restored
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF0000><colour=#0000FF>a</colour>b", true);

        AssertChars(result, "ab");
        AssertStyle(result[0], expectedColour: Blue);
        AssertStyle(result[1], expectedColour: Red);
    }

    // Bold tag

    [TestMethod]
    public void Parse_BoldTag_SetsBold()
    {
        StyledChar[] result = InlineTagParser.Parse("<b>hi</b>", true);

        AssertChars(result, "hi");
        AssertStyle(result[0], expectedBold: true);
        AssertStyle(result[1], expectedBold: true);
    }

    [TestMethod]
    public void Parse_BoldTagClosed_ClearsBold()
    {
        StyledChar[] result = InlineTagParser.Parse("a<b>b</b>c", true);

        AssertChars(result, "abc");
        AssertStyle(result[0]);
        AssertStyle(result[1], expectedBold: true);
        AssertStyle(result[2]);
    }

    // Italic tag

    [TestMethod]
    public void Parse_ItalicTag_SetsItalic()
    {
        StyledChar[] result = InlineTagParser.Parse("<i>hi</i>", true);

        AssertChars(result, "hi");
        AssertStyle(result[0], expectedItalic: true);
        AssertStyle(result[1], expectedItalic: true);
    }

    [TestMethod]
    public void Parse_ItalicTagClosed_ClearsItalic()
    {
        StyledChar[] result = InlineTagParser.Parse("a<i>b</i>c", true);

        AssertChars(result, "abc");
        AssertStyle(result[0]);
        AssertStyle(result[1], expectedItalic: true);
        AssertStyle(result[2]);
    }

    // Combined bold/italic + interaction with other tags

    [TestMethod]
    public void Parse_BoldAndItalicCombined_BothApply()
    {
        StyledChar[] result = InlineTagParser.Parse("<b><i>x</i></b>", true);

        AssertChars(result, "x");
        AssertStyle(result[0], expectedBold: true, expectedItalic: true);
    }

    [TestMethod]
    public void Parse_BoldWithColourAndUnderline_AllApply()
    {
        StyledChar[] result = InlineTagParser.Parse("<colour=#FF0000><u><b>x</b></u></colour>", true);

        AssertChars(result, "x");
        AssertStyle(result[0], expectedColour: Red, expectedUnderline: true, expectedBold: true);
    }

    [TestMethod]
    public void Parse_NestedBoldTags_OuterStillBoldAfterInnerClose()
    {
        StyledChar[] result = InlineTagParser.Parse("<b>a<b>b</b>c</b>d", true);

        AssertChars(result, "abcd");
        AssertStyle(result[0], expectedBold: true);
        AssertStyle(result[1], expectedBold: true);
        AssertStyle(result[2], expectedBold: true);
        AssertStyle(result[3]);
    }

    [TestMethod]
    public void Parse_CloseLastTag_ClosesBold()
    {
        StyledChar[] result = InlineTagParser.Parse("<b>a</>b", true);

        AssertChars(result, "ab");
        AssertStyle(result[0], expectedBold: true);
        AssertStyle(result[1]);
    }

    [TestMethod]
    public void Parse_CloseLastTag_ClosesItalic()
    {
        StyledChar[] result = InlineTagParser.Parse("<i>a</>b", true);

        AssertChars(result, "ab");
        AssertStyle(result[0], expectedItalic: true);
        AssertStyle(result[1]);
    }

    [TestMethod]
    public void Parse_BoldTagsIgnoredWhenUseInlineTagsFalse()
    {
        StyledChar[] result = InlineTagParser.Parse("<b>hi</b>", false);

        AssertChars(result, "<b>hi</b>");
        foreach (StyledChar sc in result)
        {
            AssertStyle(sc);
        }
    }

    [TestMethod]
    public void Parse_BoldDoesNotMatchUnknownBoldTag()
    {
        // <bold> is unknown — must NOT be treated as <b>.
        StyledChar[] result = InlineTagParser.Parse("a<bold>b</bold>c", true);

        AssertChars(result, "abc");
        foreach (StyledChar sc in result)
        {
            AssertStyle(sc);
        }
    }
}

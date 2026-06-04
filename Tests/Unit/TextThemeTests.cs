using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TextThemeTests
{
    [TestMethod]
    public void Default_OverrideFont_IsObservableByLaterReads()
    {
        TextTheme original = TextTheme.Default;
        try
        {
            TextTheme.Default = original with { Font = new FontDefinition("CustomGameFont", 24) };

            Assert.AreEqual("CustomGameFont", TextTheme.Default.Font.FontName);
            Assert.AreEqual(24, TextTheme.Default.Font.FontSize);
        }
        finally
        {
            TextTheme.Default = original;
        }
    }

    [TestMethod]
    public void InputBoxTheme_FromColour_UsesCurrentTextThemeDefault_Font()
    {
        // Cloning TextTheme.Default lets cross-platform / themed apps swap the default font in
        // one place and have it flow through every downstream *.Default — including
        // InputBoxTheme.FromColour, which previously hard-coded its own Times New Roman.
        TextTheme original = TextTheme.Default;
        try
        {
            TextTheme.Default = original with { Font = new FontDefinition("CustomGameFont", 24) };

            var theme = InputBoxTheme.FromColour(Colour.White);

            Assert.AreEqual("CustomGameFont", theme.Text.Font.FontName);
            Assert.AreEqual(24, theme.Text.Font.FontSize);
        }
        finally
        {
            TextTheme.Default = original;
        }
    }

    [TestMethod]
    public void InputBoxTheme_FromColour_LeftAlignsText()
    {
        // FromColour's overrides on TextTheme.Default must preserve the input-box-specific
        // alignment (left-centred), not whatever the default theme happens to be set to.
        var theme = InputBoxTheme.FromColour(Colour.White);

        Assert.AreEqual(HAlignment.Left, theme.Text.HAlignment);
        Assert.AreEqual(VAlignment.Centred, theme.Text.VAlignment);
    }
}

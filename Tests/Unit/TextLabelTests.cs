using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Cameras;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TextLabelTests
{
    private sealed class StubColourGraphic : AddableBase, IColourGraphic
    {
        public Colour Colour { get; set; } = Colour.Black;
        public bool Visible { get; set; } = true;

        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private sealed class StubBorderGraphic : AddableBase, IBorderGraphic
    {
        public Colour BorderColour { get; set; } = Colour.Black;
        public BorderPosition BorderPosition { get; set; } = BorderPosition.Inside;
        public float BorderThickness { get; set; } = 1f;
        public bool Visible { get; set; } = true;

        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private static TextLabel MakeLabel(string text = "hello", Colour colour = default,
        IColourGraphic? background = null, IBorderGraphic? border = null)
    {
        StubTextGraphic stub = new() { Text = text, Colour = colour };
        return new TextLabel(10, 20, 200, 30, stub, background, border);
    }

    // Constructor

    [TestMethod]
    public void Constructor_StoresPositionAndSize()
    {
        TextLabel label = MakeLabel();

        Assert.AreEqual(10f, label.X);
        Assert.AreEqual(20f, label.Y);
        Assert.AreEqual(200f, label.Width);
        Assert.AreEqual(30f, label.Height);
    }

    [TestMethod]
    public void Constructor_SetsInitialText()
    {
        TextLabel label = MakeLabel(text: "hello");

        Assert.AreEqual("hello", label.Text);
    }

    [TestMethod]
    public void Constructor_SetsInitialColour()
    {
        TextLabel label = MakeLabel(colour: Colour.Red);

        Assert.AreEqual(Colour.Red, label.Colour);
    }

    // Text property

    [TestMethod]
    public void Text_Set_UpdatesText()
    {
        TextLabel label = MakeLabel(text: "before");

        label.Text = "after";

        Assert.AreEqual("after", label.Text);
    }

    [TestMethod]
    public void Text_SetEmpty_AllowsEmptyString()
    {
        TextLabel label = MakeLabel(text: "something");

        label.Text = "";

        Assert.AreEqual("", label.Text);
    }

    // Colour property

    [TestMethod]
    public void Colour_Set_UpdatesColour()
    {
        TextLabel label = MakeLabel(colour: Colour.Black);

        label.Colour = Colour.Red;

        Assert.AreEqual(Colour.Red, label.Colour);
    }

    // BackgroundColour property

    [TestMethod]
    public void BackgroundColour_WhenNoBackground_ReturnsNull()
    {
        TextLabel label = MakeLabel();

        Assert.IsNull(label.BackgroundColour);
    }

    [TestMethod]
    public void BackgroundColour_WhenBackgroundProvided_ReturnsColour()
    {
        StubColourGraphic bg = new() { Colour = Colour.Red, Visible = true };
        TextLabel label = MakeLabel(background: bg);

        Assert.AreEqual(Colour.Red, label.BackgroundColour);
    }

    [TestMethod]
    public void BackgroundColour_Set_UpdatesColour()
    {
        StubColourGraphic bg = new() { Colour = Colour.Red, Visible = true };
        TextLabel label = MakeLabel(background: bg);

        label.BackgroundColour = Colour.Blue;

        Assert.AreEqual(Colour.Blue, label.BackgroundColour);
    }

    [TestMethod]
    public void BackgroundColour_SetToNull_HidesBackground()
    {
        StubColourGraphic bg = new() { Colour = Colour.Red, Visible = true };
        TextLabel label = MakeLabel(background: bg);

        label.BackgroundColour = null;

        Assert.IsNull(label.BackgroundColour);
    }

    [TestMethod]
    public void BackgroundColour_SetWhenNoBackground_IsNoOp()
    {
        TextLabel label = MakeLabel();

        label.BackgroundColour = Colour.Red;

        Assert.IsNull(label.BackgroundColour);
    }

    // BorderColour property

    [TestMethod]
    public void BorderColour_WhenNoBorder_ReturnsNull()
    {
        TextLabel label = MakeLabel();

        Assert.IsNull(label.BorderColour);
    }

    [TestMethod]
    public void BorderColour_WhenBorderProvided_ReturnsColour()
    {
        StubBorderGraphic border = new() { BorderColour = Colour.Red, Visible = true };
        TextLabel label = MakeLabel(border: border);

        Assert.AreEqual(Colour.Red, label.BorderColour);
    }

    [TestMethod]
    public void BorderColour_Set_UpdatesColour()
    {
        StubBorderGraphic border = new() { BorderColour = Colour.Red, Visible = true };
        TextLabel label = MakeLabel(border: border);

        label.BorderColour = Colour.Blue;

        Assert.AreEqual(Colour.Blue, label.BorderColour);
    }

    [TestMethod]
    public void BorderColour_SetToNull_HidesBorder()
    {
        StubBorderGraphic border = new() { BorderColour = Colour.Red, Visible = true };
        TextLabel label = MakeLabel(border: border);

        label.BorderColour = null;

        Assert.IsNull(label.BorderColour);
    }

    [TestMethod]
    public void BorderColour_SetWhenNoBorder_IsNoOp()
    {
        TextLabel label = MakeLabel();

        label.BorderColour = Colour.Red;

        Assert.IsNull(label.BorderColour);
    }
}

using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ProgressBarTests
{
    private sealed class StubGraphic : GraphicBase, IGraphic
    {
        public Colour Colour { get; set; }
        public override void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private static ProgressBarTheme StubTheme() => new()
    {
        BackgroundFactory = _ => new StubGraphic(),
        FillFactory = _ => new StubGraphic(),
    };

    private static ProgressBar Make(float amountFilled = 0f) =>
        new(0, 0, 200, 20, StubTheme(), amountFilled);

    private static ProgressBar MakeWithFill(IGraphic fill)
    {
        ProgressBarTheme theme = new()
        {
            BackgroundFactory = _ => new StubGraphic(),
            FillFactory = r =>
            {
                fill.Width = r.W;
                fill.Height = r.H;
                return fill;
            },
        };

        return new ProgressBar(0, 0, 200, 20, theme);
    }

    // Initial state

    [TestMethod]
    public void Constructor_AmountFilledIsZeroByDefault()
    {
        ProgressBar bar = Make();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Constructor_InitialAmountFilled_IsApplied()
    {
        ProgressBar bar = Make(0.5f);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    // AmountFilled clamping

    [TestMethod]
    public void AmountFilled_SetAboveOne_ClampsToOne()
    {
        ProgressBar bar = Make();

        bar.AmountFilled = 1.5f;

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetBelowZero_ClampsToZero()
    {
        ProgressBar bar = Make();

        bar.AmountFilled = -0.1f;

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetSameValue_NoEventFired()
    {
        ProgressBar bar = Make(0.5f);
        int fired = 0;
        bar.BarFilled += (_, _) => fired++;

        bar.AmountFilled = 0.5f;

        Assert.AreEqual(0, fired);
    }

    // BarFilled event

    [TestMethod]
    public void AmountFilled_SetToOne_RaisesBarFilled()
    {
        ProgressBar bar = Make();
        bool fired = false;
        bar.BarFilled += (_, _) => fired = true;

        bar.AmountFilled = 1f;

        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void AmountFilled_SetToOneAgain_DoesNotFireBarFilledTwice()
    {
        ProgressBar bar = Make();
        int fired = 0;
        bar.BarFilled += (_, _) => fired++;

        bar.AmountFilled = 1f;
        bar.AmountFilled = 1f;

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void AmountFilled_SetToPartialThenOne_RaisesBarFilled()
    {
        ProgressBar bar = Make();
        bool fired = false;
        bar.BarFilled += (_, _) => fired = true;

        bar.AmountFilled = 0.5f;
        bar.AmountFilled = 1f;

        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void AmountFilled_AlreadyFull_SetBelowOne_ThenSetToOne_RaisesBarFilledAgain()
    {
        ProgressBar bar = Make();
        int fired = 0;
        bar.BarFilled += (_, _) => fired++;

        bar.AmountFilled = 1f;
        bar.AmountFilled = 0.5f;
        bar.AmountFilled = 1f;

        Assert.AreEqual(2, fired);
    }

    // Fill graphic tracking

    [TestMethod]
    public void AmountFilled_NonTextureFill_MutatesFillWidth()
    {
        StubGraphic fill = new();
        ProgressBar bar = MakeWithFill(fill);

        bar.AmountFilled = 0.25f;

        Assert.AreEqual(50f, fill.Width);
    }
}

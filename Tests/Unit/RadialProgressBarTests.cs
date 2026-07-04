using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RadialProgressBarTests
{
    private sealed class StubGraphic : GraphicBase, IGraphic
    {
        public Colour Colour { get; set; }
        public float Angle { get; set; } = 0f;
        public int RenderCallIndex { get; private set; } = -1;
        public static int NextRenderCallIndex { get; set; } = 0;

        public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
        {
            RenderCallIndex = NextRenderCallIndex++;
        }
    }

    private static RadialProgressBarTheme StubTheme() => new()
    {
        BackgroundFactory = _ => new StubGraphic(),
        FillFactory = _ => new StubGraphic(),
    };

    private static RadialProgressBar Make(float amountFilled = 0f) =>
        new(0, 0, 100, 100, StubTheme(), amountFilled);

    // Initial state

    [TestMethod]
    public void Constructor_AmountFilledIsZeroByDefault()
    {
        RadialProgressBar bar = Make();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Constructor_InitialAmountFilled_IsApplied()
    {
        RadialProgressBar bar = Make(0.5f);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    // AmountFilled clamping

    [TestMethod]
    public void AmountFilled_SetAboveOne_ClampsToOne()
    {
        RadialProgressBar bar = Make();

        bar.AmountFilled = 1.5f;

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetBelowZero_ClampsToZero()
    {
        RadialProgressBar bar = Make();

        bar.AmountFilled = -0.1f;

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetSameValue_NoEventFired()
    {
        RadialProgressBar bar = Make(0.5f);
        int fired = 0;
        bar.Filled += (_, _) => fired++;

        bar.AmountFilled = 0.5f;

        Assert.AreEqual(0, fired);
    }

    // Filled event

    [TestMethod]
    public void AmountFilled_SetToOne_RaisesFilled()
    {
        RadialProgressBar bar = Make();
        bool fired = false;
        bar.Filled += (_, _) => fired = true;

        bar.AmountFilled = 1f;

        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void AmountFilled_SetToOneAgain_DoesNotFireFilledTwice()
    {
        RadialProgressBar bar = Make();
        int fired = 0;
        bar.Filled += (_, _) => fired++;

        bar.AmountFilled = 1f;
        bar.AmountFilled = 1f;

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void AmountFilled_SetToPartialThenOne_RaisesFilled()
    {
        RadialProgressBar bar = Make();
        bool fired = false;
        bar.Filled += (_, _) => fired = true;

        bar.AmountFilled = 0.5f;
        bar.AmountFilled = 1f;

        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void AmountFilled_AlreadyFull_SetBelowOne_ThenSetToOne_RaisesFilledAgain()
    {
        RadialProgressBar bar = Make();
        int fired = 0;
        bar.Filled += (_, _) => fired++;

        bar.AmountFilled = 1f;
        bar.AmountFilled = 0.5f;
        bar.AmountFilled = 1f;

        Assert.AreEqual(2, fired);
    }

    // Identity / layering

    [TestMethod]
    public void Constructor_DefaultLayer_IsIntMaxValue()
    {
        RadialProgressBar bar = Make();

        Assert.AreEqual(int.MaxValue, bar.Layer);
    }

    [TestMethod]
    public void Render_DrawsBackgroundBeforeFill()
    {
        StubGraphic background = new();
        StubGraphic fill = new();
        RadialProgressBarTheme theme = new()
        {
            BackgroundFactory = _ => background,
            FillFactory = _ => fill,
        };
        RadialProgressBar bar = new(0, 0, 100, 100, theme);
        StubGraphic.NextRenderCallIndex = 0;

        Matrix3 projection = Matrix3.Identity;
        Matrix3 modelView = Matrix3.Identity;
        bar.Render(ref projection, ref modelView);

        Assert.IsLessThan(fill.RenderCallIndex, background.RenderCallIndex);
    }
}

using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ProgressBarTests
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

    // Identity / layering

    [TestMethod]
    public void Constructor_DefaultLayer_IsIntMaxValue()
    {
        ProgressBar bar = Make();

        Assert.AreEqual(int.MaxValue, bar.Layer);
    }

    [TestMethod]
    public void Render_DrawsBackgroundBeforeFill()
    {
        StubGraphic background = new();
        StubGraphic fill = new();
        ProgressBarTheme theme = new()
        {
            BackgroundFactory = _ => background,
            FillFactory = _ => fill,
        };
        ProgressBar bar = new(0, 0, 200, 20, theme);
        StubGraphic.NextRenderCallIndex = 0;

        Matrix3 projection = Matrix3.Identity;
        Matrix3 modelView = Matrix3.Identity;
        bar.Render(ref projection, ref modelView);

        Assert.IsLessThan(fill.RenderCallIndex, background.RenderCallIndex);
    }
}

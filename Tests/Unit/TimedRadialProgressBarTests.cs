using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TimedRadialProgressBarTests
{
    private sealed class StubGraphic : GraphicBase, IGraphic
    {
        public Colour Colour { get; set; }
        public float Angle { get; set; } = 0f;
        public override void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private static RadialProgressBarTheme StubTheme() => new()
    {
        BackgroundFactory = _ => new StubGraphic(),
        FillFactory = _ => new StubGraphic(),
    };

    private static TimedRadialProgressBar Make(double duration) =>
        new(0, 0, 100, 100, StubTheme(), duration);

    // Initial state

    [TestMethod]
    public void Constructor_AmountFilledIsZero()
    {
        TimedRadialProgressBar bar = Make(5.0);

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    // Update — fill progression

    [TestMethod]
    public void Update_PartialElapsed_FillsProportionally()
    {
        TimedRadialProgressBar bar = Make(4.0);

        bar.Update(1.0);

        Assert.AreEqual(0.25f, bar.AmountFilled);
    }

    [TestMethod]
    public void Update_ExactDuration_FillsCompletely()
    {
        TimedRadialProgressBar bar = Make(4.0);

        bar.Update(4.0);

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void Update_BeyondDuration_ClampsAtOne()
    {
        TimedRadialProgressBar bar = Make(4.0);

        bar.Update(999.0);

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void Update_AccumulatesAcrossMultipleCalls()
    {
        TimedRadialProgressBar bar = Make(4.0);

        bar.Update(1.0);
        bar.Update(1.0);
        bar.Update(1.0);

        Assert.AreEqual(0.75f, bar.AmountFilled);
    }

    // Update — Filled event

    [TestMethod]
    public void Update_WhenFillReachesOne_FiresFilled()
    {
        TimedRadialProgressBar bar = Make(4.0);
        bool fired = false;
        bar.Filled += (_, _) => fired = true;

        bar.Update(4.0);

        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void Update_WhenAlreadyFull_DoesNotFireFilledAgain()
    {
        TimedRadialProgressBar bar = Make(4.0);
        int fired = 0;
        bar.Filled += (_, _) => fired++;

        bar.Update(4.0);
        bar.Update(4.0);

        Assert.AreEqual(1, fired);
    }

    // Restart

    [TestMethod]
    public void Restart_ResetsElapsedAndFill()
    {
        TimedRadialProgressBar bar = Make(4.0);
        bar.Update(4.0);

        bar.Restart();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Restart_AllowsBarToFillAgain()
    {
        TimedRadialProgressBar bar = Make(4.0);
        bar.Update(4.0);
        bar.Restart();

        bar.Update(2.0);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    [TestMethod]
    public void Restart_WithNewDuration_UsesNewDuration()
    {
        TimedRadialProgressBar bar = Make(4.0);
        bar.Restart(newDuration: 2.0);

        bar.Update(1.0);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    [TestMethod]
    public void Restart_WithNewDuration_AllowsFilledToFireAgain()
    {
        TimedRadialProgressBar bar = Make(4.0);
        bar.Update(4.0);
        bar.Restart();
        int fired = 0;
        bar.Filled += (_, _) => fired++;

        bar.Update(4.0);

        Assert.AreEqual(1, fired);
    }

    // Identity / IUpdateable

    [TestMethod]
    public void IsIUpdateable()
    {
        TimedRadialProgressBar bar = Make(4.0);

        Assert.IsInstanceOfType<IUpdateable>(bar);
    }

    [TestMethod]
    public void Constructor_ActiveIsTrueByDefault()
    {
        TimedRadialProgressBar bar = Make(4.0);

        Assert.IsTrue(bar.Active);
    }
}

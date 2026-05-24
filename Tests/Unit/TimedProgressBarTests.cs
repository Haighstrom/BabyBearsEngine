using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TimedProgressBarTests
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

    private static TimedProgressBar Make(double duration) =>
        new(0, 0, 300, 30, StubTheme(), duration);

    // Initial state

    [TestMethod]
    public void Constructor_AmountFilledIsZero()
    {
        TimedProgressBar bar = Make(5.0);

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    // Update — fill progression

    [TestMethod]
    public void Update_PartialElapsed_FillsProportionally()
    {
        TimedProgressBar bar = Make(4.0);

        bar.Update(1.0);

        Assert.AreEqual(0.25f, bar.AmountFilled);
    }

    [TestMethod]
    public void Update_ExactDuration_FillsCompletely()
    {
        TimedProgressBar bar = Make(4.0);

        bar.Update(4.0);

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void Update_BeyondDuration_ClampsAtOne()
    {
        TimedProgressBar bar = Make(4.0);

        bar.Update(999.0);

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void Update_AccumulatesAcrossMultipleCalls()
    {
        TimedProgressBar bar = Make(4.0);

        bar.Update(1.0);
        bar.Update(1.0);
        bar.Update(1.0);

        Assert.AreEqual(0.75f, bar.AmountFilled);
    }

    // Update — BarFilled event

    [TestMethod]
    public void Update_WhenFillReachesOne_FiresBarFilled()
    {
        TimedProgressBar bar = Make(4.0);
        bool fired = false;
        bar.BarFilled += (_, _) => fired = true;

        bar.Update(4.0);

        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void Update_WhenAlreadyFull_DoesNotFireBarFilledAgain()
    {
        TimedProgressBar bar = Make(4.0);
        int fired = 0;
        bar.BarFilled += (_, _) => fired++;

        bar.Update(4.0);
        bar.Update(4.0);

        Assert.AreEqual(1, fired);
    }

    // Restart

    [TestMethod]
    public void Restart_ResetsElapsedAndFill()
    {
        TimedProgressBar bar = Make(4.0);
        bar.Update(4.0);

        bar.Restart();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Restart_AllowsBarToFillAgain()
    {
        TimedProgressBar bar = Make(4.0);
        bar.Update(4.0);
        bar.Restart();

        bar.Update(2.0);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    [TestMethod]
    public void Restart_WithNewDuration_UsesNewDuration()
    {
        TimedProgressBar bar = Make(4.0);
        bar.Restart(newDuration: 2.0);

        bar.Update(1.0);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    [TestMethod]
    public void Restart_WithNewDuration_AllowsBarFilledToFireAgain()
    {
        TimedProgressBar bar = Make(4.0);
        bar.Update(4.0);
        bar.Restart();
        int fired = 0;
        bar.BarFilled += (_, _) => fired++;

        bar.Update(4.0);

        Assert.AreEqual(1, fired);
    }

    // Identity / IUpdateable

    [TestMethod]
    public void IsIUpdateable()
    {
        TimedProgressBar bar = Make(4.0);

        Assert.IsInstanceOfType<IUpdateable>(bar);
    }

    [TestMethod]
    public void Constructor_ActiveIsTrueByDefault()
    {
        TimedProgressBar bar = Make(4.0);

        Assert.IsTrue(bar.Active);
    }
}

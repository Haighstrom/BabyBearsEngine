using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ScrollbarTests
{
    private static Scrollbar MakeH(float thumbProportion = 0.2f, float amountFilled = 0f) =>
        new(200, 20, ScrollbarDirection.Horizontal, thumbProportion, amountFilled);

    private static Scrollbar MakeV(float thumbProportion = 0.2f, float amountFilled = 0f) =>
        new(20, 200, ScrollbarDirection.Vertical, thumbProportion, amountFilled);

    // Initial state

    [TestMethod]
    public void Constructor_AmountFilledIsZeroByDefault()
    {
        Scrollbar bar = MakeH();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Constructor_InitialAmountFilled_IsApplied()
    {
        Scrollbar bar = MakeH(amountFilled: 0.5f);

        Assert.AreEqual(0.5f, bar.AmountFilled);
    }

    [TestMethod]
    public void Constructor_ThumbProportionIsSet()
    {
        Scrollbar bar = MakeH(thumbProportion: 0.3f);

        Assert.AreEqual(0.3f, bar.ThumbProportion);
    }

    // AmountFilled clamping

    [TestMethod]
    public void AmountFilled_SetAboveOne_ClampsToOne()
    {
        Scrollbar bar = MakeH();

        bar.AmountFilled = 1.5f;

        Assert.AreEqual(1f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetBelowZero_ClampsToZero()
    {
        Scrollbar bar = MakeH();

        bar.AmountFilled = -0.5f;

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void AmountFilled_SetSameValue_DoesNotFireScrollChanged()
    {
        Scrollbar bar = MakeH(amountFilled: 0.5f);
        int fired = 0;
        bar.ScrollChanged += (_, _) => fired++;

        bar.AmountFilled = 0.5f;

        Assert.AreEqual(0, fired);
    }

    // ScrollChanged event

    [TestMethod]
    public void AmountFilled_SetNewValue_RaisesScrollChanged()
    {
        Scrollbar bar = MakeH();
        bool raised = false;
        bar.ScrollChanged += (_, _) => raised = true;

        bar.AmountFilled = 0.5f;

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ScrollChanged_EventArgs_HasCorrectOldAndNewValues()
    {
        Scrollbar bar = MakeH(amountFilled: 0.2f);
        ScrollChangedEventArgs? args = null;
        bar.ScrollChanged += (_, e) => args = e;

        bar.AmountFilled = 0.8f;

        Assert.IsNotNull(args);
        Assert.AreEqual(0.2f, args.OldValue);
        Assert.AreEqual(0.8f, args.NewValue);
    }

    [TestMethod]
    public void ScrollChanged_ClampsOutOfRangeValue_ReportsClampedInArgs()
    {
        Scrollbar bar = MakeH(amountFilled: 0.5f);
        ScrollChangedEventArgs? args = null;
        bar.ScrollChanged += (_, e) => args = e;

        bar.AmountFilled = 2f;

        Assert.IsNotNull(args);
        Assert.AreEqual(1f, args.NewValue);
    }

    // ThumbProportion clamping

    [TestMethod]
    public void ThumbProportion_SetAboveOne_ClampsToOne()
    {
        Scrollbar bar = MakeH();

        bar.ThumbProportion = 1.5f;

        Assert.AreEqual(1f, bar.ThumbProportion);
    }

    [TestMethod]
    public void ThumbProportion_SetBelowMinimum_ClampsToMinimum()
    {
        Scrollbar bar = MakeH();

        bar.ThumbProportion = 0.01f;

        Assert.IsGreaterThanOrEqualTo(0.05f, bar.ThumbProportion);
    }

    // Vertical variant construction

    [TestMethod]
    public void Vertical_Constructor_AmountFilledIsZeroByDefault()
    {
        Scrollbar bar = MakeV();

        Assert.AreEqual(0f, bar.AmountFilled);
    }

    [TestMethod]
    public void Vertical_AmountFilled_SetNewValue_RaisesScrollChanged()
    {
        Scrollbar bar = MakeV();
        bool raised = false;
        bar.ScrollChanged += (_, _) => raised = true;

        bar.AmountFilled = 0.5f;

        Assert.IsTrue(raised);
    }
}

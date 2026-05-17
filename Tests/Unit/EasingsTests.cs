using System;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class EasingsTests
{
    // All easing functions must satisfy f(0) == 0 and f(1) == 1.
    private static readonly Func<double, double>[] s_allFunctions =
    [
        Easings.Linear,
        Easings.EaseInQuad,
        Easings.EaseOutQuad,
        Easings.EaseInOutQuad,
        Easings.EaseInCubic,
        Easings.EaseOutCubic,
        Easings.EaseInOutCubic,
        Easings.EaseInSine,
        Easings.EaseOutSine,
        Easings.EaseInOutSine,
        Easings.EaseInBack,
        Easings.EaseOutBack,
        Easings.EaseOutBounce,
    ];

    [TestMethod]
    public void AllFunctions_AtZero_ReturnZero()
    {
        foreach (var f in s_allFunctions)
        {
            Assert.AreEqual(0.0, f(0.0), delta: 1e-10);
        }
    }

    [TestMethod]
    public void AllFunctions_AtOne_ReturnOne()
    {
        foreach (var f in s_allFunctions)
        {
            Assert.AreEqual(1.0, f(1.0), delta: 1e-10);
        }
    }

    [TestMethod]
    public void EaseInQuad_Midpoint_IsQuarterOfTheWay()
    {
        Assert.AreEqual(0.25, Easings.EaseInQuad(0.5), delta: 1e-10);
    }

    [TestMethod]
    public void EaseOutQuad_Midpoint_IsThreeQuartersOfTheWay()
    {
        Assert.AreEqual(0.75, Easings.EaseOutQuad(0.5), delta: 1e-10);
    }

    [TestMethod]
    public void EaseInOutQuad_Midpoint_IsHalfWay()
    {
        // The in-out function is symmetric: the midpoint maps to 0.5.
        Assert.AreEqual(0.5, Easings.EaseInOutQuad(0.5), delta: 1e-10);
    }

    [TestMethod]
    public void Linear_ReturnsSameValueAsInput()
    {
        Assert.AreEqual(0.3, Easings.Linear(0.3), delta: 1e-10);
        Assert.AreEqual(0.7, Easings.Linear(0.7), delta: 1e-10);
    }

    [TestMethod]
    public void EaseInBack_MidProgress_DipsBelowZero()
    {
        // EaseInBack pulls back past the start — output should be negative near t=0.
        Assert.IsTrue(Easings.EaseInBack(0.2) < 0.0);
    }

    [TestMethod]
    public void EaseOutBack_NearEnd_ExceedsOne()
    {
        // EaseOutBack overshoots past the end — output should exceed 1 near t=1.
        Assert.IsTrue(Easings.EaseOutBack(0.8) > 1.0);
    }
}

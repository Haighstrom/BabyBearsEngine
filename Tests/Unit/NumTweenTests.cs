using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class NumTweenTests
{
    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { entity.Parent = null; }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private static NumTween InContainer(NumTween tween)
    {
        tween.Parent = new FakeContainer();
        return tween;
    }

    [TestMethod]
    public void InitialValue_IsStartValue()
    {
        var tween = new NumTween(10.0, 20.0, duration: 1.0);

        Assert.AreEqual(10.0, tween.Value, delta: 1e-10);
    }

    [TestMethod]
    public void Update_Midway_ValueIsMidpoint()
    {
        var tween = InContainer(new NumTween(0.0, 10.0, duration: 2.0));

        tween.Update(1.0);

        Assert.AreEqual(5.0, tween.Value, delta: 1e-10);
    }

    [TestMethod]
    public void Update_AtCompletion_ValueIsEndValue()
    {
        var tween = InContainer(new NumTween(0.0, 10.0, duration: 1.0));

        tween.Update(1.0);

        Assert.AreEqual(10.0, tween.Value, delta: 1e-10);
    }

    [TestMethod]
    public void Update_NegativeRange_InterpolatesCorrectly()
    {
        var tween = InContainer(new NumTween(10.0, 0.0, duration: 2.0));

        tween.Update(1.0); // halfway

        Assert.AreEqual(5.0, tween.Value, delta: 1e-10);
    }

    [TestMethod]
    public void Update_WithEasing_ValueUsesEasedProgress()
    {
        // EaseInQuad: Progress = t² so at t=0.5, Progress = 0.25, Value = 0 + 10 * 0.25 = 2.5
        var tween = InContainer(new NumTween(0.0, 10.0, duration: 2.0, easing: Easings.EaseInQuad));

        tween.Update(1.0); // LinearProgress = 0.5, Progress = 0.25

        Assert.AreEqual(2.5, tween.Value, delta: 1e-10);
    }

    [TestMethod]
    public void Update_Loop_ValueResetsTowardsStartOnNextCycle()
    {
        var tween = InContainer(new NumTween(0.0, 10.0, duration: 1.0, loop: true));

        tween.Update(1.0); // completes — value at 10
        tween.Update(0.5); // halfway through next cycle

        Assert.AreEqual(5.0, tween.Value, delta: 1e-10);
    }
}

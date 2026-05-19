using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TweenTests
{
    // Minimal concrete subclass that counts OnProgressUpdated calls.
    private sealed class CapturingTween(double duration, bool loop = false, Func<double, double>? easing = null)
        : Tween(duration, loop, easing)
    {
        public int ProgressUpdateCount { get; private set; } = 0;

        protected override void OnProgressUpdated()
        {
            ProgressUpdateCount++;
        }
    }

    private sealed class FakeContainer : IContainer
    {
        public List<IAddable> RemovedFromHere { get; } = [];

        public void Add(IAddable entity) { }

        public void Remove(IAddable entity)
        {
            RemovedFromHere.Add(entity);
            entity.Parent = null;
        }

        public void RemoveAll() { }

        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private static CapturingTween InContainer(CapturingTween tween)
    {
        tween.Parent = new FakeContainer();
        return tween;
    }

    [TestMethod]
    public void Constructor_NegativeDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CapturingTween(-1.0));
    }

    [TestMethod]
    public void Constructor_ZeroDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CapturingTween(0.0));
    }

    [TestMethod]
    public void InitialState_ProgressAndLinearProgressAreZero()
    {
        var tween = new CapturingTween(1.0);

        Assert.AreEqual(0.0, tween.Progress);
        Assert.AreEqual(0.0, tween.LinearProgress);
    }

    [TestMethod]
    public void Update_Midway_LinearProgressIsHalf()
    {
        var tween = InContainer(new CapturingTween(2.0));

        tween.Update(1.0);

        Assert.AreEqual(0.5, tween.LinearProgress, delta: 1e-10);
    }

    [TestMethod]
    public void Update_NoEasing_ProgressEqualsLinearProgress()
    {
        var tween = InContainer(new CapturingTween(2.0));

        tween.Update(1.0);

        Assert.AreEqual(tween.LinearProgress, tween.Progress, delta: 1e-10);
    }

    [TestMethod]
    public void Update_WithEasing_ProgressIsEasedLinearProgress()
    {
        Func<double, double> easing = t => t * t;
        var tween = InContainer(new CapturingTween(2.0, easing: easing));

        tween.Update(1.0); // LinearProgress = 0.5

        Assert.AreEqual(0.5 * 0.5, tween.Progress, delta: 1e-10);
    }

    [TestMethod]
    public void Update_OnCompletion_ProgressIsOne()
    {
        var tween = InContainer(new CapturingTween(1.0));

        tween.Update(1.0);

        Assert.AreEqual(1.0, tween.Progress, delta: 1e-10);
        Assert.AreEqual(1.0, tween.LinearProgress, delta: 1e-10);
    }

    [TestMethod]
    public void Update_CallsOnProgressUpdatedEachFrame()
    {
        var tween = InContainer(new CapturingTween(2.0));

        tween.Update(0.5);
        tween.Update(0.5);

        Assert.AreEqual(2, tween.ProgressUpdateCount);
    }

    [TestMethod]
    public void Update_OnProgressUpdatedCalledBeforeCompletedFires()
    {
        // OnProgressUpdated sets Progress to 1 before Completed fires, so the
        // Completed handler sees the final value.
        double progressAtCompletion = -1;
        var tween = InContainer(new CapturingTween(1.0));
        tween.Completed += () => progressAtCompletion = tween.Progress;

        tween.Update(1.0);

        Assert.AreEqual(1.0, progressAtCompletion, delta: 1e-10);
    }

    [TestMethod]
    public void Update_OneShot_FiresCompletedOnCycleEnd()
    {
        int fired = 0;
        var tween = InContainer(new CapturingTween(1.0));
        tween.Completed += () => fired++;

        tween.Update(1.0);

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void Update_OneShot_RemovesItselfAfterCompletion()
    {
        var container = new FakeContainer();
        var tween = new CapturingTween(1.0) { Parent = container };

        tween.Update(1.0);

        Assert.HasCount(1, container.RemovedFromHere);
        Assert.IsFalse(tween.Exists);
    }

    [TestMethod]
    public void Update_Loop_DoesNotRemoveAfterCompletion()
    {
        var container = new FakeContainer();
        var tween = new CapturingTween(1.0, loop: true) { Parent = container };

        tween.Update(1.0);

        Assert.IsEmpty(container.RemovedFromHere);
        Assert.IsTrue(tween.Exists);
    }

    [TestMethod]
    public void Update_Loop_FiresCompletedOnEachCycle()
    {
        int fired = 0;
        var tween = InContainer(new CapturingTween(1.0, loop: true));
        tween.Completed += () => fired++;

        tween.Update(1.0);
        tween.Update(1.0);
        tween.Update(1.0);

        Assert.AreEqual(3, fired);
    }

    [TestMethod]
    public void Update_Loop_ResetsProgressAfterCycle()
    {
        var tween = InContainer(new CapturingTween(1.0, loop: true));

        tween.Update(1.0); // completes cycle
        tween.Update(0.5); // halfway through next cycle

        Assert.AreEqual(0.5, tween.LinearProgress, delta: 1e-10);
    }

    [TestMethod]
    public void Update_WhenInactive_DoesNotAdvanceProgress()
    {
        var tween = new CapturingTween(1.0) { Active = false };

        tween.Update(0.5);

        Assert.AreEqual(0.0, tween.Progress);
        Assert.AreEqual(0, tween.ProgressUpdateCount);
    }

    // TimeRemaining

    [TestMethod]
    public void TimeRemaining_AtStart_EqualsDuration()
    {
        var tween = new CapturingTween(4.0);

        Assert.AreEqual(4.0, tween.TimeRemaining, delta: 1e-10);
    }

    [TestMethod]
    public void TimeRemaining_Midway_IsHalfDuration()
    {
        var tween = InContainer(new CapturingTween(4.0));

        tween.Update(2.0);

        Assert.AreEqual(2.0, tween.TimeRemaining, delta: 1e-10);
    }

    [TestMethod]
    public void TimeRemaining_AtCompletion_IsZero()
    {
        var tween = InContainer(new CapturingTween(1.0));

        tween.Update(1.0);

        Assert.AreEqual(0.0, tween.TimeRemaining, delta: 1e-10);
    }

}

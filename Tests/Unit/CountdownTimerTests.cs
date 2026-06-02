using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CountdownTimerTests
{
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

    private static CountdownTimer InContainer(CountdownTimer timer)
    {
        timer.Parent = new FakeContainer();
        return timer;
    }

    // Constructor

    [TestMethod]
    public void Constructor_NegativeDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CountdownTimer(-1.0));
    }

    [TestMethod]
    public void Constructor_ZeroDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CountdownTimer(0.0));
    }

    [TestMethod]
    public void Constructor_ZeroTickInterval_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CountdownTimer(5.0, tickInterval: 0.0));
    }

    [TestMethod]
    public void Constructor_NegativeTickInterval_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CountdownTimer(5.0, tickInterval: -1.0));
    }

    [TestMethod]
    public void Constructor_OnCompleted_WiresCompletedEvent()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(1.0, onCompleted: () => fired++));

        timer.Update(1.0);

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void Constructor_OnTicked_WiresTickedEvent()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(3.0, onTicked: () => fired++));

        timer.Update(2.0);

        Assert.AreEqual(2, fired);
    }

    // Elapsed

    [TestMethod]
    public void Elapsed_AtStart_IsZero()
    {
        var timer = new CountdownTimer(5.0);

        Assert.AreEqual(0.0, timer.Elapsed, delta: 1e-10);
    }

    [TestMethod]
    public void Elapsed_AfterUpdate_ReflectsTimeAccumulated()
    {
        var timer = InContainer(new CountdownTimer(5.0));

        timer.Update(1.5);

        Assert.AreEqual(1.5, timer.Elapsed, delta: 1e-10);
    }

    // TimeRemaining

    [TestMethod]
    public void TimeRemaining_AtStart_EqualsDuration()
    {
        var timer = new CountdownTimer(5.0);

        Assert.AreEqual(5.0, timer.TimeRemaining, delta: 1e-10);
    }

    [TestMethod]
    public void TimeRemaining_Midway_IsHalfDuration()
    {
        var timer = InContainer(new CountdownTimer(4.0));

        timer.Update(2.0);

        Assert.AreEqual(2.0, timer.TimeRemaining, delta: 1e-10);
    }

    [TestMethod]
    public void TimeRemaining_AfterCompletion_IsZero()
    {
        var timer = InContainer(new CountdownTimer(1.0));

        timer.Update(2.0);

        Assert.AreEqual(0.0, timer.TimeRemaining, delta: 1e-10);
    }

    // Completed

    [TestMethod]
    public void Update_BeforeDurationElapsed_DoesNotFireCompleted()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(2.0));
        timer.Completed += () => fired++;

        timer.Update(1.0);

        Assert.AreEqual(0, fired);
    }

    [TestMethod]
    public void Update_AtDurationElapsed_FiresCompleted()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(1.0));
        timer.Completed += () => fired++;

        timer.Update(1.0);

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void Update_BeyondDurationElapsed_FiresCompletedOnce()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(1.0));
        timer.Completed += () => fired++;

        timer.Update(5.0);

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void Update_RemovesItselfAfterCompleted()
    {
        var container = new FakeContainer();
        var timer = new CountdownTimer(1.0) { Parent = container };

        timer.Update(1.0);

        Assert.HasCount(1, container.RemovedFromHere);
        Assert.AreSame(timer, container.RemovedFromHere[0]);
        Assert.IsFalse(timer.Exists);
    }

    // Ticked

    [TestMethod]
    public void Update_Ticked_FiresAtEachIntervalBoundary()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(5.0, tickInterval: 1.0));
        timer.Ticked += () => fired++;

        timer.Update(1.0);
        timer.Update(1.0);
        timer.Update(1.0);

        Assert.AreEqual(3, fired);
    }

    [TestMethod]
    public void Update_Ticked_MultipleTicksInSingleLargeFrame()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(10.0, tickInterval: 1.0));
        timer.Ticked += () => fired++;

        timer.Update(3.5);

        Assert.AreEqual(3, fired);
    }

    [TestMethod]
    public void Update_Ticked_DoesNotFireAtDurationBoundary()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(3.0, tickInterval: 1.0));
        timer.Ticked += () => fired++;

        timer.Update(3.0);

        // ticks at t=1 and t=2 only; t=3 coincides with Duration so Completed fires instead
        Assert.AreEqual(2, fired);
    }

    [TestMethod]
    public void Update_Ticked_IntervalLargerThanDuration_NeverFires()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(1.0, tickInterval: 5.0));
        timer.Ticked += () => fired++;

        timer.Update(1.0);

        Assert.AreEqual(0, fired);
    }

    [TestMethod]
    public void Update_Ticked_FiresBeforeCompleted_WhenBothOccurSameFrame()
    {
        List<string> order = [];
        var timer = InContainer(new CountdownTimer(2.0, tickInterval: 1.0));
        timer.Ticked += () => order.Add("tick");
        timer.Completed += () => order.Add("completed");

        timer.Update(2.0);

        // tick at t=1, then Completed at t=2; tick does NOT fire at t=2
        Assert.AreEqual(2, order.Count);
        Assert.AreEqual("tick", order[0]);
        Assert.AreEqual("completed", order[1]);
    }

    [TestMethod]
    public void Update_AfterCompletion_DoesNotFireCompletedAgain()
    {
        int fired = 0;
        var timer = InContainer(new CountdownTimer(1.0));
        timer.Completed += () => fired++;

        timer.Update(1.0); // completes
        timer.Update(1.0); // already done — should be a no-op
        timer.Update(1.0); // ditto

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void Update_NoParent_DoesNotThrow()
    {
        // A timer never attached to a parent should still complete cleanly without throwing
        // from Remove(). Useful for fire-and-forget timers driven manually.
        var timer = new CountdownTimer(1.0);
        int fired = 0;
        timer.Completed += () => fired++;

        timer.Update(1.0);

        Assert.AreEqual(1, fired);
    }
}

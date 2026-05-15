using System;
using System.Collections.Generic;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class AlarmTests
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

    private static Alarm InContainer(Alarm alarm)
    {
        alarm.Parent = new FakeContainer();
        return alarm;
    }

    [TestMethod]
    public void Update_BeforeDurationElapsed_DoesNotFireElapsed()
    {
        int fired = 0;
        var alarm = InContainer(new Alarm(1.0));
        alarm.Elapsed += () => fired++;

        alarm.Update(0.5);

        Assert.AreEqual(0, fired);
    }

    [TestMethod]
    public void Update_AtDurationElapsed_FiresElapsed()
    {
        int fired = 0;
        var alarm = InContainer(new Alarm(1.0));
        alarm.Elapsed += () => fired++;

        alarm.Update(1.0);

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void Update_BeyondDurationElapsed_FiresElapsed()
    {
        int fired = 0;
        var alarm = InContainer(new Alarm(1.0));
        alarm.Elapsed += () => fired++;

        alarm.Update(2.0);

        Assert.AreEqual(1, fired);
    }

    [TestMethod]
    public void Update_OneShot_RemovesItselfAfterFiring()
    {
        var container = new FakeContainer();
        var alarm = new Alarm(1.0) { Parent = container };

        alarm.Update(1.0);

        Assert.HasCount(1, container.RemovedFromHere);
        Assert.AreSame(alarm, container.RemovedFromHere[0]);
        Assert.IsFalse(alarm.Exists);
    }

    [TestMethod]
    public void Update_Repeating_DoesNotRemoveAfterFiring()
    {
        var container = new FakeContainer();
        var alarm = new Alarm(1.0, repeating: true) { Parent = container };

        alarm.Update(1.0);

        Assert.IsEmpty(container.RemovedFromHere);
        Assert.IsTrue(alarm.Exists);
    }

    [TestMethod]
    public void Update_Repeating_FiresMultipleTimesOverMultipleUpdates()
    {
        int fired = 0;
        var alarm = InContainer(new Alarm(1.0, repeating: true));
        alarm.Elapsed += () => fired++;

        alarm.Update(1.0);
        alarm.Update(1.0);
        alarm.Update(1.0);

        Assert.AreEqual(3, fired);
    }

    [TestMethod]
    public void Update_Repeating_CarriesOvershootIntoNextInterval()
    {
        int fired = 0;
        var alarm = InContainer(new Alarm(1.0, repeating: true));
        alarm.Elapsed += () => fired++;

        alarm.Update(1.5);
        alarm.Update(0.5);

        Assert.AreEqual(2, fired);
    }

    [TestMethod]
    public void Update_WhenInactive_DoesNotFireElapsed()
    {
        int fired = 0;
        var alarm = InContainer(new Alarm(1.0) { Active = false });
        alarm.Elapsed += () => fired++;

        alarm.Update(2.0);

        Assert.AreEqual(0, fired);
    }

    [TestMethod]
    public void Constructor_WithCallback_WiresElapsedEvent()
    {
        int fired = 0;
        var alarm = InContainer(new Alarm(1.0, onElapsed: () => fired++));

        alarm.Update(1.0);

        Assert.AreEqual(1, fired);
    }
}

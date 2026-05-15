using BabyBearsEngine.Tasks;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class WaitTaskTests
{
    [TestMethod]
    public void NewWaitTask_IsNotComplete()
    {
        var task = new WaitTask(1.0);

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void Update_BelowDuration_IsNotComplete()
    {
        var task = new WaitTask(1.0);

        task.Update(0.5);

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void Update_ExactlyDuration_IsComplete()
    {
        var task = new WaitTask(1.0);

        task.Update(1.0);

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Update_BeyondDuration_IsComplete()
    {
        var task = new WaitTask(1.0);

        task.Update(2.0);

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Update_AccumulatesAcrossMultipleCalls()
    {
        var task = new WaitTask(1.0);

        task.Update(0.4);
        task.Update(0.4);

        Assert.IsFalse(task.IsComplete);

        task.Update(0.2);

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Reset_ClearsElapsed_SoTaskIsIncompleteAgain()
    {
        var task = new WaitTask(1.0);
        task.Update(1.0);
        Assert.IsTrue(task.IsComplete);

        task.Reset();

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void Reset_AllowsTaskToCompleteAgain()
    {
        var task = new WaitTask(1.0);
        task.Update(1.0);
        task.Reset();

        task.Update(1.0);

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Update_FirstCall_FiresTaskStarted()
    {
        var task = new WaitTask(1.0);
        bool started = false;
        task.TaskStarted += (_, _) => started = true;

        task.Update(0.016);

        Assert.IsTrue(started);
    }
}

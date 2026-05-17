using System;
using BabyBearsEngine.Tasks;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TaskTests
{
    [TestMethod]
    public void NewTask_HasNoCompletionConditions_IsCompleteImmediately()
    {
        // With zero completion conditions, IsComplete is the empty `.All()` which returns true.
        var task = new Task();

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Update_FirstCall_FiresTaskStarted()
    {
        var task = new Task();
        bool started = false;
        task.TaskStarted += (_, _) => started = true;

        task.Update(0.016);

        Assert.IsTrue(started);
    }

    [TestMethod]
    public void Update_SecondCall_DoesNotFireTaskStartedAgain()
    {
        var task = new Task();
        int starts = 0;
        task.TaskStarted += (_, _) => starts++;

        task.Update(0.016);
        task.Update(0.016);

        Assert.AreEqual(1, starts);
    }

    [TestMethod]
    public void Update_WhenIsCompleteOnFirstTick_AutoCallsComplete()
    {
        var task = new Task();
        bool completed = false;
        task.TaskCompleted += (_, _) => completed = true;

        task.Update(0.016);

        Assert.IsTrue(completed);
    }

    [TestMethod]
    public void Update_AfterAutoComplete_DoesNotRestartTask()
    {
        var task = new Task();
        int starts = 0;
        task.TaskStarted += (_, _) => starts++;

        task.Update(0.016);
        task.Update(0.016);

        Assert.AreEqual(1, starts);
    }

    [TestMethod]
    public void Complete_RunsActionOnCompleteAndFiresEvent()
    {
        bool ran = false;
        var task = new Task(actionOnComplete: () => ran = true);
        bool completed = false;
        task.TaskCompleted += (_, _) => completed = true;

        task.Complete();

        Assert.IsTrue(ran);
        Assert.IsTrue(completed);
    }

    [TestMethod]
    public void Reset_ClearsStartedState_SoUpdateFiresStartedAgain()
    {
        var task = new Task();
        int starts = 0;
        task.TaskStarted += (_, _) => starts++;

        task.Update(0.016);
        task.Reset();
        task.Update(0.016);

        Assert.AreEqual(2, starts);
    }

    [TestMethod]
    public void DoNothing_StaticFactory_ReturnsACompletedTask()
    {
        Assert.IsTrue(Task.DoNothing.IsComplete);
    }

    [TestMethod]
    public void Reset_CallsOnReset()
    {
        bool onResetCalled = false;
        var task = new Task();
        // Use a subclass to verify the hook fires.
        var hookTask = new OnResetHookTask(() => onResetCalled = true);

        hookTask.Reset();

        Assert.IsTrue(onResetCalled);
    }

    [TestMethod]
    public void Reset_ClearsStartedState_AndCallsOnReset()
    {
        int starts = 0;
        int resets = 0;
        var task = new OnResetHookTask(() => resets++);
        task.TaskStarted += (_, _) => starts++;

        task.Update(0.016);
        task.Reset();
        task.Update(0.016);

        Assert.AreEqual(2, starts);
        Assert.AreEqual(1, resets);
    }

    private sealed class OnResetHookTask(Action onReset) : Task
    {
        protected override void OnReset() => onReset();
    }
}

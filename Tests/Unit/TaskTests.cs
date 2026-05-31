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
    public void Complete_CalledTwice_RunsActionOnCompleteOnlyOnce()
    {
        int runCount = 0;
        var task = new Task(actionOnComplete: () => runCount++);

        task.Complete();
        task.Complete();

        Assert.AreEqual(1, runCount);
    }

    [TestMethod]
    public void Complete_CalledTwice_FiresTaskCompletedOnlyOnce()
    {
        var task = new Task();
        int completedCount = 0;
        task.TaskCompleted += (_, _) => completedCount++;

        task.Complete();
        task.Complete();

        Assert.AreEqual(1, completedCount);
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

    // Cancel

    [TestMethod]
    public void Cancel_FiresTaskCancelledEvent()
    {
        var task = new Task();
        bool cancelled = false;
        task.TaskCancelled += (_, _) => cancelled = true;

        task.Cancel();

        Assert.IsTrue(cancelled);
    }

    [TestMethod]
    public void Cancel_DoesNotFireTaskCompletedEvent()
    {
        var task = new Task();
        bool completed = false;
        task.TaskCompleted += (_, _) => completed = true;

        task.Cancel();

        Assert.IsFalse(completed);
    }

    [TestMethod]
    public void Cancel_DoesNotRunActionsOnComplete()
    {
        bool ranOnComplete = false;
        var task = new Task(actionOnComplete: () => ranOnComplete = true);

        task.Cancel();

        Assert.IsFalse(ranOnComplete);
    }

    [TestMethod]
    public void Cancel_CallsOnCancelHook()
    {
        int onCancelCalls = 0;
        var task = new OnCancelHookTask(() => onCancelCalls++);

        task.Cancel();

        Assert.AreEqual(1, onCancelCalls);
    }

    [TestMethod]
    public void Cancel_PreventsFurtherUpdates()
    {
        var task = new OnCancelHookTask(() => { });
        int starts = 0;
        task.TaskStarted += (_, _) => starts++;

        task.Cancel();
        task.Update(0.016);
        task.Update(0.016);

        Assert.AreEqual(0, starts);
    }

    [TestMethod]
    public void Cancel_AfterComplete_IsNoOp()
    {
        // A task that has already completed cannot then be cancelled — Cancel is a no-op.
        var task = new Task();
        task.Update(0.016); // empty task auto-completes on first update
        int cancelEvents = 0;
        int onCancelCalls = 0;
        task.TaskCancelled += (_, _) => cancelEvents++;
        var hooked = new OnCancelHookTask(() => onCancelCalls++);
        hooked.Update(0.016);

        task.Cancel();
        hooked.Cancel();

        Assert.AreEqual(0, cancelEvents);
        Assert.AreEqual(0, onCancelCalls);
    }

    [TestMethod]
    public void Cancel_IsIdempotent_TaskCancelledFiresOnlyOnce()
    {
        var task = new Task();
        int cancelEvents = 0;
        task.TaskCancelled += (_, _) => cancelEvents++;

        task.Cancel();
        task.Cancel();
        task.Cancel();

        Assert.AreEqual(1, cancelEvents);
    }

    [TestMethod]
    public void Reset_AfterCancel_AllowsTaskToRunAgain()
    {
        var task = new Task();
        int starts = 0;
        task.TaskStarted += (_, _) => starts++;

        task.Cancel();
        task.Reset();
        task.Update(0.016);

        Assert.AreEqual(1, starts);
    }

    private sealed class OnCancelHookTask(Action onCancel) : Task
    {
        protected override void OnCancel() => onCancel();
    }

    private sealed class OnResetHookTask(Action onReset) : Task
    {
        protected override void OnReset() => onReset();
    }
}

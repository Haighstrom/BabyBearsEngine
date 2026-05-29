using System;
using BabyBearsEngine.Tasks;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TaskGroupTests
{
    /// <summary>A simple controllable task: completes when its <c>ShouldComplete</c> flag flips to true.</summary>
    private sealed class ControllableTask : Task
    {
        public bool ShouldComplete { get; set; } = false;
        public int CompleteCalls { get; private set; }

        public ControllableTask()
        {
            CompletionConditions.Add(() => ShouldComplete);
            ActionsOnComplete.Add(() => CompleteCalls++);
        }
    }

    private sealed class OnCancelHookTask(Action onCancel) : Task
    {
        protected override void OnCancel() => onCancel();
    }

    private sealed class OnCancelHookGroup(Action onCancel) : TaskGroup
    {
        protected override void OnCancel() => onCancel();
    }

    [TestMethod]
    public void EmptyGroup_HasNullCurrentTask_AndIsComplete()
    {
        var group = new TaskGroup();

        Assert.IsNull(group.CurrentTask);
        Assert.IsTrue(group.IsComplete);
    }

    [TestMethod]
    public void Constructor_WithTasks_ChainsThemAndStartsAtFirst()
    {
        var a = new ControllableTask();
        var b = new ControllableTask();
        var c = new ControllableTask();

        var group = new TaskGroup(a, b, c);

        Assert.AreSame(a, group.CurrentTask);
        Assert.AreSame(b, a.NextTask);
        Assert.AreSame(c, b.NextTask);
        Assert.IsNull(c.NextTask);
    }

    [TestMethod]
    public void Update_AdvancesToNextTaskWhenCurrentCompletes()
    {
        var a = new ControllableTask();
        var b = new ControllableTask();
        var group = new TaskGroup(a, b);

        group.Update(0.016);
        Assert.AreSame(a, group.CurrentTask);

        a.ShouldComplete = true;
        group.Update(0.016);

        Assert.AreSame(b, group.CurrentTask);
    }

    [TestMethod]
    public void IsComplete_FalseWhileChainRunning_TrueOnceExhausted()
    {
        var a = new ControllableTask();
        var group = new TaskGroup(a);

        Assert.IsFalse(group.IsComplete);

        a.ShouldComplete = true;
        group.Update(0.016);

        Assert.IsTrue(group.IsComplete);
    }

    [TestMethod]
    public void LastTask_WalksTheChain()
    {
        var a = new ControllableTask();
        var b = new ControllableTask();
        var c = new ControllableTask();
        var group = new TaskGroup(a, b, c);

        Assert.AreSame(c, group.LastTask);
    }

    [TestMethod]
    public void AddTasks_ToEmptyGroup_SetsCurrentToFirstNew()
    {
        var group = new TaskGroup();
        var a = new ControllableTask();
        var b = new ControllableTask();

        group.AddTasks(a, b);

        Assert.AreSame(a, group.CurrentTask);
        Assert.AreSame(b, a.NextTask);
    }

    [TestMethod]
    public void AddTasks_ToNonEmptyGroup_AppendsAfterLast()
    {
        var a = new ControllableTask();
        var b = new ControllableTask();
        var group = new TaskGroup(a);

        group.AddTasks(b);

        Assert.AreSame(b, a.NextTask);
    }

    [TestMethod]
    public void Update_CompletesEachChildExactlyOnce()
    {
        // Regression: a plain Task inside a TaskGroup must have Complete() called exactly once.
        // Task.Update already self-completes, so a TaskGroup that also completed its children
        // ran every non-idempotent Complete() override (e.g. CarryItemTask) twice.
        var a = new ControllableTask { ShouldComplete = true };
        var b = new ControllableTask { ShouldComplete = true };
        var group = new TaskGroup(a, b);

        group.Update(0.016);
        group.Update(0.016);

        Assert.AreEqual(1, a.CompleteCalls);
        Assert.AreEqual(1, b.CompleteCalls);
    }

    [TestMethod]
    public void Update_CompletesItselfWhenChainExhausted()
    {
        // A TaskGroup is also a task: it must complete itself once its chain is exhausted,
        // because its parent controller/group does not call Complete() on it.
        var a = new ControllableTask { ShouldComplete = true };
        var group = new TaskGroup(a);
        int groupCompletions = 0;
        group.TaskCompleted += (_, _) => groupCompletions++;

        group.Update(0.016);

        Assert.AreEqual(1, groupCompletions);
    }

    [TestMethod]
    public void Update_NestedTaskGroup_CompletesInnerGroupExactlyOnce()
    {
        var innerTask = new ControllableTask { ShouldComplete = true };
        var innerGroup = new TaskGroup(innerTask);
        int innerGroupCompletions = 0;
        innerGroup.TaskCompleted += (_, _) => innerGroupCompletions++;
        var outerGroup = new TaskGroup(innerGroup);

        outerGroup.Update(0.016);
        outerGroup.Update(0.016);

        Assert.AreEqual(1, innerGroupCompletions);
        Assert.AreEqual(1, innerTask.CompleteCalls);
    }

    [TestMethod]
    public void Update_AfterSelfCompleting_DoesNotRestartOrRecomplete()
    {
        var a = new ControllableTask { ShouldComplete = true };
        var group = new TaskGroup(a);
        int starts = 0;
        int completions = 0;
        group.TaskStarted += (_, _) => starts++;
        group.TaskCompleted += (_, _) => completions++;

        group.Update(0.016);
        group.Update(0.016);
        group.Update(0.016);

        Assert.AreEqual(1, starts);
        Assert.AreEqual(1, completions);
        Assert.AreEqual(1, a.CompleteCalls);
    }

    // Cancel

    [TestMethod]
    public void Cancel_FiresOwnTaskCancelledEvent()
    {
        var a = new ControllableTask();
        var group = new TaskGroup(a);
        int cancelEvents = 0;
        group.TaskCancelled += (_, _) => cancelEvents++;

        group.Cancel();

        Assert.AreEqual(1, cancelEvents);
    }

    [TestMethod]
    public void Cancel_CallsOnCancelHook()
    {
        int onCancelCalls = 0;
        var group = new OnCancelHookGroup(() => onCancelCalls++);

        group.Cancel();

        Assert.AreEqual(1, onCancelCalls);
    }

    [TestMethod]
    public void Cancel_CancelsCurrentTaskAndQueuedChain()
    {
        // Queued tasks may hold construction-time reservations that need releasing too —
        // Cancel must propagate through the full remaining chain, not just the current task.
        int aCancels = 0;
        int bCancels = 0;
        int cCancels = 0;
        var a = new OnCancelHookTask(() => aCancels++);
        var b = new OnCancelHookTask(() => bCancels++);
        var c = new OnCancelHookTask(() => cCancels++);
        var group = new TaskGroup(a, b, c);

        group.Cancel();

        Assert.AreEqual(1, aCancels);
        Assert.AreEqual(1, bCancels);
        Assert.AreEqual(1, cCancels);
    }

    [TestMethod]
    public void Cancel_SetsCurrentTaskToNull()
    {
        var a = new ControllableTask();
        var group = new TaskGroup(a);

        group.Cancel();

        Assert.IsNull(group.CurrentTask);
    }

    [TestMethod]
    public void Cancel_DoesNotFireTaskCompletedEvent()
    {
        var a = new ControllableTask();
        var group = new TaskGroup(a);
        bool completed = false;
        group.TaskCompleted += (_, _) => completed = true;

        group.Cancel();

        Assert.IsFalse(completed);
    }

    [TestMethod]
    public void Cancel_AfterGroupCompletes_IsNoOp()
    {
        var a = new ControllableTask { ShouldComplete = true };
        var group = new TaskGroup(a);
        group.Update(0.016); // group completes
        int cancelEvents = 0;
        group.TaskCancelled += (_, _) => cancelEvents++;

        group.Cancel();

        Assert.AreEqual(0, cancelEvents);
    }

    [TestMethod]
    public void Reset_ClearsCompletedState_SoUpdateRunsAgain()
    {
        var a = new ControllableTask { ShouldComplete = true };
        var group = new TaskGroup(a);
        int starts = 0;
        group.TaskStarted += (_, _) => starts++;

        group.Update(0.016); // completes the group
        group.Reset();
        group.Update(0.016); // _isCompleted cleared, so this runs (and fires Started) again

        Assert.AreEqual(2, starts);
    }
}

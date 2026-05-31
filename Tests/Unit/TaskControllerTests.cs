using System;
using System.Collections.Generic;
using BabyBearsEngine.Tasks;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TaskControllerTests
{
    private sealed class ControllableTask : Task
    {
        public bool ShouldComplete { get; set; } = false;
        public int UpdateCalls { get; private set; }
        public int CompleteCalls { get; private set; }

        public ControllableTask()
        {
            CompletionConditions.Add(() => ShouldComplete);
            ActionsOnComplete.Add(() => CompleteCalls++);
        }

        public override void Update(double elapsed)
        {
            base.Update(elapsed);
            UpdateCalls++;
        }
    }

    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    // An entity-like object that sits inside a world (Parent != null = in world).
    private sealed class FakeAddableContainer : IContainer, IAddable
    {
        public IContainer? Parent { get; set; }
        public bool Exists => Parent is not null;
        public event EventHandler? Added
        {
            add { }
            remove { }
        }
        public event EventHandler? Removed
        {
            add { }
            remove { }
        }
        public void Remove() => Parent = null;
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    [TestMethod]
    public void Update_WhenNoParent_DoesNothing()
    {
        var task = new ControllableTask();
        var controller = new TaskController(task);

        controller.Update(0.016);

        Assert.AreEqual(0, task.UpdateCalls);
    }

    [TestMethod]
    public void Update_WithParent_DrivesCurrentTask()
    {
        var task = new ControllableTask();
        var controller = new TaskController(task)
        {
            Parent = new FakeContainer(),
        };

        controller.Update(0.016);

        Assert.AreEqual(1, task.UpdateCalls);
    }

    [TestMethod]
    public void Update_WhenCurrentTaskCompletes_AdvancesToNext()
    {
        var a = new ControllableTask { ShouldComplete = true };
        var b = new ControllableTask();
        var controller = new TaskController([a, b])
        {
            Parent = new FakeContainer(),
        };

        controller.Update(0.016);

        Assert.AreEqual(1, a.CompleteCalls);
        Assert.AreSame(b, controller.CurrentTask);
    }

    [TestMethod]
    public void Update_CompletesTaskGroupChildExactlyOnce()
    {
        // A TaskGroup directly under a TaskController must still be completed exactly once.
        // Previously it was never completed: TaskController does not call Complete(), and
        // TaskGroup did not self-complete — so a top-level group's Complete() never ran.
        var innerTask = new ControllableTask { ShouldComplete = true };
        var group = new TaskGroup(innerTask);
        int groupCompletions = 0;
        group.TaskCompleted += (_, _) => groupCompletions++;
        var controller = new TaskController(group)
        {
            Parent = new FakeContainer(),
        };

        controller.Update(0.016);

        Assert.AreEqual(1, groupCompletions);
        Assert.AreEqual(1, innerTask.CompleteCalls);
    }

    [TestMethod]
    public void Update_WhenChainExhausted_CallsGetNextTask()
    {
        var first = new ControllableTask { ShouldComplete = true };
        var next = new ControllableTask();
        int fetchCalls = 0;

        var controller = new TaskController(first, () =>
        {
            fetchCalls++;
            return next;
        })
        {
            Parent = new FakeContainer(),
        };

        controller.Update(0.016);

        Assert.AreEqual(1, fetchCalls);
        Assert.AreSame(next, controller.CurrentTask);
    }

    [TestMethod]
    public void Update_CreateTaskTask_RunsInsertedTaskBeforeOriginalNext()
    {
        // End-to-end check that CreateTaskTask integrates with the controller's chain-advancement
        // logic: the splice happens during Start, then the controller walks through the inserted
        // task and the originally-queued next task in the right order. The splice mechanics
        // themselves are pinned in CreateTaskTaskTests.
        var inserted = new ControllableTask { ShouldComplete = true };
        var originalNext = new ControllableTask { ShouldComplete = true };
        var splicer = new CreateTaskTask(() => inserted)
        {
            NextTask = originalNext,
        };

        var executionOrder = new List<string>();
        splicer.TaskStarted += (_, _) => executionOrder.Add("splicer");
        inserted.TaskStarted += (_, _) => executionOrder.Add("inserted");
        originalNext.TaskStarted += (_, _) => executionOrder.Add("originalNext");

        var controller = new TaskController(splicer)
        {
            Parent = new FakeContainer(),
        };

        // Splicer's first update fires Start (which performs the splice), then auto-completes
        // (it has no completion conditions). After three updates the chain has fully drained.
        controller.Update(0.016);
        controller.Update(0.016);
        controller.Update(0.016);

        CollectionAssert.AreEqual(new[] { "splicer", "inserted", "originalNext" }, executionOrder);
        Assert.AreSame(inserted, splicer.NextTask);
        Assert.AreSame(originalNext, inserted.NextTask);
        Assert.AreEqual(1, inserted.CompleteCalls);
        Assert.AreEqual(1, originalNext.CompleteCalls);
        Assert.IsNull(controller.CurrentTask);
    }

    [TestMethod]
    public void ClearTask_SetsCurrentTaskToNull()
    {
        var task = new ControllableTask();
        var controller = new TaskController(task);

        controller.ClearTask();

        Assert.IsNull(controller.CurrentTask);
    }

    [TestMethod]
    public void ClearTask_CancelsCurrentTask()
    {
        // ClearTask must give the in-flight task a chance to clean up (release reservations,
        // etc.) before being detached. Documents the OnCancel auto-trigger contract.
        int onCancelCalls = 0;
        var task = new OnCancelHookTask(() => onCancelCalls++);
        var controller = new TaskController(task);

        controller.ClearTask();

        Assert.AreEqual(1, onCancelCalls);
    }

    [TestMethod]
    public void ClearTask_WithNoCurrentTask_IsSafe()
    {
        var controller = new TaskController();

        controller.ClearTask();

        Assert.IsNull(controller.CurrentTask);
    }

    [TestMethod]
    public void Update_WhenParentDetachedFromWorld_DoesNotDriveTask()
    {
        var task = new ControllableTask();
        var world = new FakeContainer();
        var entity = new FakeAddableContainer { Parent = world };
        var controller = new TaskController(task) { Parent = entity };

        entity.Parent = null; // simulate removal from world mid-frame
        controller.Update(0.016);

        Assert.AreEqual(0, task.UpdateCalls);
    }

    [TestMethod]
    public void Update_WhenParentDetachedFromWorld_CancelsCurrentTask()
    {
        // Mirror of the in-world ClearTask path: when the controller's entity is removed
        // mid-flight, the current task must still get a chance to clean up.
        int onCancelCalls = 0;
        var task = new OnCancelHookTask(() => onCancelCalls++);
        var world = new FakeContainer();
        var entity = new FakeAddableContainer { Parent = world };
        var controller = new TaskController(task) { Parent = entity };

        entity.Parent = null;
        controller.Update(0.016);

        Assert.AreEqual(1, onCancelCalls);
        Assert.IsNull(controller.CurrentTask);
    }

    private sealed class OnCancelHookTask(Action onCancel) : Task
    {
        protected override void OnCancel() => onCancel();
    }
}

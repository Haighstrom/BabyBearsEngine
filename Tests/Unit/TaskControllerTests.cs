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

    // Mirrors HappyBlacksmith's CreateTaskTask pattern: on Start, save the existing NextTask,
    // overwrite NextTask with a newly-created task, then re-attach the original after the new one.
    // The linked-list NextTask model exists specifically to support this kind of dynamic mid-chain
    // insertion — see issue #64 for the discussion of why we keep it.
    private sealed class SpliceOnStartTask(Func<ITask> taskCreator) : Task
    {
        public override void Start()
        {
            base.Start();

            ITask? existingNextTask = NextTask;

            NextTask = taskCreator();

            if (existingNextTask is not null)
            {
                NextTask.NextTask = existingNextTask;
            }
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
    public void Update_TaskThatSplicesNextTaskOnStart_RunsInsertedTaskBeforeOriginalNext()
    {
        // Documents and pins the dynamic mid-chain insertion supported by ITask.NextTask.
        // A task may, during its own Start(), overwrite NextTask with a newly-created task and
        // re-attach the originally-queued next task behind it — turning [A → C] into [A → B → C]
        // at runtime. HappyBlacksmith's CreateTaskTask relies on exactly this behaviour. See
        // issue #64.
        var inserted = new ControllableTask { ShouldComplete = true };
        var originalNext = new ControllableTask { ShouldComplete = true };
        var splicer = new SpliceOnStartTask(() => inserted);
        splicer.NextTask = originalNext;

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
}

using System;
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
    public void ClearTask_SetsCurrentTaskToNull()
    {
        var task = new ControllableTask();
        var controller = new TaskController(task);

        controller.ClearTask();

        Assert.IsNull(controller.CurrentTask);
    }
}

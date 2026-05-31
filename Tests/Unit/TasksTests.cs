using BabyBearsEngine.Tasks;
using BabyBearsEngine.Worlds;
using static BabyBearsEngine.Tasks.Tasks;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TasksTests
{
    // Run

    [TestMethod]
    public void Run_CompletesOnFirstUpdate()
    {
        ITask task = Run(() => { });

        task.Update(0.016);

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Run_ExecutesActionOnComplete()
    {
        bool ran = false;
        ITask task = Run(() => ran = true);

        task.Update(0.016);

        Assert.IsTrue(ran);
    }

    // WaitFor

    [TestMethod]
    public void WaitFor_DoesNotCompleteWhileConditionIsFalse()
    {
        ITask task = WaitFor(() => false);

        task.Update(0.016);

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void WaitFor_CompletesWhenConditionBecomesTrue()
    {
        bool condition = false;
        ITask task = WaitFor(() => condition);
        task.Update(0.016);

        condition = true;

        Assert.IsTrue(task.IsComplete);
    }

    // Delay

    [TestMethod]
    public void Delay_DoesNotCompleteBeforeDurationElapses()
    {
        ITask task = Delay(1.0);

        task.Update(0.5);

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void Delay_CompletesAfterDurationElapses()
    {
        ITask task = Delay(1.0);

        task.Update(0.5);
        task.Update(0.6); // total elapsed > 1.0

        Assert.IsTrue(task.IsComplete);
    }

    // Remove

    [TestMethod]
    public void Remove_OnComplete_RemovesAddableFromParent()
    {
        var addable = new FakeAddable { Parent = new FakeContainer() };
        ITask task = Remove(addable);

        task.Update(0.016);

        Assert.IsNull(addable.Parent);
    }

    [TestMethod]
    public void Remove_DoesNotRemoveBeforeCompletion()
    {
        // The Remove() call is deferred to ActionsOnComplete — constructing the task must not
        // touch the addable.
        var parent = new FakeContainer();
        var addable = new FakeAddable { Parent = parent };

        _ = Remove(addable);

        Assert.AreSame(parent, addable.Parent);
    }

    // Then

    [TestMethod]
    public void Then_SetsNextTaskOnFirst()
    {
        ITask a = Run(() => { });
        ITask b = Run(() => { });

        a.Then(b);

        Assert.AreSame(b, a.NextTask);
    }

    [TestMethod]
    public void Then_ReturnsFirst_ForFluentChaining()
    {
        ITask a = Run(() => { });
        ITask b = Run(() => { });

        ITask result = a.Then(b);

        Assert.AreSame(a, result);
    }

    [TestMethod]
    public void Then_AppendsToTailOfExistingChain()
    {
        ITask a = Run(() => { });
        ITask b = Run(() => { });
        ITask c = Run(() => { });

        a.Then(b).Then(c);

        Assert.AreSame(b, a.NextTask);
        Assert.AreSame(c, b.NextTask);
    }

    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private sealed class FakeAddable : IAddable
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
    }
}

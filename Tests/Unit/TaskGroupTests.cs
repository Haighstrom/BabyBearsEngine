using System.Collections.Generic;
using BabyBearsEngine.Tasks;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class TaskGroupTests
{
    /// <summary>A simple controllable task: completes when its <c>ShouldComplete</c> flag flips to true.</summary>
    private sealed class ControllableTask : Task
    {
        public bool ShouldComplete { get; set; } = false;

        public ControllableTask()
        {
            CompletionConditions.Add(() => ShouldComplete);
        }
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
}

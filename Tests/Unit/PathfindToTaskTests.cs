using BabyBearsEngine.Geometry;
using BabyBearsEngine.Pathfinding;
using BabyBearsEngine.Tasks;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PathfindToTaskTests
{
    private sealed class FakeNode(float x, float y) : IPathfindNode<FakeNode>, IPosition
    {
        public float X { get; } = x;
        public float Y { get; } = y;
        public float DistanceBetweenConnectedNodes => 1f;
        public FakeNode? ParentNode { get; set; }
        public object? GraphSearchData { get; set; }
        public IList<FakeNode> ConnectedNodes { get; } = [];

        public bool Equals(FakeNode? other) => ReferenceEquals(this, other);
    }

    private sealed class FakePathingController : IPathingController<FakeNode>
    {
        public bool ShouldFindPath { get; set; } = true;
        public int TryFindPathCalls { get; private set; }
        public int ClearPathCalls { get; private set; }
        public FakeNode? LastStart { get; private set; }
        public FakeNode? LastGoal { get; private set; }

        public bool HasPath { get; private set; }
        public Func<FakeNode, FakeNode, bool> PassableTest { get; set; } = (_, _) => true;

        public IContainer? Parent { get; set; }
        public bool Exists => Parent is not null;
        public bool Active { get; set; } = true;

        public event EventHandler? Arrived;
        public event EventHandler? PathFound;
        public event EventHandler? PathNotFound;
        public event EventHandler? Added;
        public event EventHandler? Removed;

        public void ClearPath()
        {
            ClearPathCalls++;
            HasPath = false;
        }

        public void Remove() => Parent = null;

        public bool TryFindPath(FakeNode start, FakeNode goal)
        {
            TryFindPathCalls++;
            LastStart = start;
            LastGoal = goal;

            if (ShouldFindPath)
            {
                HasPath = true;
                PathFound?.Invoke(this, EventArgs.Empty);
                return true;
            }

            PathNotFound?.Invoke(this, EventArgs.Empty);
            return false;
        }

        public void Update(double elapsed) { }

        public void FireArrived() => Arrived?.Invoke(this, EventArgs.Empty);
    }

    [TestMethod]
    public void Update_FirstCall_CallsTryFindPath()
    {
        var controller = new FakePathingController();
        var start = new FakeNode(0, 0);
        var goal = new FakeNode(10, 10);
        var task = new PathfindToTask<FakeNode>(controller, start, goal);

        task.Update(0.016);

        Assert.AreEqual(1, controller.TryFindPathCalls);
        Assert.AreSame(start, controller.LastStart);
        Assert.AreSame(goal, controller.LastGoal);
    }

    [TestMethod]
    public void Update_PathFound_DoesNotCancel()
    {
        var controller = new FakePathingController { ShouldFindPath = true };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));
        bool cancelled = false;
        task.TaskCancelled += (_, _) => cancelled = true;

        task.Update(0.016);

        Assert.IsFalse(cancelled);
    }

    [TestMethod]
    public void Update_PathNotFound_CancelsTask()
    {
        var controller = new FakePathingController { ShouldFindPath = false };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));
        bool cancelled = false;
        task.TaskCancelled += (_, _) => cancelled = true;

        task.Update(0.016);

        Assert.IsTrue(cancelled);
    }

    [TestMethod]
    public void Update_PathNotFound_DoesNotFireTaskCompleted()
    {
        var controller = new FakePathingController { ShouldFindPath = false };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));
        bool completed = false;
        task.TaskCompleted += (_, _) => completed = true;

        task.Update(0.016);

        Assert.IsFalse(completed);
    }

    [TestMethod]
    public void ControllerArrived_MakesTaskComplete()
    {
        var controller = new FakePathingController { ShouldFindPath = true };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));
        task.Update(0.016);

        controller.FireArrived();

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Update_AfterArrived_CallsComplete()
    {
        var controller = new FakePathingController { ShouldFindPath = true };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));
        bool completed = false;
        task.TaskCompleted += (_, _) => completed = true;
        task.Update(0.016);
        controller.FireArrived();

        task.Update(0.016);

        Assert.IsTrue(completed);
    }

    [TestMethod]
    public void Cancel_BeforeArrived_ClearsPath()
    {
        var controller = new FakePathingController { ShouldFindPath = true };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));
        task.Update(0.016);

        task.Cancel();

        Assert.AreEqual(1, controller.ClearPathCalls);
    }

    [TestMethod]
    public void Update_PathNotFound_ClearsPath()
    {
        // Failed pathfinding routes through Cancel → OnCancel, which clears the (empty) path
        // defensively so a re-tried Reset+Update on a recycled controller is safe.
        var controller = new FakePathingController { ShouldFindPath = false };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));

        task.Update(0.016);

        Assert.AreEqual(1, controller.ClearPathCalls);
    }

    [TestMethod]
    public void Cancel_UnsubscribesFromArrived()
    {
        var controller = new FakePathingController { ShouldFindPath = true };
        var task = new PathfindToTask<FakeNode>(controller, new FakeNode(0, 0), new FakeNode(1, 1));
        task.Update(0.016);

        task.Cancel();
        task.Reset();
        controller.FireArrived();

        Assert.IsFalse(task.IsComplete);
    }
}

using BabyBearsEngine.Geometry;
using BabyBearsEngine.Tasks;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MoveToTaskTests
{
    private sealed class FakePosition(float x, float y) : IPosition
    {
        public float X { get; } = x;
        public float Y { get; } = y;
    }

    private sealed class FakeWaypointController : IWaypointController
    {
        public IList<IPosition> SetWaypointsCalls { get; } = [];
        public int ClearWaypointsCalls { get; private set; }

        public bool ReachedDestination => Waypoints.Count == 0;
        public IList<IPosition> Waypoints { get; } = [];

        public IContainer? Parent { get; set; }
        public bool Exists => Parent is not null;
        public bool Active { get; set; } = true;

        public event EventHandler? Arrived;
        public event EventHandler<DirectionChangedEventArgs>? DirectionChanged;
        public event EventHandler? ReachedWaypoint;
        public event EventHandler? Added;
        public event EventHandler? Removed;

        public void AddWaypoints(IEnumerable<IPosition> waypoints) { }
        public void AddWaypoints(params IPosition[] waypoints) { }

        public void ClearWaypoints()
        {
            ClearWaypointsCalls++;
            Waypoints.Clear();
        }

        public IPosition GetNextWaypoint() => Waypoints[0];

        public void Remove() => Parent = null;

        public void SetWaypoints(IEnumerable<IPosition> waypoints)
        {
            SetWaypointsCalls.Add(waypoints.First());
            Waypoints.Clear();
            foreach (IPosition wp in waypoints)
            {
                Waypoints.Add(wp);
            }
        }

        public void SetWaypoints(params IPosition[] waypoints) => SetWaypoints((IEnumerable<IPosition>)waypoints);

        public void Update(double elapsed) { }

        public void FireArrived() => Arrived?.Invoke(this, EventArgs.Empty);
    }

    [TestMethod]
    public void NewTask_IsNotComplete()
    {
        var controller = new FakeWaypointController();
        var target = new FakePosition(10, 20);

        var task = new MoveToTask(controller, target);

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void Update_FirstCall_SetsWaypointOnController()
    {
        var controller = new FakeWaypointController();
        var target = new FakePosition(10, 20);
        var task = new MoveToTask(controller, target);

        task.Update(0.016);

        Assert.HasCount(1, controller.SetWaypointsCalls);
        Assert.AreSame(target, controller.SetWaypointsCalls[0]);
    }

    [TestMethod]
    public void Update_FirstCall_FiresTaskStarted()
    {
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        bool started = false;
        task.TaskStarted += (_, _) => started = true;

        task.Update(0.016);

        Assert.IsTrue(started);
    }

    [TestMethod]
    public void ControllerArrived_BeforeNextUpdate_MakesTaskComplete()
    {
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        task.Update(0.016);

        controller.FireArrived();

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Update_AfterArrived_CallsComplete()
    {
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        bool completed = false;
        task.TaskCompleted += (_, _) => completed = true;
        task.Update(0.016);
        controller.FireArrived();

        task.Update(0.016);

        Assert.IsTrue(completed);
    }

    [TestMethod]
    public void Cancel_BeforeArrived_ClearsWaypoints()
    {
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        task.Update(0.016);

        task.Cancel();

        Assert.AreEqual(1, controller.ClearWaypointsCalls);
    }

    [TestMethod]
    public void Cancel_AfterArrived_DoesNotClearWaypoints()
    {
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        task.Update(0.016);
        controller.FireArrived();
        task.Update(0.016); // task auto-completes here

        task.Cancel(); // already completed — Cancel is a no-op

        Assert.AreEqual(0, controller.ClearWaypointsCalls);
    }

    [TestMethod]
    public void Complete_UnsubscribesFromArrived()
    {
        // After completion, further Arrived events must not be observed — otherwise a
        // reused task could double-process arrivals on the same controller.
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        task.Update(0.016);
        controller.FireArrived();
        task.Update(0.016); // auto-completes

        task.Reset();
        controller.FireArrived(); // would re-set _arrived if subscription leaked

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void Cancel_UnsubscribesFromArrived()
    {
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        task.Update(0.016);

        task.Cancel();
        task.Reset();
        controller.FireArrived();

        Assert.IsFalse(task.IsComplete);
    }

    [TestMethod]
    public void Reset_AllowsTaskToRunAgain()
    {
        var controller = new FakeWaypointController();
        var task = new MoveToTask(controller, new FakePosition(0, 0));
        task.Update(0.016);
        controller.FireArrived();
        task.Update(0.016);
        Assert.IsTrue(task.IsComplete);

        task.Reset();

        Assert.IsFalse(task.IsComplete);
    }
}

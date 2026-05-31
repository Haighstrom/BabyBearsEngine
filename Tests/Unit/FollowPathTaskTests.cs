using BabyBearsEngine.Geometry;
using BabyBearsEngine.Tasks;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FollowPathTaskTests
{
    private sealed class FakePosition(float x, float y) : IPosition
    {
        public float X { get; } = x;
        public float Y { get; } = y;
    }

    private sealed class FakeWaypointController : IWaypointController
    {
        public List<IPosition> LastSetWaypoints { get; private set; } = [];
        public int SetWaypointsCalls { get; private set; }
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
            SetWaypointsCalls++;
            LastSetWaypoints = waypoints.ToList();
            Waypoints.Clear();
            foreach (IPosition wp in LastSetWaypoints)
            {
                Waypoints.Add(wp);
            }
        }

        public void SetWaypoints(params IPosition[] waypoints) => SetWaypoints((IEnumerable<IPosition>)waypoints);

        public void Update(double elapsed) { }

        public void FireArrived() => Arrived?.Invoke(this, EventArgs.Empty);
    }

    [TestMethod]
    public void Update_FirstCall_SetsAllWaypointsOnController()
    {
        var controller = new FakeWaypointController();
        var path = new IPosition[]
        {
            new FakePosition(1, 1),
            new FakePosition(2, 2),
            new FakePosition(3, 3),
        };
        var task = new FollowPathTask(controller, path);

        task.Update(0.016);

        Assert.AreEqual(1, controller.SetWaypointsCalls);
        CollectionAssert.AreEqual(path, controller.LastSetWaypoints);
    }

    [TestMethod]
    public void ControllerArrived_MakesTaskComplete()
    {
        var controller = new FakeWaypointController();
        var task = new FollowPathTask(controller, new[] { new FakePosition(0, 0) });
        task.Update(0.016);

        controller.FireArrived();

        Assert.IsTrue(task.IsComplete);
    }

    [TestMethod]
    public void Update_AfterArrived_CallsComplete()
    {
        var controller = new FakeWaypointController();
        var task = new FollowPathTask(controller, new[] { new FakePosition(0, 0) });
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
        var task = new FollowPathTask(controller, new[] { new FakePosition(0, 0) });
        task.Update(0.016);

        task.Cancel();

        Assert.AreEqual(1, controller.ClearWaypointsCalls);
    }

    [TestMethod]
    public void Cancel_UnsubscribesFromArrived()
    {
        var controller = new FakeWaypointController();
        var task = new FollowPathTask(controller, new[] { new FakePosition(0, 0) });
        task.Update(0.016);

        task.Cancel();
        task.Reset();
        controller.FireArrived();

        Assert.IsFalse(task.IsComplete);
    }
}

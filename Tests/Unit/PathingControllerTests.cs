using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Pathfinding;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PathingControllerTests
{
    private sealed class FakePathfinder : IPathfinder<PathfindNode>
    {
        public IList<PathfindNode>? PathToReturn { get; set; }
        public IList<PathfindNode>? LastStartEnd { get; private set; }
        public Func<PathfindNode, PathfindNode, bool>? LastPassableTest { get; private set; }

        public IList<PathfindNode>? FindPath(PathfindNode start, PathfindNode end, Func<PathfindNode, PathfindNode, bool> passableTest)
        {
            LastStartEnd = [start, end];
            LastPassableTest = passableTest;
            return PathToReturn;
        }

        public IList<PathfindNode> FindRandomPath(PathfindNode start, Func<PathfindNode, PathfindNode, bool> passableTest, int targetSteps, bool canBacktrack) =>
            throw new NotSupportedException();
    }

    private sealed class FakeWaypointController : IWaypointController
    {
        public IList<IPosition> Waypoints { get; private set; } = [];
        public bool ReachedDestination => Waypoints.Count == 0;

        public int SetWaypointsCallCount { get; private set; } = 0;
        public int ClearWaypointsCallCount { get; private set; } = 0;
        public double LastUpdateElapsed { get; private set; } = -1.0;

        public event EventHandler? Arrived;
        public event EventHandler<DirectionChangedEventArgs>? DirectionChanged;
        public event EventHandler? ReachedWaypoint;

        public void RaiseArrived() => Arrived?.Invoke(this, EventArgs.Empty);

        public void AddWaypoints(IEnumerable<IPosition> waypoints)
        {
            foreach (IPosition wp in waypoints)
            {
                Waypoints.Add(wp);
            }
        }

        public void AddWaypoints(params IPosition[] waypoints) => AddWaypoints((IEnumerable<IPosition>)waypoints);

        public void ClearWaypoints()
        {
            ClearWaypointsCallCount++;
            Waypoints.Clear();
        }

        public IPosition GetNextWaypoint() => Waypoints[0];

        public void SetWaypoints(IEnumerable<IPosition> waypoints)
        {
            SetWaypointsCallCount++;
            Waypoints = [.. waypoints];
        }

        public void SetWaypoints(params IPosition[] waypoints) => SetWaypoints((IEnumerable<IPosition>)waypoints);

        public bool Active { get; set; } = true;
        public bool UpdateLast => false;
        public IContainer? Parent { get; set; } = null;
        public bool Exists => Parent is not null;
        public event EventHandler? Added;
        public event EventHandler? Removed;
        public void Remove() => throw new NotSupportedException();

        public void Update(double elapsed)
        {
            LastUpdateElapsed = elapsed;
        }
    }

    private FakePathfinder _pathfinder = null!;
    private FakeWaypointController _waypoint = null!;
    private PathingController<PathfindNode> _controller = null!;
    private Func<PathfindNode, PathfindNode, bool> _passableTest = null!;
    private List<string> _events = null!;

    [TestInitialize]
    public void Setup()
    {
        _pathfinder = new();
        _waypoint = new();
        _passableTest = (_, _) => true;
        _controller = new(_pathfinder, _waypoint, _passableTest);
        _events = [];

        _controller.Arrived += (_, _) => _events.Add("Arrived");
        _controller.PathFound += (_, _) => _events.Add("PathFound");
        _controller.PathNotFound += (_, _) => _events.Add("PathNotFound");
    }

    // TryFindPath — success

    [TestMethod]
    public void TryFindPath_WhenPathfinderReturnsPath_ReturnsTrue()
    {
        PathfindNode start = new(0, 0);
        PathfindNode goal = new(2, 0);
        _pathfinder.PathToReturn = [start, new PathfindNode(1, 0), goal];

        bool found = _controller.TryFindPath(start, goal);

        Assert.IsTrue(found);
    }

    [TestMethod]
    public void TryFindPath_WhenPathfinderReturnsPath_LoadsWaypoints()
    {
        PathfindNode start = new(0, 0);
        PathfindNode goal = new(2, 0);
        PathfindNode middle = new(1, 0);
        _pathfinder.PathToReturn = [start, middle, goal];

        _controller.TryFindPath(start, goal);

        Assert.HasCount(3, _waypoint.Waypoints);
        Assert.AreEqual(1, _waypoint.SetWaypointsCallCount);
    }

    [TestMethod]
    public void TryFindPath_WhenPathfinderReturnsPath_RaisesPathFound()
    {
        PathfindNode start = new(0, 0);
        PathfindNode goal = new(2, 0);
        _pathfinder.PathToReturn = [start, goal];

        _controller.TryFindPath(start, goal);

        CollectionAssert.AreEqual(new[] { "PathFound" }, _events);
    }

    [TestMethod]
    public void TryFindPath_ReplacesExistingPath()
    {
        PathfindNode start = new(0, 0);
        PathfindNode goal = new(2, 0);
        _pathfinder.PathToReturn = [start, goal];
        _controller.TryFindPath(start, goal);

        _pathfinder.PathToReturn = [start, new PathfindNode(0, 1), new PathfindNode(0, 2)];
        _controller.TryFindPath(start, new PathfindNode(0, 2));

        Assert.HasCount(3, _waypoint.Waypoints);
        Assert.AreEqual(2, _waypoint.SetWaypointsCallCount);
    }

    // TryFindPath — failure

    [TestMethod]
    public void TryFindPath_WhenPathfinderReturnsNull_ReturnsFalse()
    {
        _pathfinder.PathToReturn = null;

        bool found = _controller.TryFindPath(new PathfindNode(0, 0), new PathfindNode(5, 5));

        Assert.IsFalse(found);
    }

    [TestMethod]
    public void TryFindPath_WhenPathfinderReturnsNull_DoesNotChangeWaypoints()
    {
        _waypoint.AddWaypoints(new Point(99, 99));
        _pathfinder.PathToReturn = null;

        _controller.TryFindPath(new PathfindNode(0, 0), new PathfindNode(5, 5));

        Assert.HasCount(1, _waypoint.Waypoints);
        Assert.AreEqual(0, _waypoint.SetWaypointsCallCount);
        Assert.AreEqual(0, _waypoint.ClearWaypointsCallCount);
    }

    [TestMethod]
    public void TryFindPath_WhenPathfinderReturnsNull_RaisesPathNotFound()
    {
        _pathfinder.PathToReturn = null;

        _controller.TryFindPath(new PathfindNode(0, 0), new PathfindNode(5, 5));

        CollectionAssert.AreEqual(new[] { "PathNotFound" }, _events);
    }

    // PassableTest

    [TestMethod]
    public void TryFindPath_PassesCurrentPassableTestToPathfinder()
    {
        Func<PathfindNode, PathfindNode, bool> custom = (_, _) => false;
        _controller.PassableTest = custom;

        _controller.TryFindPath(new PathfindNode(0, 0), new PathfindNode(1, 0));

        Assert.AreSame(custom, _pathfinder.LastPassableTest);
    }

    // HasPath

    [TestMethod]
    public void HasPath_WhenWaypointsEmpty_ReturnsFalse()
    {
        Assert.IsFalse(_controller.HasPath);
    }

    [TestMethod]
    public void HasPath_WhenWaypointsExist_ReturnsTrue()
    {
        _pathfinder.PathToReturn = [new PathfindNode(0, 0), new PathfindNode(1, 0)];

        _controller.TryFindPath(new PathfindNode(0, 0), new PathfindNode(1, 0));

        Assert.IsTrue(_controller.HasPath);
    }

    // ClearPath

    [TestMethod]
    public void ClearPath_DelegatesToWaypointController()
    {
        _pathfinder.PathToReturn = [new PathfindNode(0, 0), new PathfindNode(1, 0)];
        _controller.TryFindPath(new PathfindNode(0, 0), new PathfindNode(1, 0));

        _controller.ClearPath();

        Assert.IsFalse(_controller.HasPath);
        Assert.AreEqual(1, _waypoint.ClearWaypointsCallCount);
    }

    // Update

    [TestMethod]
    public void Update_DelegatesToWaypointController()
    {
        _controller.Update(0.25);

        Assert.AreEqual(0.25, _waypoint.LastUpdateElapsed);
    }

    // Arrived proxying

    [TestMethod]
    public void Arrived_ProxiesInnerWaypointArrived()
    {
        _waypoint.RaiseArrived();

        CollectionAssert.AreEqual(new[] { "Arrived" }, _events);
    }

    [TestMethod]
    public void Arrived_WithoutSubscriber_DoesNotThrow()
    {
        // Fresh controller with no Arrived subscribers; raising on the inner must be safe.
        // Test passes if no exception is thrown.
        _ = new PathingController<PathfindNode>(_pathfinder, _waypoint, _passableTest);

        _waypoint.RaiseArrived();
    }
}

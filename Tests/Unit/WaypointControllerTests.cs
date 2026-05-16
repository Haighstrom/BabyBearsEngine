using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class WaypointControllerTests
{
    private sealed class FakeWaypointable : IWaypointable
    {
        public float Speed { get; set; } = 100f;
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
    }

    private FakeWaypointable _target = null!;
    private WaypointController _controller = null!;
    private List<string> _events = null!;

    [TestInitialize]
    public void Setup()
    {
        _target = new();
        _controller = new(_target);
        _events = [];

        _controller.Arrived += (_, _) => _events.Add("Arrived");
        _controller.DirectionChanged += (_, e) => _events.Add($"DirectionChanged({e.OldDirection}->{e.NewDirection})");
        _controller.ReachedWaypoint += (_, _) => _events.Add("ReachedWaypoint");
    }

    private void Update(double elapsed = 0.1) => _controller.Update(elapsed);

    // ReachedDestination

    [TestMethod]
    public void ReachedDestination_WhenNoWaypoints_ReturnsTrue()
    {
        Assert.IsTrue(_controller.ReachedDestination);
    }

    [TestMethod]
    public void ReachedDestination_WhenWaypointsExist_ReturnsFalse()
    {
        _controller.AddWaypoints(new Point(100, 0));

        Assert.IsFalse(_controller.ReachedDestination);
    }

    // Update — no waypoints

    [TestMethod]
    public void Update_WhenNoWaypoints_DoesNotMoveTarget()
    {
        Update();

        Assert.AreEqual(0f, _target.X);
        Assert.AreEqual(0f, _target.Y);
    }

    [TestMethod]
    public void Update_WhenNoWaypoints_RaisesNoEvents()
    {
        Update();

        Assert.IsEmpty(_events);
    }

    // Update — partial move

    [TestMethod]
    public void Update_WhenWaypointNotReachableThisFrame_MovesTargetPartially()
    {
        // Speed=100, elapsed=0.5 → amountToMove=50. Waypoint at (100,0), distance=100 > 50.
        _target.Speed = 100f;
        _controller.AddWaypoints(new Point(100, 0));

        Update(elapsed: 0.5);

        Assert.AreEqual(50f, _target.X, 0.001f);
        Assert.AreEqual(0f, _target.Y, 0.001f);
    }

    [TestMethod]
    public void Update_WhenWaypointNotReachableThisFrame_DoesNotRemoveWaypoint()
    {
        _controller.AddWaypoints(new Point(100, 0));

        Update(elapsed: 0.5);

        Assert.AreEqual(1, _controller.Waypoints.Count);
    }

    [TestMethod]
    public void Update_WhenWaypointNotReachableThisFrame_FiresDirectionChangedOnly()
    {
        // On the first partial-move frame, DirectionChanged fires (null→Right).
        // ReachedWaypoint and Arrived do not fire.
        _controller.AddWaypoints(new Point(100, 0));

        Update(elapsed: 0.5);

        CollectionAssert.AreEqual(new[] { "DirectionChanged(->Right)" }, _events);
    }

    // Update — waypoint reached

    [TestMethod]
    public void Update_WhenWaypointReachableThisFrame_SnapsTargetToWaypoint()
    {
        // Speed=100, elapsed=1.0 → amountToMove=100. Waypoint at (50,0), distance=50 ≤ 100.
        _controller.AddWaypoints(new Point(50, 0));

        Update(elapsed: 1.0);

        Assert.AreEqual(50f, _target.X);
        Assert.AreEqual(0f, _target.Y);
    }

    [TestMethod]
    public void Update_WhenWaypointReachableThisFrame_RemovesWaypoint()
    {
        _controller.AddWaypoints(new Point(50, 0));

        Update(elapsed: 1.0);

        Assert.AreEqual(0, _controller.Waypoints.Count);
    }

    [TestMethod]
    public void Update_WhenSingleWaypointReached_RaisesEventsInOrder()
    {
        // ReachedWaypoint fires inside the move loop; DirectionChanged and Arrived fire after.
        _controller.AddWaypoints(new Point(50, 0));

        Update(elapsed: 1.0);

        CollectionAssert.AreEqual(
            new[] { "ReachedWaypoint", "DirectionChanged(->Right)", "Arrived" },
            _events);
    }

    // Update — multiple waypoints in one frame

    [TestMethod]
    public void Update_WhenMultipleWaypointsReachableThisFrame_ProcessesAll()
    {
        // Speed=200, elapsed=1.0 → amountToMove=200. Waypoints at (50,0) and (100,0).
        _target.Speed = 200f;
        _controller.AddWaypoints(new Point(50, 0), new Point(100, 0));

        Update(elapsed: 1.0);

        Assert.AreEqual(100f, _target.X, 0.001f);
        Assert.AreEqual(0, _controller.Waypoints.Count);
        Assert.IsTrue(_controller.ReachedDestination);
    }

    [TestMethod]
    public void Update_WhenMultipleWaypointsReached_RaisesReachedWaypointForEach()
    {
        _target.Speed = 200f;
        _controller.AddWaypoints(new Point(50, 0), new Point(100, 0));

        Update(elapsed: 1.0);

        Assert.AreEqual(2, _events.Count(e => e == "ReachedWaypoint"));
    }

    // Update — DirectionChanged

    [TestMethod]
    public void Update_OnFirstMovement_FiresDirectionChangedWithNullOldDirection()
    {
        _controller.AddWaypoints(new Point(100, 0));

        Update(elapsed: 0.1);

        Assert.IsTrue(_events.Contains("DirectionChanged(->Right)"));
    }

    [TestMethod]
    public void Update_WhenDirectionUnchanged_DoesNotFireDirectionChanged()
    {
        _controller.AddWaypoints(new Point(100, 0));
        Update(elapsed: 0.1);
        _events.Clear();

        Update(elapsed: 0.1);

        Assert.IsFalse(_events.Any(e => e.StartsWith("DirectionChanged")));
    }

    [TestMethod]
    public void Update_WhenDirectionChanges_FiresDirectionChangedWithBothDirections()
    {
        // Frame 1: partial move right → DirectionChanged(->Right), _lastDirection=Right.
        // Frame 2: switch to a down waypoint → DirectionChanged(Right->Down).
        _controller.AddWaypoints(new Point(100, 0));
        Update(elapsed: 0.5);  // partial, reaches (50,0). _lastDirection=Right.
        _controller.SetWaypoints(new Point(50, 100));  // straight down from current position
        _events.Clear();

        Update(elapsed: 0.5);  // moves down

        Assert.IsTrue(_events.Contains("DirectionChanged(Right->Down)"));
    }

    [TestMethod]
    public void Update_AfterArrival_FiresDirectionChangedAgainOnNextMovement()
    {
        // Arrive, then set new waypoints — DirectionChanged should fire again
        // because _lastDirection is reset to null on arrival.
        _controller.AddWaypoints(new Point(50, 0));
        Update(elapsed: 1.0);  // arrive
        _events.Clear();

        _controller.AddWaypoints(new Point(50, 100));
        Update(elapsed: 0.1);

        Assert.IsTrue(_events.Contains("DirectionChanged(->Down)"));
    }

    // Waypoint management

    [TestMethod]
    public void SetWaypoints_ReplacesExistingWaypoints()
    {
        _controller.AddWaypoints(new Point(100, 0));
        _controller.SetWaypoints(new Point(0, 50), new Point(0, 100));

        Assert.AreEqual(2, _controller.Waypoints.Count);
        Assert.AreEqual(new Point(0, 50), _controller.Waypoints[0]);
    }

    [TestMethod]
    public void ClearWaypoints_RemovesAllWaypoints()
    {
        _controller.AddWaypoints(new Point(100, 0), new Point(200, 0));

        _controller.ClearWaypoints();

        Assert.IsEmpty(_controller.Waypoints);
    }

    [TestMethod]
    public void GetNextWaypoint_WhenWaypointsExist_ReturnsFirstWithoutRemoving()
    {
        _controller.AddWaypoints(new Point(100, 0), new Point(200, 0));

        IPosition next = _controller.GetNextWaypoint();

        Assert.AreEqual(new Point(100, 0), next);
        Assert.AreEqual(2, _controller.Waypoints.Count);
    }

    [TestMethod]
    public void GetNextWaypoint_WhenNoWaypoints_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _controller.GetNextWaypoint());
    }

    // DirectionExtensions

    [TestMethod]
    [DataRow(1f, 0f, Direction.Right)]
    [DataRow(-1f, 0f, Direction.Left)]
    [DataRow(0f, 1f, Direction.Down)]
    [DataRow(0f, -1f, Direction.Up)]
    [DataRow(3f, 1f, Direction.Right)]
    [DataRow(-3f, -1f, Direction.Left)]
    [DataRow(1f, 3f, Direction.Down)]
    [DataRow(-1f, -3f, Direction.Up)]
    public void ToDirection_ReturnsCorrectCardinalDirection(float x, float y, Direction expected)
    {
        Point p = new(x, y);

        Assert.AreEqual(expected, p.ToDirection());
    }
}

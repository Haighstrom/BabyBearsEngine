using System.Linq;
using BabyBearsEngine.Pathfinding;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class GridPathfinderTests
{
    private static GridPathfinder<PathfindNode> MakeGrid(int width, int height) =>
        new(width, height, (x, y) => new PathfindNode(x, y));

    [TestMethod]
    public void Dimensions_MatchConstructorArgs()
    {
        var grid = MakeGrid(5, 3);
        Assert.AreEqual(5, grid.Width);
        Assert.AreEqual(3, grid.Height);
    }

    [TestMethod]
    public void Indexer_ReturnsNodeAtPosition()
    {
        var grid = MakeGrid(3, 3);
        var node = grid[1, 2];
        Assert.AreEqual(1, node.X);
        Assert.AreEqual(2, node.Y);
    }

    [TestMethod]
    public void EachInteriorNode_HasFourNeighbours()
    {
        var grid = MakeGrid(3, 3);
        var centre = grid[1, 1];
        Assert.HasCount(4, centre.ConnectedNodes);
    }

    [TestMethod]
    public void CornerNode_HasTwoNeighbours()
    {
        var grid = MakeGrid(3, 3);
        var corner = grid[0, 0];
        Assert.HasCount(2, corner.ConnectedNodes);
    }

    [TestMethod]
    public void IsValidGridPosition_AcceptsIntegersInsideBounds()
    {
        var grid = MakeGrid(3, 3);
        Assert.IsTrue(grid.IsValidGridPosition(0, 0));
        Assert.IsTrue(grid.IsValidGridPosition(2, 2));
        Assert.IsFalse(grid.IsValidGridPosition(-1, 0));
        Assert.IsFalse(grid.IsValidGridPosition(0, 3));
        Assert.IsFalse(grid.IsValidGridPosition(0.5f, 0));
    }

    [TestMethod]
    public void GetClosestNode_ClampsToGridBounds()
    {
        var grid = MakeGrid(3, 3);
        Assert.AreSame(grid[0, 0], grid.GetClosestNode(-5f, -5f));
        Assert.AreSame(grid[2, 2], grid.GetClosestNode(100f, 100f));
    }

    [TestMethod]
    public void FindPath_DirectRoute_ReturnsShortestSequence()
    {
        var grid = MakeGrid(3, 3);
        var path = grid.FindPath(grid[0, 0], grid[2, 0], (_, _) => true);

        Assert.IsNotNull(path);
        Assert.HasCount(3, path);
        Assert.AreSame(grid[0, 0], path[0]);
        Assert.AreSame(grid[2, 0], path[^1]);
    }

    [TestMethod]
    public void FindPath_BlockedByPassableTest_ReturnsNull()
    {
        var grid = MakeGrid(3, 1);
        // Block the middle cell entirely — no path from left to right.
        var path = grid.FindPath(grid[0, 0], grid[2, 0], (_, to) => !ReferenceEquals(to, grid[1, 0]));

        Assert.IsNull(path);
    }

    [TestMethod]
    public void FindPath_StartEqualsEnd_ReturnsSingletonPath()
    {
        var grid = MakeGrid(3, 3);
        var path = grid.FindPath(grid[1, 1], grid[1, 1], (_, _) => true);

        Assert.IsNotNull(path);
        Assert.HasCount(1, path);
        Assert.AreSame(grid[1, 1], path[0]);
    }

    [TestMethod]
    public void FindRandomPath_TargetSteps_WalksThatManyNodesWhenSpaceAllows()
    {
        var grid = MakeGrid(5, 5);
        var path = grid.FindRandomPath(grid[2, 2], (_, _) => true, targetSteps: 3, canBacktrack: false);

        // Path includes start + targetSteps moves = targetSteps + 1 nodes.
        Assert.HasCount(4, path);
        Assert.AreSame(grid[2, 2], path[0]);
        // Unique nodes when canBacktrack: false.
        Assert.AreEqual(path.Count, path.Distinct().Count());
    }
}

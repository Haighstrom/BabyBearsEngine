using System.Reflection;
using BabyBearsEngine.Pathfinding;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class AStarSolverTests
{
    private static (PathfindNode a, PathfindNode b, PathfindNode c) MakeLineGraph(float costA, float costB, float costC)
    {
        PathfindNode a = new(0, 0, distanceBetweenConnectedNodes: costA);
        PathfindNode b = new(1, 0, distanceBetweenConnectedNodes: costB);
        PathfindNode c = new(2, 0, distanceBetweenConnectedNodes: costC);

        a.ConnectedNodes.Add(b);
        b.ConnectedNodes.Add(a);
        b.ConnectedNodes.Add(c);
        c.ConnectedNodes.Add(b);

        return (a, b, c);
    }

    private static float SolveAndGetEndCost(PathfindNode start, PathfindNode end)
    {
        AStarSolver<PathfindNode> solver = new(start, end, (_, _) => true, (_, _) => 0f);
        solver.TrySolve();

        // AStarData is private to AStarSolver, but its G property is generated public on the record.
        object data = end.GraphSearchData!;
        PropertyInfo? gProperty = data.GetType().GetProperty("G");
        return (float)gProperty!.GetValue(data)!;
    }

    [TestMethod]
    public void TrySolve_PathCostIsSymmetric_WhenStartAndEndHaveDifferentCosts()
    {
        // A(cost=1) — B(cost=1) — C(cost=100). Traversing A→C and C→A visits the same
        // edges and should produce the same total path cost.
        var (a1, _, c1) = MakeLineGraph(costA: 1f, costB: 1f, costC: 100f);
        var (a2, _, c2) = MakeLineGraph(costA: 1f, costB: 1f, costC: 100f);

        float costAToC = SolveAndGetEndCost(a1, c1);
        float costCToA = SolveAndGetEndCost(c2, a2);

        Assert.AreEqual(costAToC, costCToA, 0.001f);
    }

    [TestMethod]
    public void TrySolve_PathCostUsesAverageOfSourceAndDestinationNodeCosts()
    {
        // A(1) — B(1) — C(100): expected cost with average model = (1+1)/2 + (1+100)/2 = 1 + 50.5 = 51.5
        var (a, _, c) = MakeLineGraph(costA: 1f, costB: 1f, costC: 100f);

        float cost = SolveAndGetEndCost(a, c);

        Assert.AreEqual(51.5f, cost, 0.001f);
    }
}

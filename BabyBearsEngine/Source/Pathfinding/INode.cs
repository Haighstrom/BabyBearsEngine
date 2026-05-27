namespace BabyBearsEngine.Pathfinding;

/// <summary>
/// A node in a graph: equatable to its own concrete type and knows which other nodes
/// it's connected to.
/// </summary>
/// <typeparam name="TNode">The concrete node type (CRTP — curiously recurring template pattern).</typeparam>
public interface INode<TNode> : IEquatable<TNode>
    where TNode : INode<TNode>
{
    /// <summary>Nodes reachable from this one in one step.</summary>
    IList<TNode> ConnectedNodes { get; }
}

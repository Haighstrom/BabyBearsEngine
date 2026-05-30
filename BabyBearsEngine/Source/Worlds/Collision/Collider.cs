using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Collision;

/// <summary>
/// Attaches a collision shape to an <see cref="IPosition"/> so a <see cref="CollisionSolver"/>
/// can detect when it overlaps other colliders. Add the collider to any container inside the
/// scene tree — it self-registers with the supplied <see cref="CollisionSolver"/> on add and
/// self-deregisters on remove.
/// </summary>
/// <remarks>
/// <para><b>Owner is anything with X/Y.</b> Any <see cref="IPosition"/> — an <see cref="Entity"/>,
/// a pathfinding node, a custom class, even a <see cref="Point"/> for a fixed-position collider —
/// can be the owner. The collider follows the owner's <c>(X, Y)</c> each frame; the owner does not
/// need to be rectangular or part of the entity tree.</para>
/// <para><b>Category filtering.</b> <see cref="CollisionCategory"/> is one bit identifying what
/// kind of thing this collider is (player, enemy, wall, etc.). <see cref="CollideCategories"/>
/// is the bitmask of categories this collider is willing to interact with. A pair of colliders
/// is only checked for overlap when each side's category is included in the other side's
/// <see cref="CollideCategories"/> — both must opt in. The common gotcha is setting
/// <see cref="CollideCategories"/> on one side but forgetting the other; nothing fires until
/// both sides allow each other. Defaults (<see cref="CollisionCategory"/> = <c>1</c>,
/// <see cref="CollideCategories"/> = <see cref="uint.MaxValue"/>) mean every collider collides
/// with every other; restrict only when you need filtering.</para>
/// <para><b>World vs local space.</b> The supplied <c>localShape</c> is expressed in the owner's
/// local space (i.e. relative to <c>(0, 0)</c>). The solver reads <see cref="WorldShape"/>,
/// which translates the local shape by the owner's current <c>(X, Y)</c>. Rotation and parent-chain
/// transforms are <em>not</em> applied — colliders track their direct owner only.</para>
/// <para><b>Removed colliders.</b> When a collider is removed mid-game its lingering overlaps are
/// reported as <see cref="OverlapExited"/> on the surviving partner during the next solver update.
/// The removed collider does <em>not</em> receive its own exit events; subscribe to the
/// <see cref="AddableBase.Removed"/> event for end-of-life cleanup.</para>
/// </remarks>
/// <param name="solver">The <see cref="CollisionSolver"/> this collider registers with on add.</param>
/// <param name="owner">The owner whose <c>(X, Y)</c> position is used to translate <paramref name="localShape"/> into world space each frame. Any <see cref="IPosition"/> will do.</param>
/// <param name="localShape">The collision shape in the owner's local coordinate space (relative to the owner's top-left).</param>
/// <param name="collisionCategory">A single bit identifying what kind of thing this collider is — its category tag. See remarks for filtering semantics.</param>
/// <param name="collideCategories">Bitmask of categories this collider is willing to interact with. See remarks for filtering semantics.</param>
public class Collider(CollisionSolver solver, IPosition owner, ICollisionShape localShape, uint collisionCategory = 1, uint collideCategories = uint.MaxValue)
    : AddableBase
{
    /// <summary>When false, this collider is skipped by the solver until set true again. Existing overlaps are reported as <see cref="OverlapExited"/> the next frame.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Bitmask of categories this collider is willing to interact with. For an overlap to fire, both sides must include the other's <see cref="CollisionCategory"/> here. See class remarks.</summary>
    public uint CollideCategories { get; set; } = collideCategories;

    /// <summary>A single bit identifying what kind of thing this collider is — its category tag (e.g. player, enemy, wall). See class remarks.</summary>
    public uint CollisionCategory { get; set; } = collisionCategory;

    /// <summary>The collision shape in the owner's local space (offset (0, 0) ≡ owner's <c>(X, Y)</c>).</summary>
    public ICollisionShape LocalShape { get; set; } = localShape;

    /// <summary>The owner whose position translates <see cref="LocalShape"/> into world space.</summary>
    public IPosition Owner => owner;

    /// <summary>The collision shape in world space — <see cref="LocalShape"/> translated by the owner's current <c>(X, Y)</c>.</summary>
    public ICollisionShape WorldShape => LocalShape.Translate(owner.X, owner.Y);

    /// <summary>Raised when this collider begins overlapping another collider. Fires once per pair, on the frame the overlap starts.</summary>
    public event EventHandler<OverlapEventArgs>? OverlapEntered;

    /// <summary>Raised when this collider stops overlapping a previously-overlapping collider. Fires once per pair, on the frame the overlap ends.</summary>
    public event EventHandler<OverlapEventArgs>? OverlapExited;

    /// <inheritdoc/>
    protected override void OnAdded()
    {
        solver.RegisterCollider(this);
    }

    /// <inheritdoc/>
    protected override void OnRemoved()
    {
        solver.UnregisterCollider(this);
    }

    internal void RaiseOverlapEntered(Collider other)
    {
        OverlapEntered?.Invoke(this, new OverlapEventArgs(this, other));
    }

    internal void RaiseOverlapExited(Collider other)
    {
        OverlapExited?.Invoke(this, new OverlapEventArgs(this, other));
    }
}

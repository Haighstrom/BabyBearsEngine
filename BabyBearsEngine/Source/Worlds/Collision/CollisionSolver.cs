namespace BabyBearsEngine.Worlds.Collision;

/// <summary>
/// The pairwise overlap solver. Add one to an <see cref="IWorld"/> alongside your gameplay
/// content, then pass it to every <see cref="Collider"/> you construct. Each <see cref="Update"/>
/// runs an O(N²) overlap check across all registered colliders, diffing against the previous
/// frame's overlap set to fire <see cref="Collider.OverlapEntered"/> and
/// <see cref="Collider.OverlapExited"/> events for state changes.
/// </summary>
/// <remarks>
/// <para><b>Update ordering.</b> The solver overrides <see cref="UpdateLast"/> to <c>true</c>,
/// so the container ticks it in the post-pass — after every regular updateable (including entity
/// movement controllers attached as updateable children) has finished this frame. The result is
/// that the solver always sees up-to-date positions, regardless of the order in which entities
/// and the solver itself were added.</para>
/// <para><b>No broadphase.</b> The first cut checks every pair every frame. For small scenes
/// (dozens of colliders) this is fine; scenes that scale into the hundreds should expect to
/// add spatial partitioning later.</para>
/// </remarks>
public class CollisionSolver : UpdateableBase
{
    private readonly List<Collider> _colliders = [];
    private Dictionary<Collider, HashSet<Collider>> _overlapsByCollider = [];

    /// <summary>The colliders currently registered with this solver, in registration order.</summary>
    public IReadOnlyList<Collider> Colliders => _colliders;

    /// <inheritdoc/>
    public override bool UpdateLast => true;

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        // Snapshot so event handlers that remove colliders mid-pass don't shift indices.
        Collider[] snapshot = [.. _colliders];
        Dictionary<Collider, HashSet<Collider>> newOverlaps = new(snapshot.Length);
        foreach (Collider collider in snapshot)
        {
            newOverlaps[collider] = [];
        }

        for (int firstIndex = 0; firstIndex < snapshot.Length; firstIndex++)
        {
            Collider first = snapshot[firstIndex];
            if (!first.Active)
            {
                continue;
            }

            Geometry.ICollisionShape shapeFirst = first.WorldShape;

            for (int secondIndex = firstIndex + 1; secondIndex < snapshot.Length; secondIndex++)
            {
                Collider second = snapshot[secondIndex];
                if (!second.Active)
                {
                    continue;
                }
                if (!CategoriesInteract(first, second))
                {
                    continue;
                }

                if (shapeFirst.Overlaps(second.WorldShape))
                {
                    newOverlaps[first].Add(second);
                    newOverlaps[second].Add(first);
                }
            }
        }

        // Build deferred event lists before firing so a handler that removes a collider can't
        // corrupt the dictionary mid-iteration.
        Dictionary<Collider, HashSet<Collider>> oldOverlaps = _overlapsByCollider;
        List<(Collider Self, Collider Other)> enteredEvents = [];
        List<(Collider Self, Collider Other)> exitedEvents = [];

        foreach (KeyValuePair<Collider, HashSet<Collider>> entry in newOverlaps)
        {
            Collider collider = entry.Key;
            HashSet<Collider> newSet = entry.Value;
            if (!oldOverlaps.TryGetValue(collider, out HashSet<Collider>? oldSet))
            {
                oldSet = [];
            }

            foreach (Collider other in newSet)
            {
                if (!oldSet.Contains(other))
                {
                    enteredEvents.Add((collider, other));
                }
            }

            foreach (Collider other in oldSet)
            {
                if (!newSet.Contains(other))
                {
                    exitedEvents.Add((collider, other));
                }
            }
        }

        _overlapsByCollider = newOverlaps;

        foreach ((Collider self, Collider other) in exitedEvents)
        {
            self.RaiseOverlapExited(other);
        }

        foreach ((Collider self, Collider other) in enteredEvents)
        {
            self.RaiseOverlapEntered(other);
        }
    }

    internal void RegisterCollider(Collider collider)
    {
        _colliders.Add(collider);
    }

    internal void UnregisterCollider(Collider collider)
    {
        _colliders.Remove(collider);
    }

    private static bool CategoriesInteract(Collider a, Collider b)
    {
        return (a.CollisionCategory & b.CollideCategories) != 0
            && (b.CollisionCategory & a.CollideCategories) != 0;
    }
}

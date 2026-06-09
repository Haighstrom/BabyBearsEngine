using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Benchmarks;

/// <summary>
/// Isolates the cost of Container's duplicate-membership guard on Add (issue #142): the old linear
/// <see cref="List{T}.Contains"/> scan vs the HashSet-backed O(1) lookup. Entities are plain
/// <see cref="IAddable"/>s — neither renderable nor updateable — so the membership check is the
/// only per-Add work that differs between the two strategies (the real Container's layer-sorted
/// renderable/updateable insertion is a separate O(n) concern that would otherwise mask this).
/// <para>Add time shows the O(n^2)-build vs O(n)-build divergence; <see cref="MemoryDiagnoserAttribute"/>
/// shows the memory the mirror HashSet costs on top of the ordered list.</para>
/// </summary>
[MemoryDiagnoser]
public class ContainerMembershipBenchmarks
{
    private BenchAddable[] _entities = [];

    [Params(100, 1000, 10000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        _entities = new BenchAddable[N];
        for (int i = 0; i < N; i++)
        {
            _entities[i] = new BenchAddable();
        }
    }

    [Benchmark(Baseline = true)]
    public LinearMembership LinearScan()
    {
        LinearMembership container = new();
        foreach (BenchAddable entity in _entities)
        {
            container.Add(entity);
        }
        return container;
    }

    [Benchmark]
    public HashSetMembership HashSetLookup()
    {
        HashSetMembership container = new();
        foreach (BenchAddable entity in _entities)
        {
            container.Add(entity);
        }
        return container;
    }

    // Old strategy: a linear scan of the ordered children list on every Add — O(n) per add,
    // O(n^2) to build the whole container.
    public sealed class LinearMembership
    {
        private readonly List<IAddable> _children = [];

        public void Add(IAddable entity)
        {
            if (_children.Contains(entity))
            {
                throw new InvalidOperationException("Entity already added.");
            }

            _children.Add(entity);
        }
    }

    // New strategy: a HashSet mirrors the ordered list for O(1) membership — O(1) per add, O(n) to
    // build, at the cost of the set's backing storage.
    public sealed class HashSetMembership
    {
        private readonly List<IAddable> _children = [];
        private readonly HashSet<IAddable> _childrenSet = [];

        public void Add(IAddable entity)
        {
            if (_childrenSet.Contains(entity))
            {
                throw new InvalidOperationException("Entity already added.");
            }

            _children.Add(entity);
            _childrenSet.Add(entity);
        }
    }

    private sealed class BenchAddable : IAddable
    {
        public IContainer? Parent { get; set; } = null;

        public bool Exists => Parent is not null;

        public event EventHandler? Added { add { } remove { } }

        public event EventHandler? Removed { add { } remove { } }

        public void Remove() { }
    }
}

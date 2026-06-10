using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Benchmarks;

/// <summary>
/// Measures the per-frame snapshot cost in Container (issue #140): the public Get* accessors, which
/// allocate a fresh List per call, versus the internal reused-buffer Snapshot* accessors. One
/// "frame" produces three snapshots per container — update, update-last, render — across a scene of
/// N containers, mirroring what ContainerEntity / World do every frame. MemoryDiagnoser shows the
/// allocation that the reused buffers eliminate.
/// </summary>
[MemoryDiagnoser]
public class ContainerSnapshotBenchmarks
{
    private const int ChildrenPerContainer = 8;

    private readonly RootParent _root = new();
    private Container[] _containers = [];

    [Params(100, 1000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        _containers = new Container[N];
        for (int i = 0; i < N; i++)
        {
            Container container = new(_root);
            for (int j = 0; j < ChildrenPerContainer; j++)
            {
                container.Add(new BenchEntity(j));
            }
            _containers[i] = container;
        }
    }

    // The loops iterate the result with `var`, so each path enumerates exactly the type its
    // accessor returns — IList for Get (boxed enumerator) and concrete List for Snapshot (struct
    // enumerator) — matching what ContainerEntity / World actually do per frame. `touched` is
    // returned so the JIT can't elide the calls.
    [Benchmark(Baseline = true)]
    public int Allocating_Get()
    {
        int touched = 0;
        foreach (Container container in _containers)
        {
            foreach (var _ in container.GetUpdatables()) { touched++; }
            foreach (var _ in container.GetUpdatablesLast()) { touched++; }
            foreach (var _ in container.GetRenderables()) { touched++; }
        }

        return touched;
    }

    [Benchmark]
    public int Reused_Snapshot()
    {
        int touched = 0;
        foreach (Container container in _containers)
        {
            foreach (var _ in container.SnapshotUpdatables()) { touched++; }
            foreach (var _ in container.SnapshotUpdatablesLast()) { touched++; }
            foreach (var _ in container.SnapshotRenderables()) { touched++; }
        }

        return touched;
    }

    private sealed class RootParent : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    // Both renderable and updateable (and layered) — the common case, so each entity lands in both
    // the _graphics and _updateables lists.
    private sealed class BenchEntity(int layer) : IUpdateable, IRenderable, ILayered
    {
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public int Layer { get; set; } = layer;
        public IContainer? Parent { get; set; } = null;

        public bool Exists => Parent is not null;

        public event EventHandler<LayerChangedEventArgs>? LayerChanged { add { } remove { } }
        public event EventHandler? Added { add { } remove { } }
        public event EventHandler? Removed { add { } remove { } }

        public void Remove() { }
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
        public void Update(double elapsed) { }
    }
}

using System;
using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Benchmarks;

[MemoryDiagnoser]
public class ContainerBenchmarks
{
    private StubRenderable[] _entities = [];
    private Matrix3 _identity;
    private TestScene _populatedScene = null!;

    [Params(10, 50, 100, 500)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        _identity = Matrix3.Identity;
        _populatedScene = new TestScene();
        for (int i = 0; i < N; i++)
        {
            _populatedScene.Add(new StubRenderable(i % 5));
        }
    }

    // Fresh entities per iteration: entities cannot be re-added once they belong to a container.
    [IterationSetup(Targets = new[] { nameof(BuildScene) })]
    public void SetupBuildScene()
    {
        _entities = new StubRenderable[N];
        for (int i = 0; i < N; i++)
        {
            _entities[i] = new StubRenderable(i % 5);
        }
    }

    // Total scene construction: each Add does a linear O(n) layer scan, so the full
    // build is O(n^2/2). InvocationCount(1) because entities cannot be added twice.
    [Benchmark]
    [InvocationCount(1)]
    public TestScene BuildScene()
    {
        var scene = new TestScene();
        foreach (var entity in _entities)
        {
            scene.Add(entity);
        }
        return scene;
    }

    // Per-frame cost: copies the renderable list then visits each entity with a visibility check.
    [Benchmark]
    public void RenderAll()
    {
        _populatedScene.Render(ref _identity, ref _identity);
    }

    private sealed class StubRenderable(int layer) : IRenderable, ILayered
    {
        private IContainer? _parent = null;

        public bool Exists => true;
        public int Layer { get; private set; } = layer;
        public event EventHandler<LayerChangedEventArgs>? LayerChanged;
        public IContainer? Parent => _parent;
        public bool Visible { get; set; } = true;

        public void Remove() { }

        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }

        public void SetLayer(int layer)
        {
            int old = Layer;
            Layer = layer;
            if (old != layer)
            {
                LayerChanged?.Invoke(this, new LayerChangedEventArgs(old, layer));
            }
        }

        public void SetParent(IContainer? parent)
        {
            _parent = parent;
        }
    }

    public sealed class TestScene : ContainerEntity { }
}

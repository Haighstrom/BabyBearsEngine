using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Benchmarks;

[MemoryDiagnoser]
public class RectBenchmarks
{
    private Rect _nonOverlappingRect = new();
    private Rect _overlappingRect = new();
    private Point _pointInside;
    private Point _pointOutside;
    private Rect _rect = new();

    [GlobalSetup]
    public void Setup()
    {
        _nonOverlappingRect = new Rect(200f, 200f, 100f, 100f);
        _overlappingRect = new Rect(50f, 50f, 100f, 100f);
        _pointInside = new Point(50f, 50f);
        _pointOutside = new Point(200f, 200f);
        _rect = new Rect(0f, 0f, 100f, 100f);
    }

    // Miss can short-circuit on the first failing comparison; hit must pass all four.
    [Benchmark]
    public bool Contains_Hit()
    {
        return _rect.Contains(_pointInside);
    }

    [Benchmark]
    public bool Contains_Miss()
    {
        return _rect.Contains(_pointOutside);
    }

    // Intersection allocates a new Rect on the heap.
    [Benchmark]
    public Rect Intersection()
    {
        return Rect.Intersection(_rect, _overlappingRect);
    }

    [Benchmark]
    public bool Intersects_Hit()
    {
        return _rect.Intersects(_overlappingRect);
    }

    [Benchmark]
    public bool Intersects_Miss()
    {
        return _rect.Intersects(_nonOverlappingRect);
    }
}

using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Benchmarks;

[MemoryDiagnoser]
public class PointBenchmarks
{
    private Point _other;
    private Point _point;

    [GlobalSetup]
    public void Setup()
    {
        _other = new Point(1f, 2f);
        _point = new Point(3f, 4f);
    }

    // Clamp calls Length (sqrt) twice: once for Math.Max/Min, once for the scale division.
    [Benchmark]
    public Point Clamp()
    {
        return _point.Clamp(1f, 10f);
    }

    [Benchmark]
    public float DotProduct()
    {
        return _point.DotProduct(_other);
    }

    // Single sqrt — the baseline cost of vector magnitude.
    [Benchmark]
    public float Length()
    {
        return _point.Length;
    }

    // Normal calls Length (sqrt) twice: once to divide X, once to divide Y.
    [Benchmark]
    public Point Normal()
    {
        return _point.Normal;
    }

    [Benchmark]
    public Point Rotate_AroundOrigin()
    {
        return _point.Rotate(45f);
    }

    [Benchmark]
    public Point Rotate_AroundPoint()
    {
        return _point.Rotate(45f, _other);
    }
}

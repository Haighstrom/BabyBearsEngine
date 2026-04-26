using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Benchmarks;

[MemoryDiagnoser]
public class Matrix3Benchmarks
{
    private Matrix3 _a;
    private Matrix3 _b;
    private Point _point;

    [GlobalSetup]
    public void Setup()
    {
        _a = Matrix3.CreateTranslation(100f, 200f);
        _b = Matrix3.CreateRotationAroundZAxis(45f);
        _point = new Point(3f, 4f);
    }

    // Matrix + matrix uses LINQ Zip().ToArray() — allocates an enumerator + float[9].
    [Benchmark]
    public Matrix3 Add_MatrixByMatrix()
    {
        return _a + _b;
    }

    // Projection matrix: 3 composed operations. Called at startup and on window resize.
    [Benchmark]
    public Matrix3 CreateOrtho()
    {
        return Matrix3.CreateOrtho(1920f, 1080f);
    }

    // Gauss-Jordan elimination — the most expensive matrix operation in the engine.
    [Benchmark]
    public Matrix3 Invert()
    {
        return Matrix3.Invert(_a);
    }

    // Matrix * matrix uses direct index arithmetic — allocates only the result float[9].
    [Benchmark]
    public Matrix3 Multiply_MatrixByMatrix()
    {
        return _a * _b;
    }

    // Transform a 2D point: 6 multiplies + 4 adds. Called per vertex in the render loop.
    [Benchmark]
    public Point Multiply_MatrixByPoint()
    {
        return _a * _point;
    }

    // Rotation around a world-space point: 3 matrix creates + 2 multiplies per call.
    // Called once per rotating entity per frame.
    [Benchmark]
    public Matrix3 RotateAroundPoint()
    {
        return Matrix3.RotateAroundPoint(ref _a, 45f, 100f, 100f);
    }
}

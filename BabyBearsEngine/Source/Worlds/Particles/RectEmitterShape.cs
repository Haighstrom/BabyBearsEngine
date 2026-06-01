using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// Emits particles uniformly from anywhere inside the rectangle <see cref="Area"/>, all sharing
/// the same <see cref="Velocity"/>. Use for snowfall over a region, ambient sparkles, smoke
/// rising from a fire area.
/// </summary>
/// <param name="area">Local-space rectangle that bounds the spawn area.</param>
/// <param name="velocity">Initial velocity for every emitted particle, in units per second.</param>
public sealed class RectEmitterShape : IEmitterShape
{
    private Rect _area;

    /// <param name="area">Local-space rectangle that bounds the spawn area. <see cref="Rect.W"/> and <see cref="Rect.H"/> must both be ≥ 0.</param>
    /// <param name="velocity">Initial velocity for every emitted particle, in units per second.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="area"/> has a negative width or height.</exception>
    public RectEmitterShape(Rect area, Point velocity)
    {
        ValidateArea(area);
        _area = area;
        Velocity = velocity;
    }

    /// <summary>Local-space rectangle that bounds the spawn area. Width and height must both be ≥ 0.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a rectangle with negative width or height.</exception>
    public Rect Area
    {
        get => _area;
        set
        {
            ValidateArea(value);
            _area = value;
        }
    }

    public Point Velocity { get; set; }

    public ParticleSpawn Sample(IRandom random)
    {
        Point position = new(
            Area.X + random.Float() * Area.W,
            Area.Y + random.Float() * Area.H);
        return new ParticleSpawn(position, Velocity);
    }

    private static void ValidateArea(Rect area)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(area.W, "area.W");
        ArgumentOutOfRangeException.ThrowIfNegative(area.H, "area.H");
    }
}

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// Specifies how a column or row in a <see cref="GridLayout"/> is sized.
/// Create instances with the <see cref="Fixed"/> or <see cref="Weighted"/> factory methods.
/// </summary>
public readonly struct GridCellSize
{
    private GridCellSize(GridCellSizeKind kind, float value)
    {
        Kind = kind;
        Value = value;
    }

    internal GridCellSizeKind Kind { get; }
    internal float Value { get; }

    /// <summary>A column or row that is always exactly <paramref name="pixels"/> wide or tall.</summary>
    /// <param name="pixels">Size in pixels.</param>
    public static GridCellSize Fixed(float pixels) => new(GridCellSizeKind.Fixed, pixels);

    /// <summary>
    /// A column or row that claims a proportional share of the space remaining after all
    /// <see cref="Fixed"/> columns or rows are allocated.
    /// </summary>
    /// <param name="weight">Relative weight. A value of <c>2f</c> claims twice as much space as a value of <c>1f</c>.</param>
    public static GridCellSize Weighted(float weight) => new(GridCellSizeKind.Weighted, weight);
}

internal enum GridCellSizeKind
{
    Fixed,
    Weighted,
}

namespace BabyBearsEngine;

/// <summary>
/// Selects randomly from a set of items where each item carries a relative weight.
/// Items with higher weights are proportionally more likely to be returned by <see cref="Next"/>.
/// </summary>
/// <typeparam name="T">Type of item to select from.</typeparam>
public class WeightedRandomiser<T>
{
    private readonly IRandom _random;
    private readonly List<(T Item, double Weight)> _items = [];
    private double _totalWeight = 0.0;

    public WeightedRandomiser()
    {
        _random = EngineConfiguration.RandomService;
    }

    /// <param name="random">Random source — inject a stub for deterministic tests.</param>
    public WeightedRandomiser(IRandom random)
    {
        _random = random;
    }

    /// <summary>Number of items currently in the randomiser.</summary>
    public int Count => _items.Count;

    /// <summary>
    /// Adds an item with the given weight. Items with weight 0 are registered but never selected.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="weight"/> is negative.</exception>
    public void Add(T item, double weight)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(weight);
        _items.Add((item, weight));
        _totalWeight += weight;
    }

    /// <summary>Removes all items.</summary>
    public void Clear()
    {
        _items.Clear();
        _totalWeight = 0.0;
    }

    /// <summary>
    /// Returns a randomly selected item, biased by weight. O(N) per call — typical use is small
    /// item counts (loot tables, etc.); for very large N consider a prefix-sum + lower-bound
    /// binary search instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the randomiser is empty.</exception>
    public T Next()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Cannot select from an empty WeightedRandomiser.");
        }

        double r = _random.Double() * _totalWeight;
        double cumulative = 0.0;

        foreach (var (item, weight) in _items)
        {
            cumulative += weight;
            if (r < cumulative)
            {
                return item;
            }
        }

        return _items[^1].Item;
    }
}

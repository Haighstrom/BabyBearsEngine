namespace BabyBearsEngine;

public static class CollectionExtensions
{
    public static List<T> ToList<T>(this T[,] array)
    {
        List<T> list = [];

        for (int j = 0; j < array.GetLength(1); ++j)
        {
            for (int i = 0; i < array.GetLength(0); ++i)
            {
                list.Add(array[i, j]);
            }
        }

        return list;
    }

    public static T[] GetRow<T>(this T[,] array, int row)
    {
        int rowLength = array.GetLength(0);
        var rowVector = new T[rowLength];

        for (int i = 0; i < rowLength; i++)
        {
            rowVector[i] = array[i, row];
        }

        return rowVector;
    }

    public static T[] Combine<T>(this T[] array1, T[] array2)
    {
        return array1.Concat(array2).ToArray();
    }

    public static T[,] Transpose<T>(this T[,] array)
    {
        int n = array.GetLength(0);
        int m = array.GetLength(1);

        var newArray = new T[m, n];

        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < m; ++j)
            {
                newArray[j, i] = array[i, j];
            }
        }

        return newArray;
    }

    public static bool IsEmpty<T>(this IEnumerable<T>? list)
    {
        return list is null || !list.Any();
    }

    public static void Add(this IList<string> list, string s, params object[] args)
    {
        list.Add(string.Format(s, args));
    }

    public static void Add<T>(this IList<T> list, params T[] items)
    {
        foreach (var item in items)
        {
            list.Add(item);
        }
    }

    public static void Add<T>(this IList<T> list, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            list.Add(item);
        }
    }

    public static bool IsEmpty<T>(this T[] l)
    {
        return l == null || l.Length == 0;
    }

    public static List<T> GetRange<T>(this List<T> l, int first)
    {
        return l.GetRange(first, l.Count - first);
    }

    /// <summary>
    /// Left-rotates <paramref name="list"/> in-place by <paramref name="amountShifted"/> positions:
    /// the first <paramref name="amountShifted"/> elements move to the end and later elements shift
    /// forward by the same amount.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amountShifted"/> is ≤ 0.</exception>
    public static void Rotate<T>(this IList<T> list, int amountShifted = 1)
    {
        Ensure.ArgumentPositive(amountShifted, nameof(amountShifted));

        int listSize = list.Count;

        if (listSize <= 1)
        {
            return;
        }

        int steps = amountShifted % listSize;

        if (steps == 0)
        {
            return;
        }

        if (steps < 0)
        {
            steps += listSize;
        }

        var buffer = new T[steps];

        for (int i = 0; i < steps; i++)
        {
            buffer[i] = list[i];
        }

        for (int i = steps; i < listSize; i++)
        {
            list[i - steps] = list[i];
        }

        for (int i = 0; i < steps; i++)
        {
            list[listSize - steps + i] = buffer[i];
        }
    }
}

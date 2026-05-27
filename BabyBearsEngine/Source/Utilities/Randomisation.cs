using System.Text;

namespace BabyBearsEngine;

/// <summary>
/// Static helpers for generating random values, selecting from collections, and shuffling or
/// rotating lists. All methods share a single <see cref="Random"/> instance.
/// </summary>
public static class Randomisation
{
    private static readonly Random s_random = new();

    /// <summary>Returns <c>true</c> with probability <paramref name="chanceOutOfAHundred"/>%.</summary>
    public static bool Chance(float chanceOutOfAHundred) => Rand(0, 100) < chanceOutOfAHundred;

    /// <summary>Returns a randomly selected element from <paramref name="things"/>.</summary>
    public static T Choose<T>(params T[] things) => things[Rand(things.Length)];

    /// <summary>Returns a randomly selected element from <paramref name="things"/>.</summary>
    public static T Choose<T>(List<T> things) => things[Rand(things.Count)];

    /// <summary>Returns a random <c>double</c> in [0, <paramref name="max"/>).</summary>
    public static double RandD(double max) => s_random.NextDouble() * max;

    /// <summary>Returns a random <c>int</c> in [<paramref name="min"/>, <paramref name="max"/>).</summary>
    /// <exception cref="Exception">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public static int Rand(int min, int max)
    {
        if (max < min)
        {
            throw new Exception("cannot have max less than min");
        }

        return min + s_random.Next(max - min);
    }

    /// <summary>Returns a random <c>int</c> in [0, <paramref name="max"/>). Returns 0 when <paramref name="max"/> is 0 or less.</summary>
    public static int Rand(int max)
    {
        if (max <= 0)
        {
            return 0;
        }

        return s_random.Next(max);
    }

    /// <summary>Returns a randomly selected enum value of type <typeparamref name="T"/>.</summary>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> is not an enum type.</exception>
    public static T RandEnum<T>()
        where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new InvalidOperationException($"Generic parameter T must be an enum. Provided was {typeof(T).Name}.");
        }

        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(s_random.Next(values.Length))!;
    }

    /// <summary>Returns a random <see cref="Colour"/> with random R, G, B components and full opacity.</summary>
    public static Colour RandColour() => new((byte)Rand(255), (byte)Rand(255), (byte)Rand(255), 255);

    /// <summary>Returns a random <c>float</c> in [0, <paramref name="max"/>).</summary>
    public static float RandF(int max) => (float)(s_random.NextDouble() * max);

    /// <summary>Returns a random <c>float</c> in [0, <paramref name="max"/>).</summary>
    public static float RandF(float max) => (float)(s_random.NextDouble() * max);

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="min"/>, <paramref name="max"/>].
    /// Returns <paramref name="max"/> when <paramref name="max"/> ≤ <paramref name="min"/>.
    /// </summary>
    public static float RandF(int min, int max)
    {
        if (max <= min)
        {
            return max;
        }
        return
            min + (float)s_random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="min"/>, <paramref name="max"/>].
    /// Returns <paramref name="max"/> when <paramref name="max"/> ≤ <paramref name="min"/>.
    /// </summary>
    public static float RandF(float min, float max)
    {
        if (max <= min)
        {
            return max;
        }
        return min + (float)s_random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="min"/>, <paramref name="max"/>] using an
    /// approximate Gaussian distribution (cosine-based bell curve centred on the midpoint).
    /// Returns <paramref name="max"/> when <paramref name="max"/> ≤ <paramref name="min"/>.
    /// </summary>
    public static float RandGaussianApprox(float min, float max)
    {
        if (max <= min)
        {
            return max;
        }

        //guassian = approx (1 + cos x)/2PI [SD = 1]
        //therefore (x+sinx)/2PI is integral, x from 0 to 2 PI gives 0-1 with normal distribution ish results

        double zeroTo2PI = 2 * Math.PI * s_random.NextDouble();
        double zeroToOne = (zeroTo2PI + Math.Sin(zeroTo2PI)) / (2 * Math.PI);
        return min + (float)(zeroToOne * (max - min));
    }

    /// <summary>Returns a random uppercase ASCII string of <paramref name="chars"/> characters.</summary>
    public static string RandUpperCaseString(int chars)
    {
        string def = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder ret = new();
        for (int i = 0; i < chars; i++)
        {
            ret.Append(def.AsSpan(s_random.Next(def.Length), 1));
        }
        return ret.ToString();
    }

    /// <summary>Shuffles <paramref name="array"/> in-place using Fisher–Yates and returns it.</summary>
    public static T[] Shuffle<T>(T[] array)
    {
        for (int i = array.Length; i > 1; i--)
        {
            // pick random element 0 <= j < i
            int j = Rand(i);
            // swap i and j
            (array[i - 1], array[j]) = (array[j], array[i - 1]);
        }
        return array;
    }

    /// <summary>Shuffles <paramref name="list"/> in-place using Fisher–Yates and returns it.</summary>
    public static List<T> Shuffle<T>(List<T> list)
    {
        for (int i = list.Count; i > 1; i--)
        {
            // pick random element 0 <= j < i
            int j = Rand(i);
            // swap i and j
            (list[i - 1], list[j]) = (list[j], list[i - 1]);
        }
        return list;
    }

    /// <summary>Returns a randomly selected element from <paramref name="list"/>.</summary>
    public static T RandomElement<T>(this IList<T> list)
    {
        return list[Rand(list.Count)];
    }

    /// <summary>Shuffles <paramref name="list"/> in-place using Fisher–Yates.</summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = s_random.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
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

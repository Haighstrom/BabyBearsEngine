using System.Threading;

namespace BabyBearsEngine;

/// <summary>Helpers for calling a method multiple times or retrying on failure.</summary>
public static class Repeat
{
    /// <summary>
    /// Calls <paramref name="method"/> up to <paramref name="maxTries"/> times. If it throws,
    /// waits <paramref name="waitTime"/> before the next attempt. Rethrows the last exception
    /// once all attempts are exhausted.
    /// </summary>
    public static void TryMethod(Action method, int maxTries, TimeSpan waitTime)
    {
        int numTries = 0;
        while (numTries < maxTries)
        {
            try
            {
                method();
                return;
            }
            catch
            {
                numTries++;
                if (numTries == maxTries)
                {
                    throw;
                }
                Thread.Sleep(waitTime);
            }
        }

        //todo: throw new System.Diagnostics.UnreachableException(); Needs .NET 7
        throw new Exception("Unreachable code");
    }

    /// <summary>
    /// Calls <paramref name="method"/> up to <paramref name="maxTries"/> times and returns its result.
    /// If it throws, waits <paramref name="waitTime"/> before the next attempt. Rethrows the last
    /// exception once all attempts are exhausted.
    /// </summary>
    public static TResult TryMethod<TResult>(Func<TResult> method, int maxTries, TimeSpan waitTime)
    {
        int numTries = 0;
        while (numTries < maxTries)
        {
            try
            {
                var result = method();
                return result;
            }
            catch
            {
                numTries++;
                if (numTries == maxTries)
                {
                    throw;
                }
                Thread.Sleep(waitTime);
            }
        }

        throw new Exception("Unreachable code");
    }

    /// <summary>
    /// Calls <paramref name="method"/> exactly <paramref name="times"/> times.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="times"/> is negative.</exception>
    public static void CallMethod(Action method, int times)
    {
        Ensure.ArgumentNotNegative(times, nameof(times));

        for (int i = 0; i < times; ++i)
        {
            method();
        }
    }
}

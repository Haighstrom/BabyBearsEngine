using System.Threading;
using BabyBearsEngine.Diagnostics;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Helper for surfacing OpenAL errors. OpenAL reports failures via <c>alGetError</c> rather
/// than exceptions, so every AL call site is responsible for checking — and silently dropping
/// the error if it doesn't. This wrapper logs the first occurrence of each (operation) error
/// at <see cref="LogLevel.Warning"/> and then deduplicates subsequent identical reports, so
/// a persistent failure (e.g. bad source ID) produces one log line rather than one per frame.
/// </summary>
internal static class OpenALErrorCheck
{
    private static readonly Lock s_lock = new();
    private static readonly HashSet<(string Operation, ALError Error)> s_seen = [];

    /// <summary>
    /// Calls <c>AL.GetError</c> and, if it reports an error, logs a warning naming the
    /// supplied <paramref name="operation"/> and the AL error code. Each unique
    /// <c>(operation, error)</c> pair is logged once per process; further occurrences of the
    /// same pair are silently dropped.
    /// </summary>
    public static void Check(string operation)
    {
        ALError error = AL.GetError();
        if (error == ALError.NoError)
        {
            return;
        }

        bool firstOccurrence;
        lock (s_lock)
        {
            firstOccurrence = s_seen.Add((operation, error));
        }

        if (firstOccurrence)
        {
            Logger.Warning($"Audio: AL error during {operation}: {error}.");
        }
    }

    /// <summary>
    /// Forget every previously-logged <c>(operation, error)</c> pair — the next occurrence
    /// will log again. Intended for test isolation; production code should not call this.
    /// </summary>
    internal static void ResetDedupe()
    {
        lock (s_lock)
        {
            s_seen.Clear();
        }
    }
}

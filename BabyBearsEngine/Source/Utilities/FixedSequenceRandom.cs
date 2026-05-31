namespace BabyBearsEngine;

/// <summary>
/// Test fake for <see cref="IRandom"/> that returns pre-loaded sequences of integers and
/// doubles. Each call to <see cref="Int"/> dequeues the next integer; each call to
/// <see cref="Double"/> dequeues the next double. Use this to write deterministic tests
/// around code that consumes <see cref="IRandom"/>.
/// </summary>
/// <remarks>
/// <para>The fake does <b>not</b> clamp returned ints to the requested
/// <c>[min, max)</c> range — what you queue is what callers receive. This is deliberate:
/// it keeps tests readable ("if the random returns 3, the result is X") and surfaces test
/// bugs immediately when a queued value falls outside the production code's expected range.</para>
///
/// <para>Run-out behaviour: <see cref="Int"/> and <see cref="Double"/> throw
/// <see cref="InvalidOperationException"/> with a descriptive message when their queue is
/// empty. Tests should preload enough values for the call counts they intend to exercise;
/// a thrown empty-queue exception usually means the production code made more random calls
/// than the test expected, which is itself a useful signal.</para>
/// </remarks>
public sealed class FixedSequenceRandom(IEnumerable<int>? ints = null, IEnumerable<double>? doubles = null) : IRandom
{
    private readonly Queue<int> _ints = ints is null ? new Queue<int>() : new Queue<int>(ints);
    private readonly Queue<double> _doubles = doubles is null ? new Queue<double>() : new Queue<double>(doubles);

    /// <summary>Number of integers still queued.</summary>
    public int IntsRemaining => _ints.Count;

    /// <summary>Number of doubles still queued.</summary>
    public int DoublesRemaining => _doubles.Count;

    /// <summary>Convenience factory for tests that only stub <see cref="Double"/>.</summary>
    public static FixedSequenceRandom FromDoubles(params double[] values) => new(doubles: values);

    /// <summary>Convenience factory for tests that only stub <see cref="Int"/>.</summary>
    public static FixedSequenceRandom FromInts(params int[] values) => new(ints: values);

    /// <inheritdoc/>
    /// <remarks>
    /// The supplied <paramref name="minInclusive"/> and <paramref name="maxExclusive"/> are
    /// accepted for interface conformance but ignored — the queued value is returned verbatim.
    /// </remarks>
    public int Int(int minInclusive, int maxExclusive)
    {
        if (_ints.Count == 0)
        {
            throw new InvalidOperationException(
                $"FixedSequenceRandom.Int was called but no more ints are queued (asked for [{minInclusive}, {maxExclusive})).");
        }
        return _ints.Dequeue();
    }

    /// <inheritdoc/>
    public double Double()
    {
        if (_doubles.Count == 0)
        {
            throw new InvalidOperationException(
                "FixedSequenceRandom.Double was called but no more doubles are queued.");
        }
        return _doubles.Dequeue();
    }
}

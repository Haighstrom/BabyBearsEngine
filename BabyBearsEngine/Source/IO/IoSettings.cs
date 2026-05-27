namespace BabyBearsEngine.IO;

/// <summary>
/// Configuration for file IO operations. Set <see cref="Files.Settings"/> at startup to override defaults.
/// </summary>
public class IoSettings
{
    /// <summary>Number of times a failed IO operation is retried before the exception is rethrown. Default is 5.</summary>
    public int RetryCount { get; init; } = 5;

    /// <summary>How long to wait between retry attempts. Default is 10 ms.</summary>
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMilliseconds(10);

    /// <summary>Returns a default <see cref="IoSettings"/> instance.</summary>
    public static IoSettings Default => new();
}

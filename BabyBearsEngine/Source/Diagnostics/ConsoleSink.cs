namespace BabyBearsEngine.Diagnostics;

/// <remarks>
/// Not internally thread-safe — the surrounding <see cref="Logger"/> serialises calls into the
/// sink via its own lock. Removing the per-sink lock dropped two-thirds of the lock acquisitions
/// from each log call (three sinks each taking their own lock → one lock around all three).
/// </remarks>
internal sealed class ConsoleSink(bool colourise) : ILogSink
{
    public void Write(LogLevel level, string message, Exception? exception)
    {
        ConsoleColor previous = Console.ForegroundColor;

        try
        {
            if (colourise)
            {
                Console.ForegroundColor = ColourFor(level);
            }

            Console.WriteLine(message);

            if (exception is not null)
            {
                Console.WriteLine(exception);
            }
        }
        finally
        {
            if (colourise)
            {
                Console.ForegroundColor = previous;
            }
        }
    }

    private static ConsoleColor ColourFor(LogLevel level) => level switch
    {
        LogLevel.Verbose => ConsoleColor.DarkGray,
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Fatal => ConsoleColor.Magenta,
        _ => Console.ForegroundColor,
    };
}

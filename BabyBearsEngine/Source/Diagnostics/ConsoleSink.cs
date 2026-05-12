using System.Threading;

namespace BabyBearsEngine.Diagnostics;

internal sealed class ConsoleSink(bool colourise) : ILogSink
{
    private static readonly Lock s_lock = new();

    public void Write(LogLevel level, string message, Exception? exception)
    {
        lock (s_lock)
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

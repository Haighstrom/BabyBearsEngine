using System.IO;
using Microsoft.Extensions.Logging;

namespace BabyBearsEngine.Source.Diagnostics;

public static class Logger
{
    private static readonly ILogger s_logger = GetLoggerSimple();

    private static ILogger GetLoggerSimple()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Program");
        logger.LogInformation("Hello World! Logging is {Description}.", "fun");

        var logLocation = Path.Combine(Directory.GetCurrentDirectory(), "logs");

        if (!Directory.Exists(logLocation))
        {
            Directory.CreateDirectory(logLocation);
        }

        factory.AddFile(logLocation);

        return logger;
    }

    public static void Log(string message)
    {
        s_logger.LogInformation(message);
    }
}

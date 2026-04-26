using Microsoft.Extensions.Logging;

namespace BabyBearsEngine.Diagnostics;

public sealed class FileLoggerProvider(string path) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(path);
    }

    public void Dispose()
    {
    }
}

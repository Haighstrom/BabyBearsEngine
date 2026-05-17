using System.Text.Json;
using BabyBearsEngine.IO;

namespace BabyBearsEngine;

/// <summary>
/// Configuration for the IO subsystem: retry behaviour for disk operations and the JSON
/// serialisation options used when saving and loading game data.
/// </summary>
public record class IoSettings()
{
    /// <summary>The default IO settings.</summary>
    public static IoSettings Default => new();

    private static JsonSerializerOptions GetDefaultJsonSerializerOptions()
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        options.Converters.Add(new TwoDimensionalArrayConverter());

        return options;
    }

    /// <summary>
    /// Number of times a failed IO operation is retried before the error is surfaced. Defaults to 5.
    /// </summary>
    public int RetriesForIoOperations { get; set; } = 5;

    /// <summary>
    /// Delay in milliseconds between retries of a failed IO operation. Defaults to 10.
    /// </summary>
    public int MilisecondsBetweenRetriesForIoOperations { get; set; } = 10;

    /// <summary>
    /// <see cref="JsonSerializerOptions"/> used when serialising and deserialising save data.
    /// Defaults to indented output with field inclusion and support for 2D arrays.
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; set; } = GetDefaultJsonSerializerOptions();
}

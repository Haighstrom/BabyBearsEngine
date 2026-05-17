using System;
using System.Text.Json;

namespace BabyBearsEngine.IO;

/// <summary>
/// Static facade for JSON serialization and deserialization. All methods use <see cref="DefaultOptions"/>
/// unless an explicit <see cref="JsonSerializerOptions"/> overload is called.
/// </summary>
public static class Json
{
    /// <summary>
    /// Pre-configured <see cref="JsonSerializerOptions"/> with indented output and BBE type converters
    /// registered: <see cref="ColourJsonConverter"/>, <see cref="PointJsonConverter"/>,
    /// <see cref="RectJsonConverter"/>, and <see cref="TwoDimensionalArrayConverter"/>.
    /// </summary>
    public static JsonSerializerOptions DefaultOptions { get; } = BuildDefaultOptions();

    private static JsonSerializerOptions BuildDefaultOptions()
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
        };
        options.Converters.Add(new ColourJsonConverter());
        options.Converters.Add(new PointJsonConverter());
        options.Converters.Add(new RectJsonConverter());
        options.Converters.Add(new TwoDimensionalArrayConverter());
        return options;
    }

    /// <summary>Serializes <paramref name="obj"/> to a JSON string using <see cref="DefaultOptions"/>.</summary>
    public static string Serialize<T>(T obj) =>
        JsonSerializer.Serialize(obj, DefaultOptions);

    /// <summary>Serializes <paramref name="obj"/> to a JSON string using the supplied <paramref name="options"/>.</summary>
    public static string Serialize<T>(T obj, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(obj, options);

    /// <summary>
    /// Deserializes a JSON string to <typeparamref name="T"/> using <see cref="DefaultOptions"/>.
    /// Throws <see cref="JsonException"/> if the input is malformed or produces a null result.
    /// </summary>
    public static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, DefaultOptions)
            ?? throw new JsonException($"Deserializing JSON produced null for type {typeof(T).Name}.");

    /// <summary>
    /// Deserializes a JSON string to <typeparamref name="T"/> using the supplied <paramref name="options"/>.
    /// Throws <see cref="JsonException"/> if the input is malformed or produces a null result.
    /// </summary>
    public static T Deserialize<T>(string json, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<T>(json, options)
            ?? throw new JsonException($"Deserializing JSON produced null for type {typeof(T).Name}.");

    /// <summary>
    /// Attempts to deserialize a JSON string to <typeparamref name="T"/> using <see cref="DefaultOptions"/>.
    /// Returns <c>default</c> if the input is malformed or deserialization fails for any reason.
    /// </summary>
    public static T? TryDeserialize<T>(string json)
    {
        try
        {
            return Deserialize<T>(json);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to <typeparamref name="T"/> using the supplied <paramref name="options"/>.
    /// Returns <c>default</c> if the input is malformed or deserialization fails for any reason.
    /// </summary>
    public static T? TryDeserialize<T>(string json, JsonSerializerOptions options)
    {
        try
        {
            return Deserialize<T>(json, options);
        }
        catch (Exception)
        {
            return default;
        }
    }
}

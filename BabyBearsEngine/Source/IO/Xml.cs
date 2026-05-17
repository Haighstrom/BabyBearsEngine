using System;
using System.IO;
using System.Xml.Serialization;

namespace BabyBearsEngine.IO;

/// <summary>Static facade for XML serialization and deserialization using <see cref="XmlSerializer"/>.</summary>
public static class Xml
{
    /// <summary>Serializes <paramref name="obj"/> to an XML string.</summary>
    public static string Serialize<T>(T obj)
    {
        XmlSerializer serializer = new(typeof(T));
        using StringWriter writer = new();
        serializer.Serialize(writer, obj);
        return writer.ToString();
    }

    /// <summary>
    /// Deserializes an XML string to <typeparamref name="T"/>.
    /// Throws <see cref="InvalidOperationException"/> if the input is malformed or produces a null result.
    /// </summary>
    public static T Deserialize<T>(string xml)
    {
        XmlSerializer serializer = new(typeof(T));
        using StringReader reader = new(xml);
        return (T)(serializer.Deserialize(reader)
            ?? throw new InvalidOperationException($"Deserializing XML produced null for type {typeof(T).Name}."));
    }

    /// <summary>
    /// Attempts to deserialize an XML string to <typeparamref name="T"/>.
    /// Returns <c>default</c> if the input is malformed or deserialization fails for any reason.
    /// </summary>
    public static T? TryDeserialize<T>(string xml)
    {
        try
        {
            return Deserialize<T>(xml);
        }
        catch (Exception)
        {
            return default;
        }
    }
}

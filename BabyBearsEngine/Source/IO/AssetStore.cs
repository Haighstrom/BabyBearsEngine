using System.IO;

namespace BabyBearsEngine.IO;

/// <summary>Static access point for the <see cref="IAssetReader"/> the engine loads assets through.</summary>
public static class AssetStore
{
    private static IAssetReader s_reader = new FileAssetReader();

    /// <summary>The reader used to open assets. Set at startup, before any asset loads; defaults to plain files on disk.</summary>
    public static IAssetReader Reader
    {
        get => s_reader;
        set => s_reader = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Reads the whole asset at <paramref name="path"/> through <see cref="Reader"/> into a byte array.</summary>
    public static byte[] ReadAllBytes(string path)
    {
        using Stream stream = s_reader.OpenRead(path);
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}

using System.IO;

namespace BabyBearsEngine.IO;

/// <summary>The default <see cref="IAssetReader"/>: reads assets as plain files on disk.</summary>
public sealed class FileAssetReader : IAssetReader
{
    /// <inheritdoc />
    public bool Exists(string path) => File.Exists(path);

    /// <inheritdoc />
    public Stream OpenRead(string path) => File.OpenRead(path);
}

using System.IO;
using BabyBearsEngine.IO;

namespace BabyBearsEngine.Tests.Unit;

/// <summary>Test IAssetReader serving assets from memory, keyed by logical path.</summary>
internal sealed class InMemoryAssetReader : IAssetReader
{
    private readonly Dictionary<string, byte[]> _assets = new();

    public List<string> OpenedPaths { get; } = new();

    public InMemoryAssetReader Add(string path, byte[] content)
    {
        _assets[path] = content;
        return this;
    }

    public bool Exists(string path) => _assets.ContainsKey(path);

    public Stream OpenRead(string path)
    {
        if (!_assets.TryGetValue(path, out byte[]? content))
        {
            throw new FileNotFoundException("Asset not found.", path);
        }

        OpenedPaths.Add(path);
        return new MemoryStream(content);
    }
}

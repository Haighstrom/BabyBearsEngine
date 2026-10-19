using System.IO;

namespace BabyBearsEngine.IO;

/// <summary>Opens asset files by path, so a game can swap in a reader that unscrambles or unpacks shipped assets.</summary>
public interface IAssetReader
{
    /// <summary>Returns true if an asset exists at <paramref name="path"/>.</summary>
    bool Exists(string path);

    /// <summary>Opens the asset at <paramref name="path"/> for reading. Throws <see cref="FileNotFoundException"/> if it does not exist.</summary>
    Stream OpenRead(string path);
}

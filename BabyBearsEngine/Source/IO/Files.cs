using System.IO;
using System.Threading;

namespace BabyBearsEngine.IO;

/// <summary>
/// Static facade for file and directory operations. All file reads and writes are retried on
/// <see cref="IOException"/> according to <see cref="Settings"/>, which guards against transient
/// file locks from antivirus software, sync tools, etc.
/// </summary>
public static class Files
{
    /// <summary>
    /// Controls how many times a failed IO operation is retried and how long to wait between attempts.
    /// Set this at startup; defaults to <see cref="IoSettings.Default"/>.
    /// </summary>
    public static IoSettings Settings { get; set; } = IoSettings.Default;

    // Path helpers

    /// <summary>
    /// Returns the path to a per-application folder inside the current user's roaming AppData directory,
    /// e.g. <c>%APPDATA%\MyGame</c>. The directory is not created automatically.
    /// </summary>
    public static string AppDataDirectory(string appName) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

    // Append

    /// <summary>Appends each line to <paramref name="path"/>, creating the file if it does not exist.</summary>
    public static void AppendLines(string path, IEnumerable<string> lines) =>
        Retry(() => File.AppendAllLines(path, lines));

    /// <summary>Appends <paramref name="text"/> to <paramref name="path"/>, creating the file if it does not exist.</summary>
    public static void AppendText(string path, string text) =>
        Retry(() => File.AppendAllText(path, text));

    // Copy

    /// <summary>Copies a single file. Throws if the destination already exists.</summary>
    public static void CopyFile(string sourceFile, string destinationFile) =>
        Retry(() => File.Copy(sourceFile, destinationFile));

    /// <summary>Copies files from <paramref name="sourceDirectory"/> to <paramref name="destinationDirectory"/> according to <paramref name="options"/>.</summary>
    public static void CopyFiles(string sourceDirectory, string destinationDirectory, CopyOptions options)
    {
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        SearchOption so = (options & CopyOptions.BringAll) > 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        if ((options & CopyOptions.BringFolders) > 0)
        {
            foreach (string dir in Directory.GetDirectories(sourceDirectory, "*", so))
            {
                Directory.CreateDirectory(Path.Combine(destinationDirectory, Path.GetRelativePath(sourceDirectory, dir)));
            }
        }

        foreach (string file in Directory.GetFiles(sourceDirectory, "*", so))
        {
            Retry(() => File.Copy(file, Path.Combine(destinationDirectory, Path.GetRelativePath(sourceDirectory, file)), (options & CopyOptions.Overwrite) > 0));
        }
    }

    // Create / Delete

    /// <summary>Creates a directory. Returns <c>false</c> if it already exists.</summary>
    public static bool CreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return false;
        }

        Directory.CreateDirectory(path);
        return true;
    }

    /// <summary>Deletes a directory and all its contents. No-ops if the directory does not exist.</summary>
    public static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        Directory.Delete(path, true);
    }

    /// <summary>Deletes a file. No-ops if the file does not exist.</summary>
    public static void DeleteFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        Retry(() => File.Delete(path));
    }

    // Exists

    /// <summary>Returns <c>true</c> if the directory exists.</summary>
    public static bool DirectoryExists(string path) =>
        Directory.Exists(path);

    // ExecutingDirectory

    /// <summary>Returns the directory containing the currently executing assembly.</summary>
    public static string ExecutingDirectory() =>
        AppContext.BaseDirectory;

    // FileExists

    /// <summary>Returns <c>true</c> if the file exists.</summary>
    public static bool FileExists(string path) =>
        File.Exists(path);

    // Get

    /// <summary>Returns the absolute paths of all directories under <paramref name="path"/>.</summary>
    public static List<string> GetDirectories(string path, bool includeSubDirectories) =>
        GetDirectories(path, includeSubDirectories, "*");

    /// <summary>Returns the absolute paths of all directories under <paramref name="path"/> that match <paramref name="searchPattern"/>.</summary>
    public static List<string> GetDirectories(string path, bool includeSubDirectories, string searchPattern)
    {
        SearchOption searchOption = includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return [.. Directory.GetDirectories(path, searchPattern, searchOption).Select(Path.GetFullPath)];
    }

    /// <summary>Extracts the directory component from a file path. Returns <c>null</c> if the path is a root.</summary>
    public static string? GetDirectoryFromFilePath(string path)
    {
        Ensure.ArgumentNotNullOrEmpty(path, "path");

        try
        {
            return Path.GetDirectoryName(path);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is PathTooLongException || ex is NotSupportedException)
        {
            throw new ArgumentException($"Invalid path: {path}", nameof(path), ex);
        }
    }

    /// <summary>Returns the absolute paths of all files under <paramref name="path"/>.</summary>
    public static List<string> GetFiles(string path, bool includeSubDirectories, string searchPattern = "*")
    {
        SearchOption searchOption = includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return [.. Directory.GetFiles(path, searchPattern, searchOption).Select(Path.GetFullPath)];
    }

    // Read CSV

    /// <summary>
    /// Reads a CSV file and returns its contents as a 2D array of <typeparamref name="T"/>.
    /// Values are converted using <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static T[,] ReadCsvFile<T>(string path, char separator = ',') where T : IConvertible =>
        Csv.Deserialize<T>(ReadText(path), separator);

    /// <summary>
    /// Reads a CSV file whose first row contains column headers and returns the headers and data separately.
    /// </summary>
    public static (string[] Headers, T[,] Data) ReadCsvFileWithHeader<T>(string path, char separator = ',') where T : IConvertible =>
        Csv.DeserializeWithHeader<T>(ReadText(path), separator);

    // Read JSON

    /// <summary>
    /// Reads a JSON file and deserializes it to <typeparamref name="T"/> using <see cref="Json.DefaultOptions"/>.
    /// Throws if the file does not exist or the content is malformed.
    /// </summary>
    public static T ReadJsonFile<T>(string path) =>
        Json.Deserialize<T>(ReadText(path));

    // Read Lines

    /// <summary>Reads all lines from <paramref name="path"/> and returns them as an array.</summary>
    public static string[] ReadLines(string path) =>
        Retry(() => File.ReadAllLines(path));

    // Read Text

    /// <summary>Reads the full text content of <paramref name="path"/>.</summary>
    public static string ReadText(string path) =>
        Retry(() => File.ReadAllText(path));

    // Read XML

    /// <summary>
    /// Reads an XML file and deserializes it to <typeparamref name="T"/>.
    /// Throws if the file does not exist or the content is malformed.
    /// </summary>
    public static T ReadXmlFile<T>(string path) =>
        Xml.Deserialize<T>(ReadText(path));

    // Retry (private)

    private static void Retry(Action action)
    {
        int remaining = Settings.RetryCount;
        while (true)
        {
            try
            {
                action();
                return;
            }
            catch (IOException) when (remaining-- > 0)
            {
                Thread.Sleep(Settings.RetryDelay);
            }
        }
    }

    private static T Retry<T>(Func<T> action)
    {
        int remaining = Settings.RetryCount;
        while (true)
        {
            try
            {
                return action();
            }
            catch (IOException) when (remaining-- > 0)
            {
                Thread.Sleep(Settings.RetryDelay);
            }
        }
    }

    // Try Read

    /// <summary>
    /// Attempts to deserialize a JSON file to <typeparamref name="T"/>.
    /// Returns <c>default</c> if the file does not exist or the content cannot be deserialized.
    /// </summary>
    public static T? TryReadJsonFile<T>(string path)
    {
        try
        {
            return ReadJsonFile<T>(path);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to read the full text content of <paramref name="path"/>.
    /// Returns <c>null</c> if the file does not exist or cannot be read.
    /// </summary>
    public static string? TryReadText(string path)
    {
        try
        {
            return ReadText(path);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize an XML file to <typeparamref name="T"/>.
    /// Returns <c>default</c> if the file does not exist or the content cannot be deserialized.
    /// </summary>
    public static T? TryReadXmlFile<T>(string path)
    {
        try
        {
            return ReadXmlFile<T>(path);
        }
        catch (Exception)
        {
            return default;
        }
    }

    // Write CSV

    /// <summary>
    /// Writes a 2D array to a CSV file. Values are formatted using <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static void WriteCsvFile<T>(string path, T[,] data, char separator = ',') where T : IConvertible =>
        WriteText(path, Csv.Serialize(data, separator));

    /// <summary>
    /// Writes a 2D array to a CSV file, prepending <paramref name="headers"/> as the first row.
    /// </summary>
    public static void WriteCsvFileWithHeader<T>(string path, IEnumerable<string> headers, T[,] data, char separator = ',') where T : IConvertible =>
        WriteText(path, Csv.SerializeWithHeader(headers, data, separator));

    // Write JSON

    /// <summary>
    /// Serializes <paramref name="data"/> to JSON using <see cref="Json.DefaultOptions"/> and writes it to <paramref name="path"/>.</summary>
    public static void WriteJsonFile<T>(string path, T data) =>
        WriteText(path, Json.Serialize(data));

    // Write Lines

    /// <summary>Writes <paramref name="lines"/> to <paramref name="path"/>, overwriting any existing content.</summary>
    public static void WriteLines(string path, IEnumerable<string> lines) =>
        Retry(() => File.WriteAllLines(path, lines));

    // Write Text

    /// <summary>Writes <paramref name="text"/> to <paramref name="path"/>, overwriting any existing content.</summary>
    public static void WriteText(string path, string text) =>
        Retry(() => File.WriteAllText(path, text));

    // Write XML

    /// <summary>Serializes <paramref name="data"/> to XML and writes it to <paramref name="path"/>.</summary>
    public static void WriteXmlFile<T>(string path, T data) =>
        WriteText(path, Xml.Serialize(data));
}

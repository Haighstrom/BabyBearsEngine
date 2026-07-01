namespace BabyBearsEngine.Input;

/// <summary>
/// System clipboard access for text. This reflects the OS-level clipboard shared with other
/// applications, not an engine-internal buffer.
/// </summary>
public interface IClipboard
{
    /// <summary>Returns the current text on the system clipboard, or an empty string if the clipboard is empty or holds non-text data.</summary>
    string GetText();

    /// <summary>Replaces the system clipboard's contents with <paramref name="text"/>.</summary>
    void SetText(string text);
}

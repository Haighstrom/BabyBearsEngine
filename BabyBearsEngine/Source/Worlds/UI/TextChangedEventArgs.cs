namespace BabyBearsEngine.Worlds.UI;

/// <summary>Event data for <see cref="TextInputBox.TextChanged"/>.</summary>
public sealed class TextChangedEventArgs(string oldText, string newText) : EventArgs
{
    /// <summary>The text before the change.</summary>
    public string OldText { get; } = oldText;

    /// <summary>The text after the change.</summary>
    public string NewText { get; } = newText;
}

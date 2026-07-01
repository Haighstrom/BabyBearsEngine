using BabyBearsEngine.Input;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKClipboardAdapter(OpenTKGameEngine engine) : IClipboard
{
    // GLFW (via GameWindow.ClipboardString) reports null when the clipboard holds no text
    // (empty or non-text data) rather than an empty string; normalise that here so callers
    // never have to null-check.
    public string GetText() => engine.ClipboardString ?? string.Empty;

    public void SetText(string text) => engine.ClipboardString = text;
}

using BabyBearsEngine.Input;

namespace BabyBearsEngine;

/// <summary>
/// Static facade over the installed <see cref="IClipboard"/> service. All members route through
/// <c>EngineConfiguration.ClipboardService</c>; tests substitute a fake there to exercise consumers
/// without a real system clipboard. Throws <see cref="InvalidOperationException"/> if accessed before the engine is initialised.
/// </summary>
/// <remarks>
/// The engine supports a single window per process, so there is one shared clipboard binding; running
/// two games concurrently in the same process is not supported.
/// </remarks>
public static class Clipboard
{
    private static IClipboard Implementation => EngineConfiguration.ClipboardService;

    /// <inheritdoc cref="IClipboard.GetText()"/>
    public static string GetText() => Implementation.GetText();

    /// <inheritdoc cref="IClipboard.SetText(string)"/>
    public static void SetText(string text) => Implementation.SetText(text);
}

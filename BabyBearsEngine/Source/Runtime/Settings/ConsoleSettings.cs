using System.Diagnostics;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine;

/// <summary>
/// A group of settings for instantiating a ConsoleManager
/// </summary>
public record class ConsoleSettings()
{
    /// <summary>
    /// Default console window width in pixels. Wide enough for the 70-character section headers
    /// the Logger emits, with margin for the scrollbar and window chrome.
    /// </summary>
    public const int DefaultWidth = 550;

    /// <summary>
    /// The default console settings.
    /// </summary>
    public static ConsoleSettings Default => new();

    /// <summary>
    /// When true, log messages written to the console are colourised by severity (Warning=yellow,
    /// Error=red, etc). Defaults to true. Use <see cref="LogSettings.MessageMetadata"/> to control
    /// the timestamp and level prefixes themselves.
    /// </summary>
    public bool ColouriseLogOutput { get; set; } = true;

    /// <summary>
    /// Whether the console should be shown. Defaults to true if a debugger is being used, false otherwise.
    /// </summary>
    public bool ShowConsoleWindow { get; set; } = Debugger.IsAttached; //better than #if DEBUG because may be using Release version of this dll even if Debug in the application

    /// <summary>
    /// The x-coordinate of the top left position of the console. Defaults to the top left of the screen.
    /// </summary>
    public int X { get; set; } = 0;

    /// <summary>
    /// The y-coordinate of the top left position of the console. Defaults to the top left of the screen.
    /// </summary>
    public int Y { get; set; } = 0;

    /// <summary>
    /// The width of the console in pixels. Defaults to 550.
    /// </summary>
    public int Width { get; set; } = DefaultWidth;

    /// <summary>
    /// The height of the console in pixels. Defaults to the full working-area height of the screen.
    /// </summary>
    public int Height { get; set; } = ConsoleWindow.MaxHeight;
}

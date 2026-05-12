using System.Diagnostics;

namespace BabyBearsEngine;

/// <summary>
/// A group of settings for instantiating a ConsoleManager
/// </summary>
public record class ConsoleSettings()
{
    // Wide enough to fit the 70-character section headers/banners that the Logger emits, with
    // margin for the scrollbar and window chrome at typical Windows console font sizes.
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
    /// The height of the console in pixels. Defaults to the height of the window.
    /// </summary>
    public int Height { get; set; } = 300;//new ConsoleWindow().MaxHeight;
}

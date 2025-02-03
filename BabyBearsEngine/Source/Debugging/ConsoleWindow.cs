using System.Runtime.InteropServices;

namespace BabyBearsEngine.Source.Debugging;

public static partial class ConsoleWindow
{
    /// <summary>
    /// Allocates a new console for the calling process.
    /// </summary>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    /// <summary>
    /// Detaches the calling process from its console.
    /// </summary>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FreeConsole();

    public static void Open()
    {
        AllocConsole();
    }
}

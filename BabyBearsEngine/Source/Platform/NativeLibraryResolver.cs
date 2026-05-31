using System.IO;
using System.Runtime.InteropServices;

namespace BabyBearsEngine.Platform;

/// <summary>
/// Eagerly loads native libraries we ship (currently just OpenAL) at startup, so that subsequent
/// P/Invoke calls find them already resident in the process. Call <see cref="Initialise"/> once,
/// before any P/Invoke into a shipped native library.
///
/// On Windows the DLL is normally copied next to the EXE by the standard build output and the OS
/// finds it without this helper. The probe also covers the test-host case (where the app dir is
/// the .NET SDK's testhost rather than the test assembly's bin folder) and deployments that placed
/// the file only under <c>runtimes/...</c>.
/// </summary>
internal static class NativeLibraryResolver
{
    private static bool s_initialised = false;

    public static void Initialise()
    {
        if (s_initialised)
        {
            return;
        }

        s_initialised = true;

        LoadOpenAL();
    }

    private static void LoadOpenAL()
    {
        if (OperatingSystem.IsWindows())
        {
            LoadOpenALWindows();
            return;
        }

        if (OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("OpenAL loading on Linux is not yet implemented.");
        }

        if (OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("OpenAL loading on macOS is not yet implemented.");
        }

        throw new PlatformNotSupportedException(
            $"OpenAL loading is not supported on this platform ({RuntimeInformation.OSDescription}).");
    }

    private static void LoadOpenALWindows()
    {
        string baseDir = AppContext.BaseDirectory;

        string[] candidates =
        [
            Path.Combine(baseDir, "OpenAL32.dll"),
            Path.Combine(baseDir, "runtimes", "win-x64", "native", "OpenAL32.dll"),
        ];

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out _))
            {
                return;
            }
        }
    }
}

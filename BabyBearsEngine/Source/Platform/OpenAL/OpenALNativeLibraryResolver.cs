using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Module initialiser that hooks <see cref="AssemblyLoadContext.Default"/>'s
/// <c>ResolvingUnmanagedDll</c> event so that requests for <c>openal32</c> are satisfied from the
/// copy we ship under <c>runtimes/win-x64/native/</c> when the OS's default search fails to find
/// it system-wide.
///
/// Why an AssemblyLoadContext event rather than <see cref="NativeLibrary.SetDllImportResolver"/>:
/// OpenTK's <c>ALLoader</c> already registers a per-assembly resolver on its own assembly, and the
/// runtime only allows one such resolver per assembly. The ALC-context event accepts multiple
/// subscribers and fires only when the default probe (system dirs, PATH, app dir) has failed —
/// exactly the case we need to cover for test hosts where the app dir is the .NET SDK's testhost
/// rather than the test assembly's bin folder.
///
/// For normal game executables on Windows the DLL is copied next to the EXE by the standard build
/// output, so the OS finds it before this hook fires. The hook just covers the test-host case and
/// the (unlikely) scenario of a deployment that placed the file only in <c>runtimes/...</c>.
/// </summary>
internal static class OpenALNativeLibraryResolver
{
    private const string OpenAlLibraryName = "openal32";

#pragma warning disable CA2255 // ModuleInitializer in library code — intentional, needed to install
                              // the OpenAL native lookup before any AL/ALC P/Invoke happens.
    [ModuleInitializer]
    public static void Initialise()
    {
        AssemblyLoadContext.Default.ResolvingUnmanagedDll += Resolve;
    }
#pragma warning restore CA2255

    private static IntPtr Resolve(Assembly assembly, string libraryName)
    {
        if (!libraryName.Equals(OpenAlLibraryName, StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        string baseDir = AppContext.BaseDirectory;

        string[] candidates =
        [
            Path.Combine(baseDir, "OpenAL32.dll"),
            Path.Combine(baseDir, "openal32.dll"),
            Path.Combine(baseDir, "runtimes", "win-x64", "native", "OpenAL32.dll"),
            Path.Combine(baseDir, "runtimes", "win-x64", "native", "openal32.dll"),
        ];

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out IntPtr handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }
}

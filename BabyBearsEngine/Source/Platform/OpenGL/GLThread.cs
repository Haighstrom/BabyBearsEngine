using System.Diagnostics;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Tracks which threads are permitted to make OpenGL calls. The engine main thread registers
/// itself in <c>OpenTKGameEngine.OnLoad</c>; background loading threads register via
/// <see cref="LoadingScreenWorld"/>'s shared-context plumbing. GL-resource constructors call
/// <see cref="Ensure"/> as a debug-only safety net — the call compiles out in Release.
/// </summary>
internal static class GLThread
{
    [ThreadStatic]
    private static bool t_isGLThread;

    /// <summary>Mark the current thread as a valid GL thread. Idempotent.</summary>
    public static void Register() => t_isGLThread = true;

    /// <summary>Unmark the current thread. Idempotent.</summary>
    public static void Unregister() => t_isGLThread = false;

    /// <summary>
    /// In Debug builds, throws <see cref="InvalidOperationException"/> if the current thread
    /// hasn't been registered as a GL thread. In Release builds, the method body is removed
    /// by the compiler ([Conditional("DEBUG")]).
    /// </summary>
    [Conditional("DEBUG")]
    public static void Ensure()
    {
        if (!t_isGLThread)
        {
            throw new InvalidOperationException(
                $"GL call made on non-GL thread (managed thread id {Environment.CurrentManagedThreadId}). " +
                "GL resources can only be constructed on the engine thread, or on a worker thread " +
                "that holds an active shared GL context (use LoadingScreenWorld's sync overload).");
        }
    }
}

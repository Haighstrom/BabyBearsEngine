using BabyBearsEngine.AudioSystem;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine;

/// <summary>
/// Clears all state that must not survive a <see cref="GameLauncher.Run"/> invocation: cached
/// shader-program singletons (holding GL handles from the now-defunct context), the static
/// <see cref="OpenGLHelper"/> bind cache (whose handle slots would otherwise collide with
/// freshly-generated handles in the next context), the disposable audio service, and the
/// process-wide <see cref="EngineConfiguration"/> service registry. Called from
/// <see cref="GameLauncher.Run"/>'s finally block so sequential runs start clean.
/// </summary>
internal static class EngineTeardown
{
    public static void ResetForNextRun(IAudio? audioService)
    {
        // Drop cached shader-program singletons so the next Run gets fresh GL handles in its
        // new context — otherwise the second Run uses dangling handles from the first. The
        // previous instance's GL resources leak, but the context they belonged to is being
        // torn down anyway.
        SolidColourShaderProgramMatrix.ResetForNextRun();

        // Clear the OpenGLHelper bind cache. Its s_lastBound* sentinels survive between
        // consecutive GameLauncher.Run invocations because they're static. A stale handle from
        // the previous GL context could match a freshly-generated handle in the next context
        // and cause Bind calls to be silently skipped.
        OpenGLHelper.ResetCache();

        audioService?.Dispose();

        EngineConfiguration.Reset();
    }
}

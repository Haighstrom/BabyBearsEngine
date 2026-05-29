namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Thrown from <c>OnLoad</c> when the driver-reported OpenGL version is below
/// <see cref="GpuCapabilities.EngineMinimumOpenGL"/>. Indicates the engine cannot run on
/// this GPU/driver combination at all.
/// </summary>
public sealed class EngineInitialisationException(string message) : Exception(message)
{
}

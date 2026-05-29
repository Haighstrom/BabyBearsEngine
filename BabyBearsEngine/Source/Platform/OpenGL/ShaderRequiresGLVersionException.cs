namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Thrown from <see cref="OpenGLHelper.CreateShader"/> when an individual shader's
/// <c>#version</c> directive exceeds the GLSL version the current GPU/driver supports.
/// Caught-and-disabled by feature-specific code paths means the rest of the engine can keep
/// running on a GPU that only supports a subset of shaders; uncaught it propagates out as a
/// fatal error.
/// </summary>
public sealed class ShaderRequiresGLVersionException(string shaderIdentifier, int requiredGlslVersion, int availableGlslVersion)
    : Exception(
        $"Shader '{shaderIdentifier}' requires GLSL {requiredGlslVersion} but the current " +
        $"GPU/driver only supports GLSL {availableGlslVersion}. Update your GPU driver or " +
        $"run on more capable hardware.")
{
    public string ShaderIdentifier { get; } = shaderIdentifier;
    public int RequiredGlslVersion { get; } = requiredGlslVersion;
    public int AvailableGlslVersion { get; } = availableGlslVersion;
}

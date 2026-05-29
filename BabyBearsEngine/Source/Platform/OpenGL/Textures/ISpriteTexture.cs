using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// An <see cref="ISpriteSheet"/> backed by a GPU texture — the concrete-on-GPU variant of the
/// pure sprite-sheet layout. Implementations expose both the grid (via <see cref="ISpriteSheet"/>)
/// and the raw GL handle (via <see cref="ITexture"/>).
/// </summary>
public interface ISpriteTexture : ISpriteSheet, ITexture
{
}

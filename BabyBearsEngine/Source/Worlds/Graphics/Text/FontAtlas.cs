using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// A complete, ready-to-render font atlas: per-character metrics, the GL texture
/// containing the glyph data, and the matrix-aware shader program paired with that
/// texture's format. Produced by an <see cref="IFontAtlasGenerator"/>; consumed by
/// <see cref="TextGraphic"/>.
/// </summary>
internal record FontAtlas(FontAtlasMetrics Metrics, ITexture Texture, IMatrixShaderProgram Shader);

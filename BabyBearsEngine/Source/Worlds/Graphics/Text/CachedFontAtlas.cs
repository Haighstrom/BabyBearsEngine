using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;

namespace BabyBearsEngine.Worlds.Graphics.Text;

internal record CachedFontAtlas(FontAtlasMetrics Metrics, ITexture Texture, IMatrixShaderProgram Shader);

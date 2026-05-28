namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Produces a complete <see cref="FontAtlas"/> for a <see cref="FontDefinition"/>:
/// glyph metrics, the GL texture containing the rendered glyphs, and a shader
/// program matched to that texture's format.
/// <para>
/// Implementations decide internally how to load the font, what pixel format to
/// produce (alpha-coverage bitmap, single-channel SDF, multi-channel SDF, etc.),
/// and which shader to pair with it.
/// </para>
/// </summary>
internal interface IFontAtlasGenerator
{
    FontAtlas Generate(FontDefinition fontDef);
}

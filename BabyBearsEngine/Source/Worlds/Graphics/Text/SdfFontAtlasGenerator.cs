using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Mathematics;
using StbTrueTypeSharp;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Produces a font atlas of single-channel signed distance fields (SDF) rasterised from a
/// TrueType file with stb_truetype. Each glyph cell stores distance-to-outline rather than
/// coverage, so the paired <see cref="SdfTextShaderProgram"/> can reconstruct a crisp,
/// antialiased edge at any scale. Cross-platform (pure-managed stb_truetype); unlike the GDI+
/// generator it needs the font as a shipped <c>.ttf</c> file (see <see cref="TrueTypeFontFileResolver"/>).
/// </summary>
internal sealed class SdfFontAtlasGenerator : IFontAtlasGenerator
{
    // Number of glyph columns in the atlas spritesheet. Matches the GDI+ generator's layout
    // choice so both backends produce similarly-shaped atlases.
    private const int CharactersPerRow = 13;

    // Floor for the pixel height the glyphs are rasterised at. The field is authored at roughly
    // the requested display size (see RasteriseAtlas) so on-screen sampling stays close to 1:1 —
    // a distance field minified far below the size it was authored at fragments thin strokes,
    // because their peak interior distance falls beneath the shader's antialiasing band. This
    // floor keeps the field from degenerating for very small font sizes.
    private const float MinSourcePixelHeight = 8f;

    // Floor for the SDF spread, in source texels, on each side of the glyph outline. The distance
    // field ramps from "inside" to "outside" across this many texels, giving the shader room to
    // antialias. Scales with the source resolution (see RasteriseAtlas) so the spread stays a
    // sensible fraction of the glyph regardless of size.
    private const int MinPadding = 2;

    // The SDF byte value (0..255) that represents the glyph outline itself — 128 maps to 0.5,
    // the isovalue the shader tests against. Values above are inside the glyph, below are outside.
    private const byte OnEdgeValue = 128;

    private readonly record struct GlyphBitmap(byte[] Data, int Width, int Height, int XOffset, int YOffset);

    public FontAtlas Generate(FontDefinition fontDef)
    {
        (byte[] pixels, int width, int height, FontAtlasMetrics metrics) = RasteriseAtlas(fontDef);
        ITexture texture = new DefaultTextureFactory().GenR8Texture(pixels, width, height);
        IMatrixShaderProgram shader = new SdfTextShaderProgram();
        return new FontAtlas(metrics, texture, shader);
    }

    /// <summary>
    /// Rasterises the SDF atlas and computes per-glyph metrics on the CPU, without touching GL.
    /// Exposed as <c>internal</c> so benchmarks/tests that don't have a GL context can measure
    /// the pure rasterisation step. Returns the single-channel (R8) pixel buffer, its dimensions,
    /// and the backend-agnostic metrics consumed by <see cref="TextGraphic"/>.
    /// </summary>
    internal (byte[] Pixels, int Width, int Height, FontAtlasMetrics Metrics) RasteriseAtlas(FontDefinition fontDef)
    {
        string fontPath = TrueTypeFontFileResolver.Resolve(fontDef.FontName);
        byte[] fontData = File.ReadAllBytes(fontPath);
        StbTrueType.stbtt_fontinfo font = StbTrueType.CreateFont(fontData, 0)
            ?? throw new InvalidOperationException($"Failed to load font '{fontDef.FontName}' from '{fontPath}'.");

        string charsToLoad = fontDef.CharactersToLoad;
        int charCount = charsToLoad.Length;

        // Author the field at (about) the requested display size so on-screen sampling stays
        // close to 1:1. The SDF win — crisp magnification and smooth scaling — is preserved
        // around this size, while heavy minification (which fragments thin strokes) is avoided.
        float sourcePixelHeight = MathF.Max(fontDef.FontSize, MinSourcePixelHeight);
        int padding = Math.Max(MinPadding, (int)MathF.Round(sourcePixelHeight / 8f));
        float pixelDistScale = OnEdgeValue / (float)padding;

        float scale = StbTrueType.stbtt_ScaleForPixelHeight(font, sourcePixelHeight);

        var glyphs = new GlyphBitmap?[charCount];
        var advances = new int[charCount];
        int baselineRow;
        int emBoxHeight;

        // The atlas cell must be big enough to hold the widest/tallest glyph bitmap *including* its
        // SDF glow margins, so the distance ramp is never clipped at a cell boundary.
        int cellWidth = 1;
        int cellHeight = 1;

        unsafe
        {
            int ascent, descent, lineGap;
            StbTrueType.stbtt_GetFontVMetrics(font, &ascent, &descent, &lineGap);

            // Baseline sits 'ascent' down from the top of the em box; the em box spans the full em
            // (ascent above the baseline, descent below) and drives line height and glyph placement.
            baselineRow = (int)MathF.Round(ascent * scale);
            emBoxHeight = Math.Max(1, (int)MathF.Round((ascent - descent) * scale));

            for (int i = 0; i < charCount; i++)
            {
                char c = charsToLoad[i];

                int advanceWidth, leftSideBearing;
                StbTrueType.stbtt_GetCodepointHMetrics(font, c, &advanceWidth, &leftSideBearing);
                advances[i] = Math.Max(1, (int)MathF.Round(advanceWidth * scale));

                int glyphWidth, glyphHeight, xOffset, yOffset;
                byte* sdfPtr = StbTrueType.stbtt_GetCodepointSDF(
                    font, scale, c, padding, OnEdgeValue, pixelDistScale,
                    &glyphWidth, &glyphHeight, &xOffset, &yOffset);

                // Glyphs with no outline (e.g. space, or codepoints absent from the font) return
                // null. They draw nothing; only their advance contributes to layout.
                if (sdfPtr is null)
                {
                    glyphs[i] = null;
                    continue;
                }

                byte[] glyphData = new byte[glyphWidth * glyphHeight];
                Marshal.Copy((IntPtr)sdfPtr, glyphData, 0, glyphData.Length);
                StbTrueType.stbtt_FreeSDF(sdfPtr, null);

                glyphs[i] = new GlyphBitmap(glyphData, glyphWidth, glyphHeight, xOffset, yOffset);

                if (glyphWidth > cellWidth)
                {
                    cellWidth = glyphWidth;
                }

                if (glyphHeight > cellHeight)
                {
                    cellHeight = glyphHeight;
                }
            }
        }

        int spriteSheetWidth = CharactersPerRow * cellWidth;
        int rowCount = (int)Math.Ceiling((float)charCount / CharactersPerRow);
        int spriteSheetHeight = Math.Max(1, rowCount * cellHeight);
        byte[] pixels = new byte[spriteSheetWidth * spriteSheetHeight];

        float logicalScale = fontDef.FontSize / sourcePixelHeight;
        int highestCharLogical = Math.Max(1, (int)MathF.Round(emBoxHeight * logicalScale));
        int widestCharLogical = 1;

        Dictionary<char, Box2i> charPositions = [];
        Dictionary<char, Box2> charPositionsNormalised = [];
        Dictionary<char, int> charAdvances = [];
        Dictionary<char, Vector2i> charBearings = [];

        for (int i = 0; i < charCount; i++)
        {
            char c = charsToLoad[i];
            int cellX = i % CharactersPerRow * cellWidth;
            int cellY = i / CharactersPerRow * cellHeight;

            // Advance (logical pixels) drives layout spacing, decoupled from the render-quad size so
            // the glow margin never pushes glyphs apart.
            int advanceLogical = Math.Max(1, (int)MathF.Round(advances[i] * logicalScale));
            charAdvances[c] = advanceLogical;

            if (advanceLogical > widestCharLogical)
            {
                widestCharLogical = advanceLogical;
            }

            if (glyphs[i] is GlyphBitmap glyph)
            {
                BlitGlyph(pixels, spriteSheetWidth, cellX, cellY, glyph);

                // UVs map to the full glyph bitmap (glow included) at the source resolution.
                charPositionsNormalised[c] = new Box2(
                    cellX / (float)spriteSheetWidth,
                    cellY / (float)spriteSheetHeight,
                    (cellX + glyph.Width) / (float)spriteSheetWidth,
                    (cellY + glyph.Height) / (float)spriteSheetHeight);

                // Render-quad size (logical pixels): the box the shader draws into, glow and all.
                int logicalWidth = Math.Max(1, (int)MathF.Round(glyph.Width * logicalScale));
                int logicalHeight = Math.Max(1, (int)MathF.Round(glyph.Height * logicalScale));
                charPositions[c] = new Box2i(0, 0, logicalWidth, logicalHeight);

                // Offset of the render quad's top-left from the pen position and line top. The SDF
                // bitmap begins 'padding' texels left of, and above, the glyph outline (encoded in
                // stb's x/y offsets), so the glow lands outside the advance box instead of clipped.
                int bearingX = (int)MathF.Round(glyph.XOffset * logicalScale);
                int bearingY = (int)MathF.Round((baselineRow + glyph.YOffset) * logicalScale);
                charBearings[c] = new Vector2i(bearingX, bearingY);
            }
            else
            {
                // No outline: nothing to draw, only the advance applies. A zero-size render box
                // suppresses the quad while keeping a valid entry for every requested character.
                charPositionsNormalised[c] = new Box2(0f, 0f, 0f, 0f);
                charPositions[c] = new Box2i(0, 0, 0, 0);
                charBearings[c] = Vector2i.Zero;
            }
        }

        FontAtlasMetrics metrics = new(
            WidestChar: widestCharLogical,
            HighestChar: highestCharLogical,
            CharPositions: charPositions,
            CharPositionsNormalised: charPositionsNormalised,
            CharAdvances: charAdvances,
            CharBearings: charBearings);

        return (pixels, spriteSheetWidth, spriteSheetHeight, metrics);
    }

    /// <summary>
    /// Copies a glyph's full SDF bitmap — distance ramp and glow margins included — into the
    /// top-left of its atlas cell. The cell is sized to the largest glyph bitmap, so no clipping is
    /// needed and the distance field is preserved on every side. The surrounding empty cell space
    /// stays zero ("far outside"), which prevents any bleed into neighbouring cells under linear
    /// sampling. Per-glyph placement relative to the text baseline is applied later via bearings.
    /// </summary>
    private static void BlitGlyph(byte[] pixels, int atlasWidth, int cellX, int cellY, GlyphBitmap glyph)
    {
        for (int sy = 0; sy < glyph.Height; sy++)
        {
            int destRowStart = (cellY + sy) * atlasWidth + cellX;
            int srcRowStart = sy * glyph.Width;

            for (int sx = 0; sx < glyph.Width; sx++)
            {
                pixels[destRowStart + sx] = glyph.Data[srcRowStart + sx];
            }
        }
    }
}

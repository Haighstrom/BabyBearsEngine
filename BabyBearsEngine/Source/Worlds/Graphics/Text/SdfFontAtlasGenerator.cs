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
/// generator it needs the font as a shipped <c>.ttf</c> file (see <see cref="SdfFontFileResolver"/>).
/// </summary>
internal sealed class SdfFontAtlasGenerator : IFontAtlasGenerator
{
    // Number of glyph columns in the atlas spritesheet. Matches the GDI+ generator's layout
    // choice so both backends produce similarly-shaped atlases.
    private const int CharactersPerRow = 13;

    // Pixel height the glyphs are rasterised at to build the SDF texture. Decoupled from the
    // requested FontDefinition.FontSize: the texture is built once at this resolution for good
    // quality, while the exposed metrics are scaled down to the logical font size so layout
    // matches regardless of how big the source bitmap is.
    private const float SourcePixelHeight = 48f;

    // SDF spread, in source texels, on each side of the glyph outline. The distance field ramps
    // from "inside" to "outside" across this many texels, giving the shader room to antialias.
    private const int Padding = 6;

    // The SDF byte value (0..255) that represents the glyph outline itself — 128 maps to 0.5,
    // the isovalue the shader tests against. Values above are inside the glyph, below are outside.
    private const byte OnEdgeValue = 128;

    // How much the stored SDF value changes per texel of distance from the edge. Chosen so the
    // distance reaches 0 exactly Padding texels outside the outline (and saturates inside).
    private const float PixelDistScale = OnEdgeValue / (float)Padding;

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
        string fontPath = SdfFontFileResolver.Resolve(fontDef.FontName);
        byte[] fontData = File.ReadAllBytes(fontPath);
        StbTrueType.stbtt_fontinfo font = StbTrueType.CreateFont(fontData, 0)
            ?? throw new InvalidOperationException($"Failed to load font '{fontDef.FontName}' from '{fontPath}'.");

        string charsToLoad = fontDef.CharactersToLoad;
        int charCount = charsToLoad.Length;

        float scale = StbTrueType.stbtt_ScaleForPixelHeight(font, SourcePixelHeight);

        var glyphs = new GlyphBitmap?[charCount];
        var cellWidths = new int[charCount];
        int widestCell = 1;
        int baselineRow;
        int sourceCellHeight;

        unsafe
        {
            int ascent, descent, lineGap;
            StbTrueType.stbtt_GetFontVMetrics(font, &ascent, &descent, &lineGap);

            // Baseline sits 'ascent' down from the top of the cell; the cell spans the full em
            // (ascent above the baseline, descent below) so ascenders and descenders both fit.
            baselineRow = (int)MathF.Round(ascent * scale);
            sourceCellHeight = Math.Max(1, (int)MathF.Round((ascent - descent) * scale));

            for (int i = 0; i < charCount; i++)
            {
                char c = charsToLoad[i];

                int advanceWidth, leftSideBearing;
                StbTrueType.stbtt_GetCodepointHMetrics(font, c, &advanceWidth, &leftSideBearing);
                int cellWidth = Math.Max(1, (int)MathF.Round(advanceWidth * scale));
                cellWidths[i] = cellWidth;

                if (cellWidth > widestCell)
                {
                    widestCell = cellWidth;
                }

                int glyphWidth, glyphHeight, xOffset, yOffset;
                byte* sdfPtr = StbTrueType.stbtt_GetCodepointSDF(
                    font, scale, c, Padding, OnEdgeValue, PixelDistScale,
                    &glyphWidth, &glyphHeight, &xOffset, &yOffset);

                // Glyphs with no outline (e.g. space, or codepoints absent from the font) return
                // null. The cell still occupies its advance width so spacing stays correct.
                if (sdfPtr is null)
                {
                    glyphs[i] = null;
                    continue;
                }

                byte[] glyphData = new byte[glyphWidth * glyphHeight];
                Marshal.Copy((IntPtr)sdfPtr, glyphData, 0, glyphData.Length);
                StbTrueType.stbtt_FreeSDF(sdfPtr, null);

                glyphs[i] = new GlyphBitmap(glyphData, glyphWidth, glyphHeight, xOffset, yOffset);
            }
        }

        int spriteSheetWidth = CharactersPerRow * widestCell;
        int rowCount = (int)Math.Ceiling((float)charCount / CharactersPerRow);
        int spriteSheetHeight = Math.Max(1, rowCount * sourceCellHeight);
        byte[] pixels = new byte[spriteSheetWidth * spriteSheetHeight];

        float logicalScale = fontDef.FontSize / SourcePixelHeight;
        int highestCharLogical = Math.Max(1, (int)MathF.Round(sourceCellHeight * logicalScale));
        int widestCharLogical = 1;

        Dictionary<char, Box2i> charPositions = [];
        Dictionary<char, Box2> charPositionsNormalised = [];

        for (int i = 0; i < charCount; i++)
        {
            char c = charsToLoad[i];
            int cellX = i % CharactersPerRow * widestCell;
            int cellY = i / CharactersPerRow * sourceCellHeight;
            int cellWidth = cellWidths[i];

            if (glyphs[i] is GlyphBitmap glyph)
            {
                BlitGlyph(pixels, spriteSheetWidth, cellX, cellY, cellWidth, sourceCellHeight, baselineRow, glyph);
            }

            // UVs map to the source-resolution atlas; independent of the logical font size.
            charPositionsNormalised[c] = new Box2(
                cellX / (float)spriteSheetWidth,
                cellY / (float)spriteSheetHeight,
                (cellX + cellWidth) / (float)spriteSheetWidth,
                (cellY + sourceCellHeight) / (float)spriteSheetHeight);

            // Logical glyph box drives layout: only the size is consumed by TextGraphic, scaled
            // down from the source resolution to the requested font size.
            int logicalWidth = Math.Max(1, (int)MathF.Round(cellWidth * logicalScale));

            if (logicalWidth > widestCharLogical)
            {
                widestCharLogical = logicalWidth;
            }

            int logicalX = (int)MathF.Round(cellX * logicalScale);
            int logicalY = (int)MathF.Round(cellY * logicalScale);
            charPositions[c] = new Box2i(logicalX, logicalY, logicalX + logicalWidth, logicalY + highestCharLogical);
        }

        FontAtlasMetrics metrics = new(
            WidestChar: widestCharLogical,
            HighestChar: highestCharLogical,
            CharPositions: charPositions,
            CharPositionsNormalised: charPositionsNormalised);

        return (pixels, spriteSheetWidth, spriteSheetHeight, metrics);
    }

    /// <summary>
    /// Copies a glyph's SDF bitmap into its atlas cell at the correct baseline-relative position,
    /// clipping to the cell so a glyph's distance ramp never bleeds into a neighbouring cell.
    /// </summary>
    private static void BlitGlyph(byte[] pixels, int atlasWidth, int cellX, int cellY, int cellWidth, int cellHeight, int baselineRow, GlyphBitmap glyph)
    {
        for (int sy = 0; sy < glyph.Height; sy++)
        {
            int destY = cellY + baselineRow + glyph.YOffset + sy;

            if (destY < cellY || destY >= cellY + cellHeight)
            {
                continue;
            }

            for (int sx = 0; sx < glyph.Width; sx++)
            {
                int destX = cellX + glyph.XOffset + sx;

                if (destX < cellX || destX >= cellX + cellWidth)
                {
                    continue;
                }

                pixels[destY * atlasWidth + destX] = glyph.Data[sy * glyph.Width + sx];
            }
        }
    }
}

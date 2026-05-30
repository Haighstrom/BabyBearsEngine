using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BabyBearsEngine.OpenGL;
using FreeTypeSharp;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Produces a font atlas of single-channel (R8) grayscale coverage rasterised from a TrueType file
/// with FreeType, using "light" autohinting (vertical-only grid-fitting) so small fixed-size text
/// stays crisp. The cross-platform counterpart to <see cref="GdiFontAtlasGenerator"/>: glyphs are
/// stored as alpha coverage authored at the requested pixel size and paired with
/// <see cref="CoverageTextShaderProgram"/>, which simply tints that coverage. Unlike
/// <see cref="SdfFontAtlasGenerator"/> there is no distance field, so quality is fixed to the source
/// size — best for UI text, not scaled/world text. Needs the font shipped as a <c>.ttf</c> file
/// (see <see cref="TrueTypeFontFileResolver"/>).
/// </summary>
internal sealed class FreeTypeFontAtlasGenerator : IFontAtlasGenerator
{
    // Number of glyph columns in the atlas spritesheet. Matches the other generators' layout so all
    // backends produce similarly-shaped atlases.
    private const int CharactersPerRow = 13;

    private readonly record struct GlyphBitmap(byte[] Data, int Width, int Height, int Left, int Top, int Advance);

    public FontAtlas Generate(FontDefinition fontDef)
    {
        (byte[] pixels, int width, int height, FontAtlasMetrics metrics) = RasteriseAtlas(fontDef);

        // Nearest filtering: the coverage is hinted and authored at the target pixel size, and
        // TextGraphic pixel-snaps each glyph quad, so 1:1 point sampling keeps the hinted edges
        // crisp. (The SDF backend, by contrast, needs linear filtering to reconstruct distances.)
        ITexture texture = new DefaultTextureFactory().GenR8Texture(pixels, width, height, linearFilter: false);
        IMatrixShaderProgram shader = Shaders.CoverageText;
        return new FontAtlas(metrics, texture, shader);
    }

    /// <summary>
    /// Rasterises the coverage atlas and computes per-glyph metrics on the CPU, without touching GL.
    /// Exposed as <c>internal</c> so benchmarks/tests that don't have a GL context can measure the
    /// pure rasterisation step. Returns the single-channel (R8) pixel buffer, its dimensions, and the
    /// backend-agnostic metrics consumed by <see cref="TextGraphic"/>.
    /// </summary>
    internal (byte[] Pixels, int Width, int Height, FontAtlasMetrics Metrics) RasteriseAtlas(FontDefinition fontDef)
    {
        string fontPath = TrueTypeFontFileResolver.Resolve(fontDef.FontName);
        byte[] fontData = File.ReadAllBytes(fontPath);

        string charsToLoad = fontDef.CharactersToLoad;
        int charCount = charsToLoad.Length;

        // Author the coverage at the requested display size — that is the whole point of a hinted
        // coverage backend: crisp at its native pixel size. Floor at 1px so the size is always valid.
        int pixelHeight = Math.Max(1, (int)MathF.Round(fontDef.FontSize));

        var glyphs = new GlyphBitmap[charCount];
        int lineHeight;

        // The atlas cell must be big enough to hold the widest/tallest glyph bitmap.
        int cellWidth = 1;
        int cellHeight = 1;

        // FreeType references (does not copy) the font buffer via FT_New_Memory_Face, so the bytes
        // must stay pinned for the lifetime of the face.
        var fontHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
        try
        {
            using FreeTypeLibrary library = new();
            FreeTypeFaceFacade face = new(library, fontHandle.AddrOfPinnedObject(), fontData.Length, 0);

            try
            {
                (glyphs, lineHeight, cellWidth, cellHeight) = LoadGlyphs(face, charsToLoad, pixelHeight, fontDef.FontName);
            }
            finally
            {
                unsafe
                {
                    FT.FT_Done_Face(face.FaceRec);
                }
            }
        }
        finally
        {
            fontHandle.Free();
        }

        int spriteSheetWidth = CharactersPerRow * cellWidth;
        int rowCount = (int)Math.Ceiling((float)charCount / CharactersPerRow);
        int spriteSheetHeight = Math.Max(1, rowCount * cellHeight);
        byte[] pixels = new byte[spriteSheetWidth * spriteSheetHeight];

        int highestChar = Math.Max(1, lineHeight);
        int widestChar = 1;

        Dictionary<char, Box2i> charPositions = [];
        Dictionary<char, Box2> charPositionsNormalised = [];
        Dictionary<char, int> charAdvances = [];
        Dictionary<char, Vector2i> charBearings = [];

        for (int i = 0; i < charCount; i++)
        {
            char c = charsToLoad[i];
            int cellX = i % CharactersPerRow * cellWidth;
            int cellY = i / CharactersPerRow * cellHeight;
            GlyphBitmap glyph = glyphs[i];

            // Advance drives layout spacing. FreeType already reports it in whole pixels.
            charAdvances[c] = glyph.Advance;

            if (glyph.Advance > widestChar)
            {
                widestChar = glyph.Advance;
            }

            if (glyph.Width > 0 && glyph.Height > 0)
            {
                BlitGlyph(pixels, spriteSheetWidth, cellX, cellY, glyph);

                // UVs map to the tight coverage bitmap within its atlas cell.
                charPositionsNormalised[c] = new Box2(
                    cellX / (float)spriteSheetWidth,
                    cellY / (float)spriteSheetHeight,
                    (cellX + glyph.Width) / (float)spriteSheetWidth,
                    (cellY + glyph.Height) / (float)spriteSheetHeight);

                // Render quad maps 1:1 to the coverage bitmap (no glow margin, unlike SDF).
                charPositions[c] = new Box2i(0, 0, glyph.Width, glyph.Height);

                // Offset of the render quad's top-left from the pen position and line top.
                charBearings[c] = new Vector2i(glyph.Left, glyph.Top);
            }
            else
            {
                // No coverage (e.g. space): nothing to draw, only the advance applies.
                charPositionsNormalised[c] = new Box2(0f, 0f, 0f, 0f);
                charPositions[c] = new Box2i(0, 0, 0, 0);
                charBearings[c] = Vector2i.Zero;
            }
        }

        FontAtlasMetrics metrics = new(
            WidestChar: widestChar,
            HighestChar: highestChar,
            CharPositions: charPositions,
            CharPositionsNormalised: charPositionsNormalised,
            CharAdvances: charAdvances,
            CharBearings: charBearings);

        return (pixels, spriteSheetWidth, spriteSheetHeight, metrics);
    }

    /// <summary>
    /// Loads, light-hints and renders every requested glyph into a tight managed coverage buffer
    /// (FreeType reuses the glyph slot's bitmap on each load, so the bytes must be copied out), and
    /// returns the per-glyph data alongside the line height and the cell size needed to hold the
    /// largest glyph.
    /// </summary>
    private static unsafe (GlyphBitmap[] Glyphs, int LineHeight, int CellWidth, int CellHeight) LoadGlyphs(
        FreeTypeFaceFacade face, string charsToLoad, int pixelHeight, string fontName)
    {
        FT_Error sizeError = FT.FT_Set_Pixel_Sizes(face.FaceRec, 0, (uint)pixelHeight);
        if (sizeError != FT_Error.FT_Err_Ok)
        {
            throw new InvalidOperationException(
                $"FreeType failed to set pixel size {pixelHeight} for font '{fontName}' ({sizeError}).");
        }

        // Baseline sits 'ascent' px below the line top; LineSpacing is the font's recommended line
        // height and drives glyph vertical placement and the metrics' HighestChar.
        int ascent = face.Ascender;
        int lineHeight = face.LineSpacing;

        // Light hinting (vertical-only grid-fit) is the grayscale sweet spot for crisp small text.
        var loadFlags = (FT_LOAD)((int)FT_LOAD.FT_LOAD_RENDER | FT.FT_LOAD_TARGET_LIGHT);

        int charCount = charsToLoad.Length;
        var glyphs = new GlyphBitmap[charCount];
        int cellWidth = 1;
        int cellHeight = 1;

        for (int i = 0; i < charCount; i++)
        {
            char c = charsToLoad[i];

            FT_Error loadError = FT.FT_Load_Char(face.FaceRec, (UIntPtr)(uint)c, loadFlags);
            if (loadError != FT_Error.FT_Err_Ok)
            {
                // Skip glyphs FreeType cannot load, but keep a valid (blank, 1px-advance) entry so
                // every requested character still has metrics.
                glyphs[i] = new GlyphBitmap([], 0, 0, 0, 0, 1);
                continue;
            }

            int advance = Math.Max(1, face.GlyphMetricHorizontalAdvance);
            FT_Bitmap_ bitmap = face.GlyphBitmap;
            int glyphWidth = (int)bitmap.width;
            int glyphHeight = (int)bitmap.rows;

            // Glyphs with no coverage (e.g. space) draw nothing; only their advance contributes.
            if (glyphWidth == 0 || glyphHeight == 0 || bitmap.buffer == null)
            {
                glyphs[i] = new GlyphBitmap([], 0, 0, 0, 0, advance);
                continue;
            }

            byte[] glyphData = new byte[glyphWidth * glyphHeight];
            int pitch = bitmap.pitch;
            for (int sourceRow = 0; sourceRow < glyphHeight; sourceRow++)
            {
                // pitch may exceed width (row padding); it is positive for the top-down rows FreeType
                // produces for grayscale coverage.
                byte* sourceRowStart = bitmap.buffer + (sourceRow * pitch);
                Marshal.Copy((IntPtr)sourceRowStart, glyphData, sourceRow * glyphWidth, glyphWidth);
            }

            // bitmap_left/top are relative to the pen and baseline (bitmap_left may be negative).
            // Convert bitmap_top to a line-top offset: the baseline is 'ascent' px below the line top.
            int left = face.GlyphBitmapLeft;
            int top = ascent - face.GlyphBitmapTop;
            glyphs[i] = new GlyphBitmap(glyphData, glyphWidth, glyphHeight, left, top, advance);

            if (glyphWidth > cellWidth)
            {
                cellWidth = glyphWidth;
            }

            if (glyphHeight > cellHeight)
            {
                cellHeight = glyphHeight;
            }
        }

        return (glyphs, lineHeight, cellWidth, cellHeight);
    }

    /// <summary>
    /// Copies a glyph's tight coverage bitmap into the top-left of its atlas cell. The cell is sized
    /// to the largest glyph, so no clipping is needed; the surrounding empty cell space stays zero
    /// ("no coverage"), which prevents bleed into neighbouring cells. Per-glyph placement relative to
    /// the text baseline is applied later via bearings.
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

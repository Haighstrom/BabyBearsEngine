using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Produces a font atlas by rasterising glyphs with GDI+ at the font's chosen pixel
/// size. Encodes coverage (alpha) into the texture and pairs it with the standard
/// matrix shader. Windows-only — the GDI+ APIs it uses are not available on other
/// platforms, so <see cref="Generate"/> and <see cref="RasteriseAtlas"/> throw
/// <see cref="PlatformNotSupportedException"/> when invoked off Windows. The
/// engine-wide default text backend is <see cref="TextRenderer.FreeType"/> (also
/// cross-platform) for that reason; pick Gdi explicitly via
/// <see cref="TextSettings.Renderer"/> or <see cref="FontDefinition.Renderer"/>
/// only on Windows targets.
/// </summary>
internal sealed class GdiFontAtlasGenerator : IFontAtlasGenerator
{
    // Layout choice — number of glyph columns in the atlas spritesheet. Internal
    // to the GDI+ path; other generators may lay glyphs out differently.
    private const int CharactersPerRow = 13;

    private readonly CharacterBitmapGenerator _charBitmapGenerator = new();

    public FontAtlas Generate(FontDefinition fontDef)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                $"{nameof(GdiFontAtlasGenerator)} requires Windows; use {nameof(TextRenderer)}.{nameof(TextRenderer.Sdf)} or {nameof(TextRenderer)}.{nameof(TextRenderer.FreeType)} on other platforms.");
        }

        (Bitmap bitmap, FontAtlasMetrics metrics) = RasteriseAtlas(fontDef);

        // Point (nearest) sampling, not bilinear: GDI+ is the fixed-size, grid-fitted backend, so its
        // glyphs are authored to be drawn 1:1. Bilinear filtering would resample the coverage and
        // soften the hinted edges even at scale 1; nearest keeps them crisp. (The SDF backend keeps
        // linear filtering — it relies on inter-texel blending to reconstruct its distance field.)
        ITexture texture = UploadBitmapAsTexture(bitmap, linearFilter: false);
        IMatrixShaderProgram shader = Shaders.Standard;
        return new FontAtlas(metrics, texture, shader);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "GDI+ is Windows-only by design.")]
    private static ITexture UploadBitmapAsTexture(Bitmap bmp, bool linearFilter)
    {
        GLThread.Ensure();
        bmp = BitmapTools.PremultiplyAlpha(bmp);

        Texture t = new(
            handle: GL.GenTexture(),
            width: bmp.Width,
            height: bmp.Height);

        OpenGLHelper.BindTexture(t.Handle);

        var minFilter = linearFilter ? TextureMinFilter.Linear : TextureMinFilter.Nearest;
        var magFilter = linearFilter ? TextureMagFilter.Linear : TextureMagFilter.Nearest;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        BitmapData bmpd = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpd.Width, bmpd.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpd.Scan0);

        bmp.UnlockBits(bmpd);

        return t;
    }

    /// <summary>
    /// Builds the GDI+ bitmap atlas and computes per-glyph metrics, without touching
    /// GL. Exposed as <c>internal</c> so benchmarks/tests that don't have a GL context
    /// can measure the pure-CPU rasterisation step.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "GDI+ is Windows-only by design.")]
    internal (Bitmap Bitmap, FontAtlasMetrics Metrics) RasteriseAtlas(FontDefinition fontDef)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                $"{nameof(GdiFontAtlasGenerator)} requires Windows; use {nameof(TextRenderer)}.{nameof(TextRenderer.Sdf)} or {nameof(TextRenderer)}.{nameof(TextRenderer.FreeType)} on other platforms.");
        }

        Font font = new FontLoader().LoadFont(fontDef);
        string charsToLoad = fontDef.CharactersToLoad;

        int widestChar = 0;
        int highestChar = 0;

        var characterBMPs = new List<Bitmap>();
        //find the biggest character and create the spritesheet based on this size
        foreach (char c in charsToLoad)
        {
            var b = _charBitmapGenerator.GenerateCharacterBitmap(c, font);

            if (b.Width > widestChar)
            {
                widestChar = b.Width;
            }

            if (b.Height > highestChar)
            {
                highestChar = b.Height;
            }

            characterBMPs.Add(b);
        }

        int spriteSheetWidth = CharactersPerRow * widestChar;
        int spriteSheetHeight = (int)Math.Ceiling((float)characterBMPs.Count / CharactersPerRow) * highestChar;

        Bitmap characterSS = new(spriteSheetWidth, spriteSheetHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var g = System.Drawing.Graphics.FromImage(characterSS);
        g.CompositingQuality = CompositingQuality.HighQuality;
        // Grid-fit (hint) the glyphs: AntiAliasGridFit snaps stems to whole pixels, so small text
        // stays crisp instead of smearing each stem across two columns at partial coverage — the
        // blur the plain AntiAlias hint produces. Grayscale (not ClearType) AA keeps the atlas a
        // single coverage channel, which the alpha-tinting text shader expects.
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        int j = 0, posX, posY;

        Dictionary<char, Box2i> charPositions = [];
        Dictionary<char, Box2> charPositionsNormalised = [];

        foreach (char c in charsToLoad)
        {
            posX = j % CharactersPerRow * widestChar;
            posY = j / CharactersPerRow * highestChar;

            var charRect = new Box2i(posX, posY, posX + characterBMPs[j].Width, posY + highestChar);
            charPositions.Add(c, charRect);
            //charRect = charRect.ScaleAround(spriteSheetWidth, spriteSheetHeight, 0, 0);
            var charRectNormalised = new Box2(charRect.Min.X / (float)spriteSheetWidth, charRect.Min.Y / (float)spriteSheetHeight, charRect.Min.X / (float)spriteSheetWidth + charRect.Size.X / (float)spriteSheetWidth, charRect.Min.Y / (float)spriteSheetHeight + charRect.Size.Y / (float)spriteSheetHeight);
            charPositionsNormalised.Add(c, charRectNormalised);

            using (var ia = new ImageAttributes())
            {
                ia.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(characterBMPs[j], posX, posY);
            }

            j++;
        }

        foreach (var b in characterBMPs)
        {
            b.Dispose();
        }

        FontAtlasMetrics metrics = new(
            WidestChar: widestChar,
            HighestChar: highestChar,
            CharPositions: charPositions,
            CharPositionsNormalised: charPositionsNormalised);

        return (characterSS, metrics);
    }
}

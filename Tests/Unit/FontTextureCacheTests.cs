using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FontTextureCacheTests
{
    private sealed class StubTexture : ITexture
    {
        public int DisposeCallCount { get; private set; } = 0;

        public int Handle => 0;
        public int Width => 0;
        public int Height => 0;

        public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0)
        {
        }

        public void Dispose()
        {
            DisposeCallCount++;
        }
    }

    private sealed class StubShader : IMatrixShaderProgram
    {
        public int DisposeCallCount { get; private set; } = 0;

        public int Handle => 0;

        public void Bind()
        {
        }

        public void Dispose()
        {
            DisposeCallCount++;
        }

        public void SetModelViewMatrix(ref Matrix3 matrix)
        {
        }

        public void SetProjectionMatrix(ref Matrix3 matrix)
        {
        }
    }

    private sealed class CountingGenerator : IFontAtlasGenerator
    {
        public int GenerateCalls { get; private set; } = 0;

        public FontAtlas Generate(FontDefinition fontDef)
        {
            GenerateCalls++;
            FontAtlasMetrics metrics = new(0, 0, [], []);
            return new FontAtlas(metrics, new StubTexture(), new StubShader());
        }
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    [TestMethod]
    public void GetOrCreate_FirstCallForFont_InvokesGenerator()
    {
        CountingGenerator generator = new();
        EngineConfiguration.AtlasGenerator = generator;
        FontDefinition fontDef = new("Arial", 12);

        FontTextureCache.GetOrCreate(fontDef);

        Assert.AreEqual(1, generator.GenerateCalls);
    }

    [TestMethod]
    public void GetOrCreate_SecondCallForSameFont_ReturnsCachedAtlas()
    {
        CountingGenerator generator = new();
        EngineConfiguration.AtlasGenerator = generator;
        FontDefinition fontDef = new("Arial", 12);

        FontAtlas first = FontTextureCache.GetOrCreate(fontDef);
        FontAtlas second = FontTextureCache.GetOrCreate(fontDef);

        Assert.AreEqual(1, generator.GenerateCalls);
        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void GetOrCreate_DifferentFontDefinitions_GenerateEachOnce()
    {
        CountingGenerator generator = new();
        EngineConfiguration.AtlasGenerator = generator;
        FontDefinition fontA = new("Arial", 12);
        FontDefinition fontB = new("Times New Roman", 14);

        FontTextureCache.GetOrCreate(fontA);
        FontTextureCache.GetOrCreate(fontB);
        FontTextureCache.GetOrCreate(fontA);
        FontTextureCache.GetOrCreate(fontB);

        Assert.AreEqual(2, generator.GenerateCalls);
    }

    [TestMethod]
    public void AtlasGenerator_Setter_ClearsCache()
    {
        CountingGenerator firstGenerator = new();
        EngineConfiguration.AtlasGenerator = firstGenerator;
        FontDefinition fontDef = new("Arial", 12);
        FontTextureCache.GetOrCreate(fontDef);

        CountingGenerator secondGenerator = new();
        EngineConfiguration.AtlasGenerator = secondGenerator;
        FontTextureCache.GetOrCreate(fontDef);

        Assert.AreEqual(1, firstGenerator.GenerateCalls);
        Assert.AreEqual(1, secondGenerator.GenerateCalls);
    }

    [TestMethod]
    public void AtlasGenerator_SetterWithNull_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => EngineConfiguration.AtlasGenerator = null!);
    }

    [TestMethod]
    public void AtlasGenerator_GetterBeforeInitialise_Throws()
    {
        EngineConfiguration.Reset();

        Assert.ThrowsExactly<InvalidOperationException>(() => _ = EngineConfiguration.AtlasGenerator);
    }

    [TestMethod]
    public void Reset_ClearsCache()
    {
        CountingGenerator firstGenerator = new();
        EngineConfiguration.AtlasGenerator = firstGenerator;
        FontDefinition fontDef = new("Arial", 12);
        FontTextureCache.GetOrCreate(fontDef);

        EngineConfiguration.Reset();

        CountingGenerator secondGenerator = new();
        EngineConfiguration.AtlasGenerator = secondGenerator;
        FontTextureCache.GetOrCreate(fontDef);

        Assert.AreEqual(1, secondGenerator.GenerateCalls);
    }

    [TestMethod]
    public void GetOrCreate_FontPinnedToRenderer_UsesGeneratorRegisteredForThatBackend()
    {
        // The default backend (Gdi after Reset) gets one generator; Sdf gets another.
        CountingGenerator defaultGenerator = new();
        CountingGenerator sdfGenerator = new();
        EngineConfiguration.AtlasGenerator = defaultGenerator;
        EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Sdf, sdfGenerator);
        FontDefinition pinnedFont = new("Arial", 12, Renderer: TextRenderer.Sdf);

        FontTextureCache.GetOrCreate(pinnedFont);

        // The font pinned itself to Sdf, so only that backend's generator runs.
        Assert.AreEqual(0, defaultGenerator.GenerateCalls);
        Assert.AreEqual(1, sdfGenerator.GenerateCalls);
    }

    [TestMethod]
    public void GetOrCreate_SameFontDifferentRenderer_CachedSeparately()
    {
        CountingGenerator gdiGenerator = new();
        CountingGenerator sdfGenerator = new();
        EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Gdi, gdiGenerator);
        EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Sdf, sdfGenerator);
        FontDefinition gdiFont = new("Arial", 12, Renderer: TextRenderer.Gdi);
        FontDefinition sdfFont = new("Arial", 12, Renderer: TextRenderer.Sdf);

        FontTextureCache.GetOrCreate(gdiFont);
        FontTextureCache.GetOrCreate(sdfFont);
        FontTextureCache.GetOrCreate(gdiFont);
        FontTextureCache.GetOrCreate(sdfFont);

        // Definitions differing only by backend are distinct cache keys, so each generates once.
        Assert.AreEqual(1, gdiGenerator.GenerateCalls);
        Assert.AreEqual(1, sdfGenerator.GenerateCalls);
    }

    [TestMethod]
    public void GetOrCreate_UnpinnedFont_FollowsDefaultTextRenderer()
    {
        CountingGenerator gdiGenerator = new();
        CountingGenerator sdfGenerator = new();
        EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Gdi, gdiGenerator);
        EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Sdf, sdfGenerator);
        EngineConfiguration.DefaultTextRenderer = TextRenderer.Sdf;
        FontDefinition unpinnedFont = new("Arial", 12);

        FontTextureCache.GetOrCreate(unpinnedFont);

        // No backend pinned, so the engine-wide default (now Sdf) decides.
        Assert.AreEqual(0, gdiGenerator.GenerateCalls);
        Assert.AreEqual(1, sdfGenerator.GenerateCalls);
    }

    [TestMethod]
    public void DefaultTextRenderer_Setter_ClearsCache()
    {
        CountingGenerator gdiGenerator = new();
        CountingGenerator sdfGenerator = new();
        EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Gdi, gdiGenerator);
        EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Sdf, sdfGenerator);
        EngineConfiguration.DefaultTextRenderer = TextRenderer.Gdi;
        FontDefinition unpinnedFont = new("Arial", 12);
        FontTextureCache.GetOrCreate(unpinnedFont);

        EngineConfiguration.DefaultTextRenderer = TextRenderer.Sdf;
        FontTextureCache.GetOrCreate(unpinnedFont);

        // Changing the default must invalidate atlases that resolved through the old default.
        Assert.AreEqual(1, gdiGenerator.GenerateCalls);
        Assert.AreEqual(1, sdfGenerator.GenerateCalls);
    }

    [TestMethod]
    public void RegisterAtlasGenerator_WithNull_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => EngineConfiguration.RegisterAtlasGenerator(TextRenderer.Sdf, null!));
    }

    [TestMethod]
    public void InvalidateCache_DisposesEachCachedAtlasTexture()
    {
        // Each generated atlas owns a fresh ITexture — when the cache is cleared (e.g. backend
        // swap) those textures need to be disposed, otherwise their GL handles leak.
        CountingGenerator generator = new();
        EngineConfiguration.AtlasGenerator = generator;
        FontAtlas atlasA = FontTextureCache.GetOrCreate(new FontDefinition("Arial", 12));
        FontAtlas atlasB = FontTextureCache.GetOrCreate(new FontDefinition("Arial", 16));
        var textureA = (StubTexture)atlasA.Texture;
        var textureB = (StubTexture)atlasB.Texture;

        FontTextureCache.InvalidateCache();

        Assert.AreEqual(1, textureA.DisposeCallCount);
        Assert.AreEqual(1, textureB.DisposeCallCount);
    }

    [TestMethod]
    public void InvalidateCache_DoesNotDisposeAtlasShader()
    {
        // Atlas shaders in production are process-wide singletons (Shaders.SdfText etc.) shared
        // across all atlases of the same backend AND with other graphics. Disposing them would
        // break unrelated rendering.
        CountingGenerator generator = new();
        EngineConfiguration.AtlasGenerator = generator;
        FontAtlas atlas = FontTextureCache.GetOrCreate(new FontDefinition("Arial", 12));
        var shader = (StubShader)atlas.Shader;

        FontTextureCache.InvalidateCache();

        Assert.AreEqual(0, shader.DisposeCallCount);
    }
}

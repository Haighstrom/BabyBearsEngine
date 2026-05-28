using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FontTextureCacheTests
{
    private sealed class StubTexture : ITexture
    {
        public int Handle => 0;
        public int Width => 0;
        public int Height => 0;

        public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class StubShader : IMatrixShaderProgram
    {
        public int Handle => 0;

        public void Bind()
        {
        }

        public void Dispose()
        {
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
    public void Cleanup() => FontTextureCache.Reset();

    [TestMethod]
    public void GetOrCreate_FirstCallForFont_InvokesGenerator()
    {
        CountingGenerator generator = new();
        FontTextureCache.Generator = generator;
        FontDefinition fontDef = new("Arial", 12);

        FontTextureCache.GetOrCreate(fontDef);

        Assert.AreEqual(1, generator.GenerateCalls);
    }

    [TestMethod]
    public void GetOrCreate_SecondCallForSameFont_ReturnsCachedAtlas()
    {
        CountingGenerator generator = new();
        FontTextureCache.Generator = generator;
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
        FontTextureCache.Generator = generator;
        FontDefinition fontA = new("Arial", 12);
        FontDefinition fontB = new("Times New Roman", 14);

        FontTextureCache.GetOrCreate(fontA);
        FontTextureCache.GetOrCreate(fontB);
        FontTextureCache.GetOrCreate(fontA);
        FontTextureCache.GetOrCreate(fontB);

        Assert.AreEqual(2, generator.GenerateCalls);
    }

    [TestMethod]
    public void Generator_Setter_ClearsCache()
    {
        CountingGenerator firstGenerator = new();
        FontTextureCache.Generator = firstGenerator;
        FontDefinition fontDef = new("Arial", 12);
        FontTextureCache.GetOrCreate(fontDef);

        CountingGenerator secondGenerator = new();
        FontTextureCache.Generator = secondGenerator;
        FontTextureCache.GetOrCreate(fontDef);

        Assert.AreEqual(1, firstGenerator.GenerateCalls);
        Assert.AreEqual(1, secondGenerator.GenerateCalls);
    }

    [TestMethod]
    public void Generator_SetterWithNull_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => FontTextureCache.Generator = null!);
    }

    [TestMethod]
    public void Reset_RestoresDefaultGenerator()
    {
        CountingGenerator stubGenerator = new();
        FontTextureCache.Generator = stubGenerator;

        FontTextureCache.Reset();

        Assert.IsInstanceOfType<GdiFontAtlasGenerator>(FontTextureCache.Generator);
    }

    [TestMethod]
    public void Reset_ClearsCache()
    {
        CountingGenerator firstGenerator = new();
        FontTextureCache.Generator = firstGenerator;
        FontDefinition fontDef = new("Arial", 12);
        FontTextureCache.GetOrCreate(fontDef);

        FontTextureCache.Reset();

        CountingGenerator secondGenerator = new();
        FontTextureCache.Generator = secondGenerator;
        FontTextureCache.GetOrCreate(fontDef);

        Assert.AreEqual(1, secondGenerator.GenerateCalls);
    }
}

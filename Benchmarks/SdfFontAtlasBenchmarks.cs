using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Benchmarks;

[MemoryDiagnoser]
public class SdfFontAtlasBenchmarks
{
    private readonly Dictionary<FontDefinition, FontAtlasMetrics> _cache = [];
    private FontAtlasMetrics _cachedMetrics = null!;
    private FontDefinition _fontDef = null!;
    private SdfFontAtlasGenerator _generator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fontDef = new FontDefinition("Arial", 16);
        _generator = new SdfFontAtlasGenerator();
        _cachedMetrics = _generator.RasteriseAtlas(_fontDef).Metrics;
        _cache[_fontDef] = _cachedMetrics;
    }

    // Measures the stb_truetype SDF rasterisation step — the CPU hot path before caching.
    // Uses RasteriseAtlas rather than the public IFontAtlasGenerator.Generate, because
    // Generate also creates a GL texture which requires an active OpenGL context.
    [Benchmark]
    public int GenerateAtlas_Uncached()
    {
        return _generator.RasteriseAtlas(_fontDef).Metrics.HighestChar;
    }

    // Measures the cached path — a plain dictionary lookup.
    [Benchmark]
    public int GenerateAtlas_Cached()
    {
        _cache.TryGetValue(_fontDef, out var result);
        return result?.HighestChar ?? 0;
    }
}

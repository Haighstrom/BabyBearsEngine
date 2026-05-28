using System.Collections.Generic;
using System.Drawing;
using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Benchmarks;

[MemoryDiagnoser]
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public class FontAtlasBenchmarks
{
    private readonly Dictionary<FontDefinition, FontAtlasMetrics> _cache = [];
    private FontAtlasMetrics _cachedMetrics = null!;
    private Font _font = null!;
    private FontDefinition _fontDef = null!;

    [GlobalSetup]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows-only benchmark.")]
    public void Setup()
    {
        _fontDef = new FontDefinition("Arial", 16);
        _font = new FontLoader().LoadFont(_fontDef);
        (_, _cachedMetrics) = new FontBitmapGenerator().GenerateCharSpritesheetAndPositions(
            _font, _fontDef.CharactersToLoad, _fontDef.AntiAliased, 13);
        _cache[_fontDef] = _cachedMetrics;
    }

    // Measures the GDI+ atlas generation — the hot path before caching.
    [Benchmark]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows-only benchmark.")]
    public int GenerateAtlas_Uncached()
    {
        return new FontBitmapGenerator().GenerateCharSpritesheetAndPositions(
            _font, _fontDef.CharactersToLoad, _fontDef.AntiAliased, 13).Metrics.HighestChar;
    }

    // Measures the cached path — a plain dictionary lookup.
    [Benchmark]
    public int GenerateAtlas_Cached()
    {
        _cache.TryGetValue(_fontDef, out var result);
        return result?.HighestChar ?? 0;
    }
}

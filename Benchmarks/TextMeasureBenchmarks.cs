using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BabyBearsEngine.Worlds.Graphics.Text;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Benchmarks;

/// <summary>
/// TextInputBox-style prefix measurement (issue #236): the old Substring path, which allocates a
/// heap string per measured slice, versus the AsSpan path, which is allocation-free. Each op
/// measures four prefix slices of a string of length N — mirroring what UpdateDisplay does per
/// keystroke (cursor X plus selection edges). MemoryDiagnoser shows the substring allocations the
/// span overload eliminates.
/// </summary>
[MemoryDiagnoser]
public class TextMeasureBenchmarks
{
    private FontAtlasMetrics _metrics = null!;
    private string _text = "";

    [Params(16, 64, 256)]
    public int Length;

    [GlobalSetup]
    public void Setup()
    {
        Dictionary<char, Box2i> positions = [];
        for (char c = ' '; c <= '~'; c++)
        {
            positions[c] = new Box2i(0, 0, 8, 16);
        }

        _metrics = new FontAtlasMetrics(8, 16, positions, []);
        _text = new string('a', Length);
    }

    [Benchmark(Baseline = true)]
    public int Substring()
    {
        int total = 0;
        total += _metrics.MeasureString(_text.Substring(0, Length / 4)).X;
        total += _metrics.MeasureString(_text.Substring(0, Length / 2)).X;
        total += _metrics.MeasureString(_text.Substring(0, 3 * Length / 4)).X;
        total += _metrics.MeasureString(_text.Substring(0, Length)).X;
        return total;
    }

    [Benchmark]
    public int Span()
    {
        int total = 0;
        total += _metrics.MeasureString(_text.AsSpan(0, Length / 4)).X;
        total += _metrics.MeasureString(_text.AsSpan(0, Length / 2)).X;
        total += _metrics.MeasureString(_text.AsSpan(0, 3 * Length / 4)).X;
        total += _metrics.MeasureString(_text.AsSpan(0, Length)).X;
        return total;
    }
}

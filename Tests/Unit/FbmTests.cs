using System;
using BabyBearsEngine.Utilities.Noise;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FbmTests
{
    private const int Seed = 12345;

    [TestMethod]
    public void Sample_SingleOctave_EqualsRawSourceSample()
    {
        SimplexNoise noise = new(Seed);
        float raw = noise.Sample(0.5f, 0.5f);
        float fbm = Fbm.Sample(noise, 0.5f, 0.5f, octaves: 1);

        Assert.AreEqual(raw, fbm, 1e-6f);
    }

    [TestMethod]
    public void Sample_MultipleOctaves_StaysWithinSourceRange()
    {
        SimplexNoise noise = new(Seed);
        const float Tolerance = 0.05f;

        for (int sampleIndex = 0; sampleIndex < 1_000; sampleIndex++)
        {
            float x = sampleIndex * 0.137f;
            float y = sampleIndex * 0.271f;
            float fbm = Fbm.Sample(noise, x, y, octaves: 6);

            Assert.IsGreaterThanOrEqualTo(-1f - Tolerance, fbm);
            Assert.IsLessThanOrEqualTo(1f + Tolerance, fbm);
        }
    }

    [TestMethod]
    public void Sample_DeterministicForFixedNoise()
    {
        SimplexNoise noise = new(Seed);

        float first = Fbm.Sample(noise, 1.23f, 4.56f, octaves: 4);
        float second = Fbm.Sample(noise, 1.23f, 4.56f, octaves: 4);

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void Sample_MoreOctaves_DifferFromFewerOctaves()
    {
        SimplexNoise noise = new(Seed);
        float fewOctaves = Fbm.Sample(noise, 1.23f, 4.56f, octaves: 1);
        float manyOctaves = Fbm.Sample(noise, 1.23f, 4.56f, octaves: 8);

        Assert.AreNotEqual(fewOctaves, manyOctaves);
    }

    [TestMethod]
    public void Sample_ZeroOctaves_Throws()
    {
        SimplexNoise noise = new(Seed);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Fbm.Sample(noise, 0f, 0f, octaves: 0));
    }

    [TestMethod]
    public void Sample_NegativeOctaves_Throws()
    {
        SimplexNoise noise = new(Seed);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Fbm.Sample(noise, 0f, 0f, octaves: -1));
    }
}

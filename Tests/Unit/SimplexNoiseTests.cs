using System;
using BabyBearsEngine.Utilities.Noise;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class SimplexNoiseTests
{
    private const int Seed = 12345;

    // Seeding & determinism

    [TestMethod]
    public void Sample2D_SameSeed_ProducesIdenticalOutput()
    {
        SimplexNoise first = new(Seed);
        SimplexNoise second = new(Seed);

        for (int sampleIndex = 0; sampleIndex < 50; sampleIndex++)
        {
            float x = sampleIndex * 0.13f;
            float y = sampleIndex * 0.27f;
            Assert.AreEqual(first.Sample(x, y), second.Sample(x, y));
        }
    }

    [TestMethod]
    public void Sample3D_SameSeed_ProducesIdenticalOutput()
    {
        SimplexNoise first = new(Seed);
        SimplexNoise second = new(Seed);

        for (int sampleIndex = 0; sampleIndex < 50; sampleIndex++)
        {
            float x = sampleIndex * 0.13f;
            float y = sampleIndex * 0.27f;
            float z = sampleIndex * 0.41f;
            Assert.AreEqual(first.Sample(x, y, z), second.Sample(x, y, z));
        }
    }

    [TestMethod]
    public void DifferentSeeds_ProduceDifferentOutput()
    {
        SimplexNoise first = new(1);
        SimplexNoise second = new(2);

        int differing = 0;
        for (int sampleIndex = 0; sampleIndex < 50; sampleIndex++)
        {
            float x = sampleIndex * 0.13f;
            float y = sampleIndex * 0.27f;
            if (first.Sample(x, y) != second.Sample(x, y))
            {
                differing++;
            }
        }

        Assert.IsGreaterThan(40, differing, "Two differently-seeded simplex noises should disagree on the vast majority of samples");
    }

    [TestMethod]
    public void Seed_IsExposedAsProperty()
    {
        SimplexNoise noise = new(Seed);
        Assert.AreEqual(Seed, noise.Seed);
    }

    [TestMethod]
    public void ParameterlessConstructor_StillUsable()
    {
        SimplexNoise noise = new();
        // Just confirm the random-seed ctor doesn't throw and produces *some* value.
        float value = noise.Sample(1f, 2f);
        Assert.IsTrue(float.IsFinite(value));
    }

    // Range

    [TestMethod]
    public void Sample2D_OutputStaysWithinExpectedRange()
    {
        SimplexNoise noise = new(Seed);
        const float Tolerance = 0.05f;

        for (int sampleIndex = 0; sampleIndex < 10_000; sampleIndex++)
        {
            float x = sampleIndex * 0.137f;
            float y = sampleIndex * 0.271f;
            float value = noise.Sample(x, y);

            Assert.IsGreaterThanOrEqualTo(-1f - Tolerance, value);
            Assert.IsLessThanOrEqualTo(1f + Tolerance, value);
        }
    }

    [TestMethod]
    public void Sample3D_OutputStaysWithinExpectedRange()
    {
        SimplexNoise noise = new(Seed);
        const float Tolerance = 0.05f;

        for (int sampleIndex = 0; sampleIndex < 10_000; sampleIndex++)
        {
            float x = sampleIndex * 0.137f;
            float y = sampleIndex * 0.271f;
            float z = sampleIndex * 0.413f;
            float value = noise.Sample(x, y, z);

            Assert.IsGreaterThanOrEqualTo(-1f - Tolerance, value);
            Assert.IsLessThanOrEqualTo(1f + Tolerance, value);
        }
    }

    // Continuity — close samples should produce close output (no JPEG-style jumps)

    [TestMethod]
    public void Sample2D_NeighbouringSamplesAreClose()
    {
        SimplexNoise noise = new(Seed);
        const float Step = 0.001f;
        const float MaxAcceptableJump = 0.05f;

        for (int sampleIndex = 0; sampleIndex < 1_000; sampleIndex++)
        {
            float x = sampleIndex * 0.13f;
            float y = sampleIndex * 0.27f;

            float here = noise.Sample(x, y);
            float justRight = noise.Sample(x + Step, y);
            float justBelow = noise.Sample(x, y + Step);

            Assert.IsLessThanOrEqualTo(MaxAcceptableJump, Math.Abs(justRight - here));
            Assert.IsLessThanOrEqualTo(MaxAcceptableJump, Math.Abs(justBelow - here));
        }
    }

    [TestMethod]
    public void Sample2D_AtOrigin_DoesNotThrow()
    {
        SimplexNoise noise = new(Seed);
        // Origin is a degenerate cell corner; confirm it produces a finite result.
        float value = noise.Sample(0f, 0f);
        Assert.IsTrue(float.IsFinite(value));
    }
}

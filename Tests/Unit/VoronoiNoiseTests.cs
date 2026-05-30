using System;
using BabyBearsEngine.Utilities.Noise;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class VoronoiNoiseTests
{
    private const int Seed = 12345;

    // Seeding & determinism

    [TestMethod]
    public void Sample_SameSeed_ProducesIdenticalOutput()
    {
        VoronoiNoise first = new(Seed);
        VoronoiNoise second = new(Seed);

        for (int sampleIndex = 0; sampleIndex < 50; sampleIndex++)
        {
            float x = sampleIndex * 0.13f;
            float y = sampleIndex * 0.27f;

            VoronoiSample fromFirst = first.Sample(x, y);
            VoronoiSample fromSecond = second.Sample(x, y);

            Assert.AreEqual(fromFirst.F1, fromSecond.F1);
            Assert.AreEqual(fromFirst.F2, fromSecond.F2);
        }
    }

    [TestMethod]
    public void DifferentSeeds_ProduceDifferentOutput()
    {
        VoronoiNoise first = new(1);
        VoronoiNoise second = new(2);

        int differing = 0;
        for (int sampleIndex = 0; sampleIndex < 50; sampleIndex++)
        {
            float x = sampleIndex * 0.13f;
            float y = sampleIndex * 0.27f;
            if (first.Sample(x, y).F1 != second.Sample(x, y).F1)
            {
                differing++;
            }
        }

        Assert.IsGreaterThan(40, differing, "Two differently-seeded Voronoi noises should disagree on the vast majority of samples");
    }

    [TestMethod]
    public void Seed_IsExposedAsProperty()
    {
        VoronoiNoise noise = new(Seed);
        Assert.AreEqual(Seed, noise.Seed);
    }

    // Geometric properties

    [TestMethod]
    public void Sample_F1IsAtMostF2()
    {
        VoronoiNoise noise = new(Seed);

        for (int sampleIndex = 0; sampleIndex < 1_000; sampleIndex++)
        {
            float x = sampleIndex * 0.137f;
            float y = sampleIndex * 0.271f;
            VoronoiSample sample = noise.Sample(x, y);

            Assert.IsLessThanOrEqualTo(sample.F2, sample.F1);
            Assert.IsGreaterThanOrEqualTo(0f, sample.EdgeDistance);
        }
    }

    [TestMethod]
    public void Sample_F1IsAtMostMaxNeighbourDistance()
    {
        // With a 3x3 cell search the closest point can be at most √2 ≈ 1.414 in cell units; allow a touch of slack.
        const float MaxPossibleF1 = 1.5f;
        VoronoiNoise noise = new(Seed);

        for (int sampleIndex = 0; sampleIndex < 1_000; sampleIndex++)
        {
            float x = sampleIndex * 0.137f;
            float y = sampleIndex * 0.271f;
            VoronoiSample sample = noise.Sample(x, y);

            Assert.IsGreaterThanOrEqualTo(0f, sample.F1);
            Assert.IsLessThanOrEqualTo(MaxPossibleF1, sample.F1);
        }
    }

    [TestMethod]
    public void Sample_OutputIsFiniteAtAxisAlignedLocations()
    {
        VoronoiNoise noise = new(Seed);

        VoronoiSample atOrigin = noise.Sample(0f, 0f);
        VoronoiSample onXAxis = noise.Sample(5f, 0f);
        VoronoiSample onYAxis = noise.Sample(0f, 5f);
        VoronoiSample atCellEdge = noise.Sample(1f, 1f);

        foreach (VoronoiSample sample in new[] { atOrigin, onXAxis, onYAxis, atCellEdge })
        {
            Assert.IsTrue(float.IsFinite(sample.F1));
            Assert.IsTrue(float.IsFinite(sample.F2));
        }
    }

    [TestMethod]
    public void EdgeDistance_EqualsF2MinusF1()
    {
        VoronoiNoise noise = new(Seed);
        VoronoiSample sample = noise.Sample(3.7f, 9.1f);

        Assert.AreEqual(sample.F2 - sample.F1, sample.EdgeDistance);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class SystemRandomTests
{
    // ─── Int(int, int) ───

    [TestMethod]
    public void Int_MinMax_ResultInRange()
    {
        SystemRandom random = new(seed: 12345);

        for (int i = 0; i < 200; i++)
        {
            int value = random.Int(5, 15);
            Assert.IsGreaterThanOrEqualTo(5, value);
            Assert.IsLessThan(15, value);
        }
    }

    [TestMethod]
    public void Int_MinEqualsMax_ReturnsThatValue()
    {
        SystemRandom random = new(seed: 1);

        Assert.AreEqual(7, random.Int(7, 7));
    }

    [TestMethod]
    public void Int_MaxLessThanMin_Throws()
    {
        SystemRandom random = new(seed: 1);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => random.Int(10, 5));
    }

    // ─── Double ───

    [TestMethod]
    public void Double_ResultInRange()
    {
        SystemRandom random = new(seed: 12345);

        for (int i = 0; i < 200; i++)
        {
            double value = random.Double();
            Assert.IsGreaterThanOrEqualTo(0.0, value);
            Assert.IsLessThan(1.0, value);
        }
    }

    // ─── Seeded reproducibility ───

    [TestMethod]
    public void Seed_SameSeed_ProducesIdenticalSequence()
    {
        SystemRandom a = new(seed: 42);
        SystemRandom b = new(seed: 42);

        for (int i = 0; i < 50; i++)
        {
            Assert.AreEqual(a.Double(), b.Double());
            Assert.AreEqual(a.Int(0, 1000), b.Int(0, 1000));
        }
    }

    [TestMethod]
    public void Seed_DifferentSeeds_ProduceDifferentSequences()
    {
        SystemRandom a = new(seed: 1);
        SystemRandom b = new(seed: 2);

        // It's astronomically unlikely 50 samples agree, but we only need one disagreement to prove independence.
        bool sawDifference = false;
        for (int i = 0; i < 50 && !sawDifference; i++)
        {
            if (a.Double() != b.Double())
            {
                sawDifference = true;
            }
        }

        Assert.IsTrue(sawDifference);
    }

    [TestMethod]
    public void Parameterless_DoesNotThrow()
    {
        SystemRandom random = new();

        _ = random.Double();
        _ = random.Int(0, 10);
    }

    // ─── Thread safety ───

    [TestMethod]
    public void ConcurrentAccess_DoesNotCorruptStateOrThrow()
    {
        SystemRandom random = new();
        const int threadCount = 8;
        const int callsPerThread = 5000;
        int totalCalls = threadCount * callsPerThread;
        int nonZeroCount = 0;

        Parallel.For(0, threadCount, _ =>
        {
            int localNonZero = 0;
            for (int i = 0; i < callsPerThread; i++)
            {
                double value = random.Double();
                if (value > 0.0)
                {
                    localNonZero++;
                }
            }
            Interlocked.Add(ref nonZeroCount, localNonZero);
        });

        // Without the lock, a corrupted Random can lock into returning all zeros. Require
        // that the vast majority of samples are non-zero — a healthy Random hits 0.0
        // essentially never.
        Assert.IsGreaterThan(totalCalls - 10, nonZeroCount);
    }
}

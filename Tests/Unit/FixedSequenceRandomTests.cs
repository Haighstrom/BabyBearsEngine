namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FixedSequenceRandomTests
{
    // ─── Int(int, int) ───

    [TestMethod]
    public void Int_ReturnsQueuedIntsInOrder()
    {
        FixedSequenceRandom random = new(ints: [3, 7, 1]);

        Assert.AreEqual(3, random.Int(0, 10));
        Assert.AreEqual(7, random.Int(0, 10));
        Assert.AreEqual(1, random.Int(0, 10));
    }

    [TestMethod]
    public void Int_IgnoresMinMaxArguments()
    {
        // Deliberate test of the documented "what you queue is what you get" behaviour —
        // the fake does NOT clamp to the requested range.
        FixedSequenceRandom random = new(ints: [999]);

        Assert.AreEqual(999, random.Int(0, 10));
    }

    [TestMethod]
    public void Int_EmptyQueue_Throws()
    {
        FixedSequenceRandom random = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => random.Int(0, 10));
    }

    [TestMethod]
    public void Int_AfterExhausting_Throws()
    {
        FixedSequenceRandom random = new(ints: [1]);
        _ = random.Int(0, 10);

        Assert.ThrowsExactly<InvalidOperationException>(() => random.Int(0, 10));
    }

    // ─── Double ───

    [TestMethod]
    public void Double_ReturnsQueuedDoublesInOrder()
    {
        FixedSequenceRandom random = new(doubles: [0.1, 0.5, 0.9]);

        Assert.AreEqual(0.1, random.Double());
        Assert.AreEqual(0.5, random.Double());
        Assert.AreEqual(0.9, random.Double());
    }

    [TestMethod]
    public void Double_EmptyQueue_Throws()
    {
        FixedSequenceRandom random = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => random.Double());
    }

    [TestMethod]
    public void Double_AfterExhausting_Throws()
    {
        FixedSequenceRandom random = new(doubles: [0.5]);
        _ = random.Double();

        Assert.ThrowsExactly<InvalidOperationException>(() => random.Double());
    }

    // ─── Independent queues ───

    [TestMethod]
    public void IntAndDoubleQueues_AreIndependent()
    {
        FixedSequenceRandom random = new(ints: [42], doubles: [0.3]);

        Assert.AreEqual(0.3, random.Double());
        Assert.AreEqual(42, random.Int(0, 100));
    }

    [TestMethod]
    public void Double_DoesNotConsumeInts()
    {
        FixedSequenceRandom random = new(ints: [5], doubles: [0.7]);

        _ = random.Double();

        Assert.AreEqual(1, random.IntsRemaining);
        Assert.AreEqual(5, random.Int(0, 10));
    }

    // ─── Counters ───

    [TestMethod]
    public void IntsRemaining_ReflectsQueueDepth()
    {
        FixedSequenceRandom random = new(ints: [1, 2, 3]);

        Assert.AreEqual(3, random.IntsRemaining);
        _ = random.Int(0, 10);
        Assert.AreEqual(2, random.IntsRemaining);
    }

    [TestMethod]
    public void DoublesRemaining_ReflectsQueueDepth()
    {
        FixedSequenceRandom random = new(doubles: [0.1, 0.2]);

        Assert.AreEqual(2, random.DoublesRemaining);
        _ = random.Double();
        Assert.AreEqual(1, random.DoublesRemaining);
    }

    // ─── Factories ───

    [TestMethod]
    public void FromDoubles_LoadsDoublesOnly()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.25, 0.75);

        Assert.AreEqual(2, random.DoublesRemaining);
        Assert.AreEqual(0, random.IntsRemaining);
        Assert.AreEqual(0.25, random.Double());
    }

    [TestMethod]
    public void FromInts_LoadsIntsOnly()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(11, 22);

        Assert.AreEqual(2, random.IntsRemaining);
        Assert.AreEqual(0, random.DoublesRemaining);
        Assert.AreEqual(11, random.Int(0, 100));
    }

    [TestMethod]
    public void DefaultConstructor_BothQueuesEmpty()
    {
        FixedSequenceRandom random = new();

        Assert.AreEqual(0, random.IntsRemaining);
        Assert.AreEqual(0, random.DoublesRemaining);
    }
}

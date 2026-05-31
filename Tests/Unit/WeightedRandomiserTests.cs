using System;
using System.Collections.Generic;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class WeightedRandomiserTests
{
    /// <summary>
    /// Returns a preset sequence of doubles from a queue. NextDouble() should return
    /// values in [0, 1) — the caller is responsible for supplying values in that range.
    /// </summary>
    private sealed class StubRandom(params double[] values) : IRandom
    {
        private readonly Queue<double> _values = new(values);

        public int Int(int minInclusive, int maxExclusive) => throw new NotImplementedException("WeightedRandomiser only uses Double.");

        public double Double() => _values.Dequeue();
    }

    // ─── Count / Clear ───

    [TestMethod]
    public void Count_AfterAdding_ReflectsNumberOfItems()
    {
        WeightedRandomiser<string> wr = new();
        wr.Add("A", 1);
        wr.Add("B", 2);

        Assert.AreEqual(2, wr.Count);
    }

    [TestMethod]
    public void Clear_RemovesAllItems_CountIsZero()
    {
        WeightedRandomiser<string> wr = new();
        wr.Add("A", 1);
        wr.Add("B", 2);

        wr.Clear();

        Assert.AreEqual(0, wr.Count);
    }

    [TestMethod]
    public void Next_AfterClear_Throws()
    {
        WeightedRandomiser<string> wr = new();
        wr.Add("A", 1);
        wr.Clear();

        Assert.ThrowsExactly<InvalidOperationException>(() => wr.Next());
    }

    // ─── Input validation ───

    [TestMethod]
    public void Add_NegativeWeight_ThrowsArgumentOutOfRangeException()
    {
        WeightedRandomiser<string> wr = new();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => wr.Add("A", -1.0));
    }

    [TestMethod]
    public void Next_EmptyRandomiser_ThrowsInvalidOperationException()
    {
        WeightedRandomiser<string> wr = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => wr.Next());
    }

    // ─── Single item ───

    [TestMethod]
    public void Next_SingleItem_AlwaysReturnsThatItem()
    {
        // roll of 0.0 → 0.0 * 1.0 = 0.0 → cumulative 1.0 > 0.0 → "only"
        // roll of 0.99 → 0.99 * 1.0 = 0.99 → cumulative 1.0 > 0.99 → "only"
        WeightedRandomiser<string> wr = new(new StubRandom(0.0, 0.99));
        wr.Add("only", 1);

        Assert.AreEqual("only", wr.Next());
        Assert.AreEqual("only", wr.Next());
    }

    // ─── Zero weight ───

    [TestMethod]
    public void Next_ZeroWeightItem_IsNeverSelected()
    {
        // Total weight = 1. Any roll in [0,1) maps to [0,1), which always reaches "always"
        // before "never" (weight 0 contributes nothing to cumulative).
        WeightedRandomiser<string> wr = new(new StubRandom(0.0, 0.5, 0.99));
        wr.Add("never", 0);
        wr.Add("always", 1);

        Assert.AreEqual("always", wr.Next());
        Assert.AreEqual("always", wr.Next());
        Assert.AreEqual("always", wr.Next());
    }

    // ─── Selection by roll value ───

    [TestMethod]
    public void Next_EqualWeights_LowRollSelectsFirst()
    {
        // Total weight = 2. roll=0.0 → r=0.0 → cumulative hits A at 1.0 → A wins.
        WeightedRandomiser<string> wr = new(new StubRandom(0.0));
        wr.Add("A", 1);
        wr.Add("B", 1);

        Assert.AreEqual("A", wr.Next());
    }

    [TestMethod]
    public void Next_EqualWeights_HighRollSelectsSecond()
    {
        // Total weight = 2. roll=0.99 → r=1.98 → A cumulative=1.0 < 1.98, B cumulative=2.0 >= 1.98 → B wins.
        WeightedRandomiser<string> wr = new(new StubRandom(0.99));
        wr.Add("A", 1);
        wr.Add("B", 1);

        Assert.AreEqual("B", wr.Next());
    }

    [TestMethod]
    public void Next_UnequalWeights_MidRollSelectsHeavierItem()
    {
        // Weights: A=3, B=1. Total=4. roll=0.5 → r=2.0 → A cumulative=3.0 >= 2.0 → A wins.
        WeightedRandomiser<string> wr = new(new StubRandom(0.5));
        wr.Add("A", 3);
        wr.Add("B", 1);

        Assert.AreEqual("A", wr.Next());
    }

    [TestMethod]
    public void Next_UnequalWeights_HighRollSelectsLighterItem()
    {
        // Weights: A=3, B=1. Total=4. roll=0.99 → r=3.96 → A cumulative=3.0 < 3.96, B cumulative=4.0 >= 3.96 → B wins.
        WeightedRandomiser<string> wr = new(new StubRandom(0.99));
        wr.Add("A", 3);
        wr.Add("B", 1);

        Assert.AreEqual("B", wr.Next());
    }

    [TestMethod]
    public void Next_ThreeItems_CorrectItemSelectedForEachRegion()
    {
        // Weights: A=1, B=2, C=1. Total=4. Regions: A=[0,1), B=[1,3), C=[3,4)
        // roll=0.1 → r=0.4 → A wins
        // roll=0.4 → r=1.6 → B wins
        // roll=0.8 → r=3.2 → C wins
        WeightedRandomiser<string> wr = new(new StubRandom(0.1, 0.4, 0.8));
        wr.Add("A", 1);
        wr.Add("B", 2);
        wr.Add("C", 1);

        Assert.AreEqual("A", wr.Next());
        Assert.AreEqual("B", wr.Next());
        Assert.AreEqual("C", wr.Next());
    }
}

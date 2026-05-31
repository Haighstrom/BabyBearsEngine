namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RandomExtensionsTests
{
    private enum TestEnum
    {
        Alpha,
        Beta,
        Gamma,
    }

    // ─── Int(int max) ───

    [TestMethod]
    public void Int_MaxOnly_DelegatesToFullRangeWithZeroMin()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(4);

        Assert.AreEqual(4, random.Int(10));
    }

    [TestMethod]
    public void Int_MaxZero_ReturnsZeroWithoutCallingPrimitive()
    {
        // Empty queue would throw if Int(0) reached the primitive — proves the short-circuit.
        FixedSequenceRandom random = new();

        Assert.AreEqual(0, random.Int(0));
    }

    [TestMethod]
    public void Int_NegativeMax_Throws()
    {
        FixedSequenceRandom random = new();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => random.Int(-1));
    }

    // ─── Double(max) ───

    [TestMethod]
    public void Double_Max_ScalesByMax()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.5);

        Assert.AreEqual(5.0, random.Double(10.0));
    }

    [TestMethod]
    public void Double_MinMax_ScalesAndOffsets()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.5);

        // 2 + 0.5 * (10 - 2) = 6
        Assert.AreEqual(6.0, random.Double(2.0, 10.0));
    }

    // ─── Float ───

    [TestMethod]
    public void Float_NoArgs_ReturnsDoubleCast()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.5);

        Assert.AreEqual(0.5f, random.Float());
    }

    [TestMethod]
    public void Float_Max_ScalesByMax()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.25);

        Assert.AreEqual(2.5f, random.Float(10f));
    }

    [TestMethod]
    public void Float_MinMax_ScalesAndOffsets()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.5);

        // 2 + 0.5 * (10 - 2) = 6
        Assert.AreEqual(6f, random.Float(2f, 10f));
    }

    // ─── Chance ───

    [TestMethod]
    public void Chance_ZeroProbability_ReturnsFalseWithoutCallingPrimitive()
    {
        FixedSequenceRandom random = new();

        Assert.IsFalse(random.Chance(0f));
    }

    [TestMethod]
    public void Chance_OneProbability_ReturnsTrueWithoutCallingPrimitive()
    {
        FixedSequenceRandom random = new();

        Assert.IsTrue(random.Chance(1f));
    }

    [TestMethod]
    public void Chance_RollBelowThreshold_ReturnsTrue()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.1);

        Assert.IsTrue(random.Chance(0.5f));
    }

    [TestMethod]
    public void Chance_RollAboveThreshold_ReturnsFalse()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.9);

        Assert.IsFalse(random.Chance(0.5f));
    }

    // ─── ChancePercent ───

    [TestMethod]
    public void ChancePercent_FiftyPercentRollBelow_ReturnsTrue()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.4);

        Assert.IsTrue(random.ChancePercent(50f));
    }

    [TestMethod]
    public void ChancePercent_FiftyPercentRollAbove_ReturnsFalse()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromDoubles(0.6);

        Assert.IsFalse(random.ChancePercent(50f));
    }

    [TestMethod]
    public void ChancePercent_Zero_AlwaysFalse()
    {
        FixedSequenceRandom random = new();

        Assert.IsFalse(random.ChancePercent(0f));
    }

    [TestMethod]
    public void ChancePercent_Hundred_AlwaysTrue()
    {
        FixedSequenceRandom random = new();

        Assert.IsTrue(random.ChancePercent(100f));
    }

    // ─── Choose ───

    [TestMethod]
    public void Choose_Params_ReturnsElementAtRolledIndex()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(1);

        Assert.AreEqual("B", random.Choose("A", "B", "C"));
    }

    [TestMethod]
    public void Choose_EmptyParams_Throws()
    {
        FixedSequenceRandom random = new();

        Assert.ThrowsExactly<ArgumentException>(() => random.Choose<string>([]));
    }

    [TestMethod]
    public void Choose_IList_ReturnsElementAtRolledIndex()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(2);
        List<int> items = [10, 20, 30];

        Assert.AreEqual(30, random.Choose((IList<int>)items));
    }

    [TestMethod]
    public void Choose_EmptyIList_Throws()
    {
        FixedSequenceRandom random = new();
        List<int> empty = [];

        Assert.ThrowsExactly<ArgumentException>(() => random.Choose((IList<int>)empty));
    }

    // ─── Enum ───

    [TestMethod]
    public void Enum_ReturnsValueAtRolledIndex()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(1);

        Assert.AreEqual(TestEnum.Beta, random.Enum<TestEnum>());
    }

    // ─── Shuffle ───

    [TestMethod]
    public void Shuffle_Array_ReturnsSameReference()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(0, 0, 0, 0);
        int[] array = [1, 2, 3, 4, 5];

        int[] returned = random.Shuffle(array);

        Assert.AreSame(array, returned);
    }

    [TestMethod]
    public void Shuffle_Array_PreservesElementSet()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(1, 0, 2, 1);
        int[] array = [1, 2, 3, 4, 5];

        random.Shuffle(array);

        CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, array);
    }

    [TestMethod]
    public void Shuffle_Array_DeterministicWithKnownRolls()
    {
        // Fisher–Yates iterates i = n-1 down to 1, swapping array[i] with array[swap].
        // For [1,2,3,4]:
        //   i=3, swap=0: swap(3,0) -> [4,2,3,1]
        //   i=2, swap=2: swap(2,2) -> [4,2,3,1] (no change)
        //   i=1, swap=0: swap(1,0) -> [2,4,3,1]
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(0, 2, 0);
        int[] array = [1, 2, 3, 4];

        random.Shuffle(array);

        CollectionAssert.AreEqual(new[] { 2, 4, 3, 1 }, array);
    }

    [TestMethod]
    public void Shuffle_IList_PreservesElementSet()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(1, 0, 2, 1);
        List<int> list = [1, 2, 3, 4, 5];

        random.Shuffle(list);

        CollectionAssert.AreEquivalent(new List<int> { 1, 2, 3, 4, 5 }, list);
    }

    [TestMethod]
    public void Shuffle_SingleElement_NoRandomCalls()
    {
        // Empty queue would throw if any roll were made; the loop body shouldn't execute when count=1.
        FixedSequenceRandom random = new();
        int[] array = [42];

        random.Shuffle(array);

        Assert.AreEqual(42, array[0]);
    }

    [TestMethod]
    public void Shuffle_EmptyArray_NoRandomCalls()
    {
        FixedSequenceRandom random = new();
        int[] array = [];

        random.Shuffle(array);

        Assert.AreEqual(0, array.Length);
    }

    // ─── GaussianApprox ───

    [TestMethod]
    public void GaussianApprox_ResultInRange()
    {
        SystemRandom random = new(seed: 1);

        for (int i = 0; i < 200; i++)
        {
            float value = random.GaussianApprox(0f, 10f);
            Assert.IsGreaterThanOrEqualTo(0f, value);
            Assert.IsLessThanOrEqualTo(10f, value);
        }
    }

    [TestMethod]
    public void GaussianApprox_MaxEqualsMin_ReturnsMax()
    {
        SystemRandom random = new(seed: 1);

        Assert.AreEqual(5f, random.GaussianApprox(5f, 5f));
    }

    [TestMethod]
    public void GaussianApprox_MaxLessThanMin_ReturnsMax()
    {
        SystemRandom random = new(seed: 1);

        Assert.AreEqual(3f, random.GaussianApprox(10f, 3f));
    }

    // ─── UpperCaseString ───

    [TestMethod]
    public void UpperCaseString_ReturnsCorrectLength()
    {
        FixedSequenceRandom random = new(ints: Enumerable.Repeat(0, 8));

        string result = random.UpperCaseString(8);

        Assert.AreEqual(8, result.Length);
    }

    [TestMethod]
    public void UpperCaseString_OnlyUppercaseAToZ()
    {
        SystemRandom random = new(seed: 12345);

        string result = random.UpperCaseString(50);

        Assert.IsTrue(result.All(c => c is >= 'A' and <= 'Z'));
    }

    [TestMethod]
    public void UpperCaseString_ZeroLength_ReturnsEmpty()
    {
        FixedSequenceRandom random = new();

        Assert.AreEqual(string.Empty, random.UpperCaseString(0));
    }

    [TestMethod]
    public void UpperCaseString_NegativeLength_Throws()
    {
        FixedSequenceRandom random = new();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => random.UpperCaseString(-1));
    }

    [TestMethod]
    public void UpperCaseString_IndexZero_ReturnsA()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(0, 25);

        Assert.AreEqual("AZ", random.UpperCaseString(2));
    }

    // ─── Colour ───

    [TestMethod]
    public void Colour_AlphaIsMax()
    {
        SystemRandom random = new(seed: 12345);

        for (int i = 0; i < 50; i++)
        {
            Colour colour = random.Colour();
            Assert.AreEqual(byte.MaxValue, colour.A);
        }
    }

    [TestMethod]
    public void Colour_UsesRolledRgb()
    {
        FixedSequenceRandom random = FixedSequenceRandom.FromInts(100, 150, 200);

        Colour colour = random.Colour();

        Assert.AreEqual((byte)100, colour.R);
        Assert.AreEqual((byte)150, colour.G);
        Assert.AreEqual((byte)200, colour.B);
        Assert.AreEqual(byte.MaxValue, colour.A);
    }

    // ─── NamedColour ───

    [TestMethod]
    public void NamedColour_ResultIsAlwaysOneOfTheNamedStaticColours()
    {
        SystemRandom random = new(seed: 12345);
        IReadOnlyList<Colour> named = DiscoverNamedColours();

        for (int i = 0; i < 50; i++)
        {
            Colour result = random.NamedColour();
            Assert.Contains(result, named);
        }
    }

    [TestMethod]
    public void NamedColour_NamedSetIsNonEmpty()
    {
        // Sanity: Colour must expose at least some named static colours — otherwise NamedColour
        // would have nothing to choose from and would throw at runtime.
        IReadOnlyList<Colour> named = DiscoverNamedColours();
        Assert.IsGreaterThan(0, named.Count);
    }

    private static IReadOnlyList<Colour> DiscoverNamedColours()
    {
        List<Colour> colours = [];
        foreach (System.Reflection.PropertyInfo property in typeof(Colour).GetProperties(
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
        {
            if (property.GetValue(null) is Colour colour)
            {
                colours.Add(colour);
            }
        }
        return colours;
    }
}

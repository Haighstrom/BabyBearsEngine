namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RandomisationTests
{
    // ─── Chance / ChancePercent ───

    [TestMethod]
    public void Chance_ZeroProbability_AlwaysFalse()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.IsFalse(Randomisation.Chance(0f));
        }
    }

    [TestMethod]
    public void Chance_OneProbability_AlwaysTrue()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.IsTrue(Randomisation.Chance(1f));
        }
    }

    [TestMethod]
    public void ChancePercent_Zero_AlwaysFalse()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.IsFalse(Randomisation.ChancePercent(0f));
        }
    }

    [TestMethod]
    public void ChancePercent_Hundred_AlwaysTrue()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.IsTrue(Randomisation.ChancePercent(100f));
        }
    }

    // ─── Int(int max) ───

    [TestMethod]
    public void Int_MaxZero_ReturnsZero()
    {
        Assert.AreEqual(0, Randomisation.Int(0));
    }

    [TestMethod]
    public void Int_MaxNegative_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Randomisation.Int(-5));
    }

    [TestMethod]
    public void Int_Max_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            int result = Randomisation.Int(10);
            Assert.IsGreaterThanOrEqualTo(0, result);
            Assert.IsLessThan(10, result);
        }
    }

    // ─── Int(int min, int max) ───

    [TestMethod]
    public void Int_MinMax_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            int result = Randomisation.Int(5, 15);
            Assert.IsGreaterThanOrEqualTo(5, result);
            Assert.IsLessThan(15, result);
        }
    }

    [TestMethod]
    public void Int_MinEqualsMax_ReturnsThatValue()
    {
        // min == max means range [min, max) is empty; the IRandom contract returns min in this case.
        int result = Randomisation.Int(7, 7);
        Assert.AreEqual(7, result);
    }

    [TestMethod]
    public void Int_MaxLessThanMin_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Randomisation.Int(10, 5));
    }

    // ─── Float ───

    [TestMethod]
    public void Float_NoArgs_ResultInUnitInterval()
    {
        for (int i = 0; i < 100; i++)
        {
            float result = Randomisation.Float();
            Assert.IsGreaterThanOrEqualTo(0f, result);
            Assert.IsLessThan(1f, result);
        }
    }

    [TestMethod]
    public void Float_Max_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            float result = Randomisation.Float(5f);
            Assert.IsGreaterThanOrEqualTo(0f, result);
            Assert.IsLessThan(5f, result);
        }
    }

    [TestMethod]
    public void Float_MinMax_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            float result = Randomisation.Float(2f, 8f);
            Assert.IsGreaterThanOrEqualTo(2f, result);
            Assert.IsLessThanOrEqualTo(8f, result);
        }
    }

    [TestMethod]
    public void Float_MaxEqualsMin_ReturnsMax()
    {
        float result = Randomisation.Float(5f, 5f);
        Assert.AreEqual(5f, result);
    }

    [TestMethod]
    public void Float_MaxLessThanMin_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Randomisation.Float(10f, 3f));
    }

    [TestMethod]
    public void Float_MaxNegative_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Randomisation.Float(-1f));
    }

    // ─── Double ───

    [TestMethod]
    public void Double_NoArgs_ResultInUnitInterval()
    {
        for (int i = 0; i < 100; i++)
        {
            double result = Randomisation.Double();
            Assert.IsGreaterThanOrEqualTo(0.0, result);
            Assert.IsLessThan(1.0, result);
        }
    }

    [TestMethod]
    public void Double_MaxNegative_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Randomisation.Double(-1.0));
    }

    [TestMethod]
    public void Double_MaxLessThanMin_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Randomisation.Double(10.0, 3.0));
    }

    // ─── GaussianApprox ───

    [TestMethod]
    public void GaussianApprox_ResultInRange()
    {
        for (int i = 0; i < 200; i++)
        {
            float result = Randomisation.GaussianApprox(0f, 10f);
            Assert.IsGreaterThanOrEqualTo(0f, result);
            Assert.IsLessThanOrEqualTo(10f, result);
        }
    }

    [TestMethod]
    public void GaussianApprox_MaxEqualsMin_ReturnsMax()
    {
        float result = Randomisation.GaussianApprox(5f, 5f);
        Assert.AreEqual(5f, result);
    }

    // ─── Choose ───

    [TestMethod]
    public void Choose_Params_ReturnsElementFromArray()
    {
        string[] options = ["A", "B", "C"];

        for (int i = 0; i < 50; i++)
        {
            string result = Randomisation.Choose("A", "B", "C");
            Assert.Contains(result, options);
        }
    }


    // ─── UpperCaseString ───

    [TestMethod]
    public void UpperCaseString_ReturnsCorrectLength()
    {
        string result = Randomisation.UpperCaseString(8);
        Assert.AreEqual(8, result.Length);
    }

    [TestMethod]
    public void UpperCaseString_ContainsOnlyUppercaseLetters()
    {
        string result = Randomisation.UpperCaseString(20);
        Assert.IsTrue(result.All(c => c is >= 'A' and <= 'Z'));
    }

    [TestMethod]
    public void UpperCaseString_ZeroChars_ReturnsEmpty()
    {
        string result = Randomisation.UpperCaseString(0);
        Assert.AreEqual(string.Empty, result);
    }

    // ─── Shuffle (array) ───

    [TestMethod]
    public void Shuffle_Array_ReturnsSameElements()
    {
        int[] original = [1, 2, 3, 4, 5];
        int[] input = [1, 2, 3, 4, 5];

        Randomisation.Shuffle(input);

        CollectionAssert.AreEquivalent(original, input);
    }

    [TestMethod]
    public void Shuffle_Array_ReturnsSameReference()
    {
        int[] array = [1, 2, 3];
        int[] result = Randomisation.Shuffle(array);

        Assert.AreSame(array, result);
    }

    // ─── Shuffle (IList extension) ───

    [TestMethod]
    public void Shuffle_IList_ReturnsSameElements()
    {
        List<int> original = [1, 2, 3, 4, 5];
        List<int> input = [1, 2, 3, 4, 5];

        input.Shuffle();

        CollectionAssert.AreEquivalent(original, input);
    }

    [TestMethod]
    public void Shuffle_IList_ReturnsSameReference()
    {
        List<int> list = [1, 2, 3];
        IList<int> result = list.Shuffle();

        Assert.AreSame(list, result);
    }

    // ─── RandomElement ───

    [TestMethod]
    public void RandomElement_ReturnsElementFromList()
    {
        List<string> list = ["X", "Y", "Z"];

        for (int i = 0; i < 50; i++)
        {
            string result = list.RandomElement();
            Assert.Contains(result, list);
        }
    }

    // ─── Colour ───

    [TestMethod]
    public void Colour_AlphaIsMax()
    {
        for (int i = 0; i < 20; i++)
        {
            Colour c = Randomisation.Colour();
            Assert.AreEqual(255, c.A);
        }
    }

    // ─── NamedColour ───

    [TestMethod]
    public void NamedColour_ReturnsOneOfTheNamedStaticColours()
    {
        // Smoke test on the facade — exhaustive coverage of the underlying extension lives in
        // RandomExtensionsTests.NamedColour_ResultIsAlwaysOneOfTheNamedStaticColours.
        for (int i = 0; i < 20; i++)
        {
            Colour result = Randomisation.NamedColour();
            bool matchesAnyNamed = false;
            foreach (System.Reflection.PropertyInfo property in typeof(Colour).GetProperties(
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                if (property.GetValue(null) is Colour named && named.Equals(result))
                {
                    matchesAnyNamed = true;
                    break;
                }
            }
            Assert.IsTrue(matchesAnyNamed);
        }
    }
}

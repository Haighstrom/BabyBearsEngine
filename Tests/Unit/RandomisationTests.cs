using System;
using System.Collections.Generic;
using System.Linq;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RandomisationTests
{
    // Chance

    [TestMethod]
    public void Chance_Zero_AlwaysFalse()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.IsFalse(Randomisation.Chance(0f));
        }
    }

    [TestMethod]
    public void Chance_Hundred_AlwaysTrue()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.IsTrue(Randomisation.Chance(100f));
        }
    }

    // Rand(int max)

    [TestMethod]
    public void Rand_MaxZero_ReturnsZero()
    {
        Assert.AreEqual(0, Randomisation.Rand(0));
    }

    [TestMethod]
    public void Rand_MaxNegative_ReturnsZero()
    {
        Assert.AreEqual(0, Randomisation.Rand(-5));
    }

    [TestMethod]
    public void Rand_Max_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            int result = Randomisation.Rand(10);
            Assert.IsGreaterThanOrEqualTo(0, result);
            Assert.IsLessThan(10, result);
        }
    }

    // Rand(int min, int max)

    [TestMethod]
    public void Rand_MinMax_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            int result = Randomisation.Rand(5, 15);
            Assert.IsGreaterThanOrEqualTo(5, result);
            Assert.IsLessThan(15, result);
        }
    }

    [TestMethod]
    public void Rand_MinEqualsMax_ReturnsThatValue()
    {
        // min == max means range [min, max) which is empty; but per implementation: Next(0) returns 0 → returns min
        int result = Randomisation.Rand(7, 7);
        Assert.AreEqual(7, result);
    }

    [TestMethod]
    public void Rand_MaxLessThanMin_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Randomisation.Rand(10, 5));
    }

    // RandF(float max)

    [TestMethod]
    public void RandF_FloatMax_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            float result = Randomisation.RandF(5f);
            Assert.IsGreaterThanOrEqualTo(0f, result);
            Assert.IsLessThan(5f, result);
        }
    }

    // RandF(float min, float max)

    [TestMethod]
    public void RandF_MinMax_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            float result = Randomisation.RandF(2f, 8f);
            Assert.IsGreaterThanOrEqualTo(2f, result);
            Assert.IsLessThanOrEqualTo(8f, result);
        }
    }

    [TestMethod]
    public void RandF_MaxEqualsMin_ReturnsMax()
    {
        float result = Randomisation.RandF(5f, 5f);
        Assert.AreEqual(5f, result);
    }

    [TestMethod]
    public void RandF_MaxLessThanMin_ReturnsMax()
    {
        float result = Randomisation.RandF(10f, 3f);
        Assert.AreEqual(3f, result);
    }

    // RandF(int min, int max)

    [TestMethod]
    public void RandF_IntMinMax_ResultInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            float result = Randomisation.RandF(1, 9);
            Assert.IsGreaterThanOrEqualTo(1f, result);
            Assert.IsLessThanOrEqualTo(9f, result);
        }
    }

    // RandGaussianApprox

    [TestMethod]
    public void RandGaussianApprox_ResultInRange()
    {
        for (int i = 0; i < 200; i++)
        {
            float result = Randomisation.RandGaussianApprox(0f, 10f);
            Assert.IsGreaterThanOrEqualTo(0f, result);
            Assert.IsLessThanOrEqualTo(10f, result);
        }
    }

    [TestMethod]
    public void RandGaussianApprox_MaxEqualsMin_ReturnsMax()
    {
        float result = Randomisation.RandGaussianApprox(5f, 5f);
        Assert.AreEqual(5f, result);
    }

    // Choose

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

    [TestMethod]
    public void Choose_List_ReturnsElementFromList()
    {
        List<int> options = [10, 20, 30];

        for (int i = 0; i < 50; i++)
        {
            int result = Randomisation.Choose(options);
            Assert.Contains(result, options);
        }
    }

    // RandUpperCaseString

    [TestMethod]
    public void RandUpperCaseString_ReturnsCorrectLength()
    {
        string result = Randomisation.RandUpperCaseString(8);
        Assert.AreEqual(8, result.Length);
    }

    [TestMethod]
    public void RandUpperCaseString_ContainsOnlyUppercaseLetters()
    {
        string result = Randomisation.RandUpperCaseString(20);
        Assert.IsTrue(result.All(c => c is >= 'A' and <= 'Z'));
    }

    [TestMethod]
    public void RandUpperCaseString_ZeroChars_ReturnsEmpty()
    {
        string result = Randomisation.RandUpperCaseString(0);
        Assert.AreEqual(string.Empty, result);
    }

    // Shuffle (array)

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

    // Shuffle (list)

    [TestMethod]
    public void Shuffle_List_ReturnsSameElements()
    {
        List<int> original = [1, 2, 3, 4, 5];
        List<int> input = [1, 2, 3, 4, 5];

        Randomisation.Shuffle(input);

        CollectionAssert.AreEquivalent(original, input);
    }

    // Shuffle (IList extension)

    [TestMethod]
    public void Shuffle_IListExtension_ReturnsSameElements()
    {
        List<int> original = [1, 2, 3, 4, 5];
        List<int> input = [1, 2, 3, 4, 5];

        input.Shuffle();

        CollectionAssert.AreEquivalent(original, input);
    }

    // RandomElement

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

    // RandColour

    [TestMethod]
    public void RandColour_AlphaIsMax()
    {
        for (int i = 0; i < 20; i++)
        {
            Colour c = Randomisation.RandColour();
            Assert.AreEqual(255, c.A);
        }
    }

    // RandNamedColour

    [TestMethod]
    public void RandNamedColour_ReturnsOneOfTheNamedStaticColours()
    {
        // Smoke test on the facade — exhaustive coverage of the underlying extension lives in
        // RandomExtensionsTests.NamedColour_ResultIsAlwaysOneOfTheNamedStaticColours.
        for (int i = 0; i < 20; i++)
        {
            Colour result = Randomisation.RandNamedColour();
            // Discoverable property: at least one of the named static Colour properties equals it.
            // We don't need to compute the full set here — RandomExtensionsTests already does so.
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

using System;
using System.Collections.Generic;
using System.IO;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class EnsureTests
{
    // ArgumentPositive

    [TestMethod]
    public void ArgumentPositive_PositiveValue_DoesNotThrow()
    {
        Ensure.ArgumentPositive(1f, "x");
    }

    [TestMethod]
    public void ArgumentPositive_Zero_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Ensure.ArgumentPositive(0f, "x"));
    }

    [TestMethod]
    public void ArgumentPositive_Negative_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Ensure.ArgumentPositive(-1f, "x"));
    }

    // ArgumentNotNegative

    [TestMethod]
    public void ArgumentNotNegative_PositiveValue_DoesNotThrow()
    {
        Ensure.ArgumentNotNegative(5f, "x");
    }

    [TestMethod]
    public void ArgumentNotNegative_Zero_DoesNotThrow()
    {
        Ensure.ArgumentNotNegative(0f, "x");
    }

    [TestMethod]
    public void ArgumentNotNegative_Negative_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Ensure.ArgumentNotNegative(-0.1f, "x"));
    }

    // ArgumentInRange

    [TestMethod]
    public void ArgumentInRange_AtMin_DoesNotThrow()
    {
        Ensure.ArgumentInRange(0f, "x", minValue: 0f, maxValue: 10f);
    }

    [TestMethod]
    public void ArgumentInRange_InMiddle_DoesNotThrow()
    {
        Ensure.ArgumentInRange(5f, "x", minValue: 0f, maxValue: 10f);
    }

    [TestMethod]
    public void ArgumentInRange_BelowMin_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Ensure.ArgumentInRange(-1f, "x", minValue: 0f, maxValue: 10f));
    }

    [TestMethod]
    public void ArgumentInRange_AtMax_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Ensure.ArgumentInRange(10f, "x", minValue: 0f, maxValue: 10f));
    }

    // ArgumentNotNull

    [TestMethod]
    public void ArgumentNotNull_NonNull_DoesNotThrow()
    {
        Ensure.ArgumentNotNull("hello", "x");
    }

    [TestMethod]
    public void ArgumentNotNull_Null_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => Ensure.ArgumentNotNull(null, "x"));
    }

    // ArgumentNotNullOrEmpty

    [TestMethod]
    public void ArgumentNotNullOrEmpty_NonEmptyString_DoesNotThrow()
    {
        Ensure.ArgumentNotNullOrEmpty("hello", "x");
    }

    [TestMethod]
    public void ArgumentNotNullOrEmpty_EmptyString_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Ensure.ArgumentNotNullOrEmpty(string.Empty, "x"));
    }

    [TestMethod]
    public void ArgumentNotNullOrEmpty_NullString_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Ensure.ArgumentNotNullOrEmpty(null, "x"));
    }

    // ArgumentCollectionNotNullOrEmpty

    [TestMethod]
    public void ArgumentCollectionNotNullOrEmpty_FilledCollection_DoesNotThrow()
    {
        Ensure.ArgumentCollectionNotNullOrEmpty(new[] { 1, 2 }, "x");
    }

    [TestMethod]
    public void ArgumentCollectionNotNullOrEmpty_EmptyCollection_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            Ensure.ArgumentCollectionNotNullOrEmpty(Array.Empty<int>(), "x"));
    }

    [TestMethod]
    public void ArgumentCollectionNotNullOrEmpty_NullCollection_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            Ensure.ArgumentCollectionNotNullOrEmpty<int>(null!, "x"));
    }

    // CollectionNotNullOrEmpty

    [TestMethod]
    public void CollectionNotNullOrEmpty_FilledCollection_DoesNotThrow()
    {
        Ensure.CollectionNotNullOrEmpty(new[] { 1 });
    }

    [TestMethod]
    public void CollectionNotNullOrEmpty_EmptyCollection_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Ensure.CollectionNotNullOrEmpty(Array.Empty<int>()));
    }

    // DictionaryKeyExists

    [TestMethod]
    public void DictionaryKeyExists_ExistingKey_DoesNotThrow()
    {
        Dictionary<string, int> dict = new() { ["a"] = 1 };

        Ensure.DictionaryKeyExists(dict, "a");
    }

    [TestMethod]
    public void DictionaryKeyExists_MissingKey_Throws()
    {
        Dictionary<string, int> dict = new() { ["a"] = 1 };

        Assert.ThrowsExactly<KeyNotFoundException>(() => Ensure.DictionaryKeyExists(dict, "b"));
    }

    // NotNull

    [TestMethod]
    public void NotNull_NonNull_DoesNotThrow()
    {
        Ensure.NotNull(new object());
    }

    [TestMethod]
    public void NotNull_Null_Throws()
    {
        Assert.ThrowsExactly<NullReferenceException>(() => Ensure.NotNull(null));
    }

    // IsInRange (int)

    [TestMethod]
    public void IsInRange_Int_WithinRange_DoesNotThrow()
    {
        Ensure.IsInRange(5, minAllowed: 0, maxAllowed: 10);
    }

    [TestMethod]
    public void IsInRange_Int_AtMin_DoesNotThrow()
    {
        Ensure.IsInRange(0, minAllowed: 0, maxAllowed: 10);
    }

    [TestMethod]
    public void IsInRange_Int_AtMax_DoesNotThrow()
    {
        Ensure.IsInRange(10, minAllowed: 0, maxAllowed: 10);
    }

    [TestMethod]
    public void IsInRange_Int_BelowMin_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Ensure.IsInRange(-1, minAllowed: 0, maxAllowed: 10));
    }

    [TestMethod]
    public void IsInRange_Int_AboveMax_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Ensure.IsInRange(11, minAllowed: 0, maxAllowed: 10));
    }

    // IsInRange (float)

    [TestMethod]
    public void IsInRange_Float_WithinRange_DoesNotThrow()
    {
        Ensure.IsInRange(0.5f, minAllowed: 0f, maxAllowed: 1f);
    }

    [TestMethod]
    public void IsInRange_Float_BelowMin_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Ensure.IsInRange(-0.1f, minAllowed: 0f, maxAllowed: 1f));
    }

    // IsNull

    [TestMethod]
    public void IsNull_Null_DoesNotThrow()
    {
        Ensure.IsNull(null);
    }

    [TestMethod]
    public void IsNull_NonNull_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Ensure.IsNull(new object()));
    }

    // That

    [TestMethod]
    public void That_True_DoesNotThrow()
    {
        Ensure.That(true);
    }

    [TestMethod]
    public void That_False_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Ensure.That(false));
    }

    // FileExists

    [TestMethod]
    public void FileExists_ExistingFile_DoesNotThrow()
    {
        string path = Path.GetTempFileName();
        try
        {
            Ensure.FileExists(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void FileExists_MissingFile_Throws()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() =>
            Ensure.FileExists("C:\\does_not_exist_bbengine_test.txt"));
    }
}

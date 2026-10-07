namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CrockfordBase32Tests
{
    [TestMethod]
    public void Alphabet_Has32DistinctSymbolsWithoutAmbiguousLetters()
    {
        Assert.AreEqual(32, CrockfordBase32.Alphabet.Length);
        Assert.HasCount(32, CrockfordBase32.Alphabet.Distinct());
        Assert.DoesNotContain('I', CrockfordBase32.Alphabet);
        Assert.DoesNotContain('L', CrockfordBase32.Alphabet);
        Assert.DoesNotContain('O', CrockfordBase32.Alphabet);
        Assert.DoesNotContain('U', CrockfordBase32.Alphabet);
    }

    [TestMethod]
    public void Encode_SingleCharacter_MapsValueToSymbol()
    {
        Assert.AreEqual("0", CrockfordBase32.Encode(0, 1));
        Assert.AreEqual("9", CrockfordBase32.Encode(9, 1));
        Assert.AreEqual("A", CrockfordBase32.Encode(10, 1));
        Assert.AreEqual("Z", CrockfordBase32.Encode(31, 1));
    }

    [TestMethod]
    public void Encode_PadsWithLeadingZeros()
    {
        Assert.AreEqual("0001", CrockfordBase32.Encode(1, 4));
    }

    [TestMethod]
    public void Encode_FillsAllBitsOfTheLength()
    {
        Assert.AreEqual("ZZZZ", CrockfordBase32.Encode((1UL << 20) - 1, 4));
    }

    [TestMethod]
    public void Encode_ValueTooLargeForLength_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CrockfordBase32.Encode(1UL << 20, 4));
    }

    [TestMethod]
    public void Encode_LengthOutOfRange_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => CrockfordBase32.Encode(0, 0));
        Assert.ThrowsExactly<InvalidOperationException>(() => CrockfordBase32.Encode(0, CrockfordBase32.MaxLength + 1));
    }

    [TestMethod]
    public void EncodeThenDecode_RoundTrips()
    {
        foreach (ulong value in new ulong[] { 0, 1, 31, 32, 1023, 123456, (1UL << 30) - 1 })
        {
            string code = CrockfordBase32.Encode(value, 6);

            Assert.IsTrue(CrockfordBase32.TryDecode(code, out ulong decoded));
            Assert.AreEqual(value, decoded);
        }
    }

    [TestMethod]
    public void EncodeThenDecode_MaxLength_RoundTrips()
    {
        ulong value = (1UL << 60) - 1;

        string code = CrockfordBase32.Encode(value, CrockfordBase32.MaxLength);

        Assert.IsTrue(CrockfordBase32.TryDecode(code, out ulong decoded));
        Assert.AreEqual(value, decoded);
    }

    [TestMethod]
    public void TryDecode_IgnoresCaseSpacesAndDashes()
    {
        Assert.IsTrue(CrockfordBase32.TryDecode("k7q-2x9", out ulong dashed));
        Assert.IsTrue(CrockfordBase32.TryDecode(" K7Q 2X9 ", out ulong spaced));
        Assert.IsTrue(CrockfordBase32.TryDecode("K7Q2X9", out ulong plain));

        Assert.AreEqual(plain, dashed);
        Assert.AreEqual(plain, spaced);
    }

    [TestMethod]
    public void TryDecode_TreatsOAsZeroAndIAndLAsOne()
    {
        Assert.IsTrue(CrockfordBase32.TryDecode("OIL", out ulong confusable));
        Assert.IsTrue(CrockfordBase32.TryDecode("011", out ulong canonical));

        Assert.AreEqual(canonical, confusable);
    }

    [TestMethod]
    public void TryDecode_CharacterOutsideAlphabet_ReturnsFalse()
    {
        Assert.IsFalse(CrockfordBase32.TryDecode("K7U2", out _));
        Assert.IsFalse(CrockfordBase32.TryDecode("K7!2", out _));
    }

    [TestMethod]
    public void TryDecode_EmptyOrTooLong_ReturnsFalse()
    {
        Assert.IsFalse(CrockfordBase32.TryDecode("", out _));
        Assert.IsFalse(CrockfordBase32.TryDecode("---", out _));
        Assert.IsFalse(CrockfordBase32.TryDecode(new string('1', CrockfordBase32.MaxLength + 1), out _));
    }

    [TestMethod]
    public void Normalise_UpperCasesAndFixesConfusables()
    {
        Assert.AreEqual("K7Q02X9", CrockfordBase32.Normalise("k7q-o2x9"));
        Assert.AreEqual("1111", CrockfordBase32.Normalise("1 i L-1"));
    }
}

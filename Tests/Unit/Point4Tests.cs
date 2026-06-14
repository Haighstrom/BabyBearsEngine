using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class Point4Tests
{
    private const float Delta = 0.0001f;

    // Constructor / properties

    [TestMethod]
    public void Constructor_SetsFloatComponents()
    {
        Point4 p = new(1f, 2f, 3f, 4f);

        Assert.AreEqual(1f, p.X, Delta);
        Assert.AreEqual(2f, p.Y, Delta);
        Assert.AreEqual(3f, p.Z, Delta);
        Assert.AreEqual(4f, p.W, Delta);
    }

    [TestMethod]
    public void Properties_PreserveFloatPrecision()
    {
        Point4 p = new(1.9f, 2.1f, 3.7f, 4.5f);

        Assert.AreEqual(1.9f, p.X, Delta);
        Assert.AreEqual(2.1f, p.Y, Delta);
        Assert.AreEqual(3.7f, p.Z, Delta);
        Assert.AreEqual(4.5f, p.W, Delta);
    }

    [TestMethod]
    public void Properties_AreSettable()
    {
        Point4 p = new(0f, 0f, 0f, 0f);
        p.X = 7.5f;
        p.Y = 8.5f;
        p.Z = 9.5f;
        p.W = 10.5f;

        Assert.AreEqual(7.5f, p.X, Delta);
        Assert.AreEqual(8.5f, p.Y, Delta);
        Assert.AreEqual(9.5f, p.Z, Delta);
        Assert.AreEqual(10.5f, p.W, Delta);
    }

    [TestMethod]
    public void Zero_AllComponentsZero()
    {
        Point4 z = Point4.Zero;

        Assert.AreEqual(0f, z.X, Delta);
        Assert.AreEqual(0f, z.Y, Delta);
        Assert.AreEqual(0f, z.Z, Delta);
        Assert.AreEqual(0f, z.W, Delta);
    }

    // Length / LengthSquared

    [TestMethod]
    public void Length_KnownVector_CorrectMagnitude()
    {
        Point4 p = new(1f, 0f, 0f, 0f);

        Assert.AreEqual(1f, p.Length, Delta);
    }

    [TestMethod]
    public void LengthSquared_KnownVector_CorrectValue()
    {
        Point4 p = new(1f, 2f, 2f, 0f);

        Assert.AreEqual(9f, p.LengthSquared, Delta);
    }

    // Normal

    [TestMethod]
    public void Normal_ReturnsUnitVector()
    {
        Point4 p = new(3f, 0f, 4f, 0f);
        Point4 n = p.Normal;

        Assert.AreEqual(1f, n.Length, Delta);
    }

    [TestMethod]
    public void Normal_ZeroVector_ReturnsZero()
    {
        Point4 n = Point4.Zero.Normal;

        Assert.AreEqual(0f, n.X, Delta);
        Assert.AreEqual(0f, n.Y, Delta);
        Assert.AreEqual(0f, n.Z, Delta);
        Assert.AreEqual(0f, n.W, Delta);
    }

    // Normalize

    [TestMethod]
    public void Normalize_SetsLengthToOne()
    {
        Point4 p = new(3f, 0f, 4f, 0f);
        p.Normalize();

        Assert.AreEqual(1f, p.Length, Delta);
    }

    [TestMethod]
    public void Normalize_ZeroVector_RemainsZero()
    {
        Point4 p = Point4.Zero;
        p.Normalize();

        Assert.AreEqual(0f, p.X, Delta);
        Assert.AreEqual(0f, p.Y, Delta);
    }

    // Clamp

    [TestMethod]
    public void Clamp_ExceedsMax_ClampsLength()
    {
        Point4 p = new(3f, 0f, 4f, 0f);
        Point4 result = p.Clamp(2.5f);

        Assert.AreEqual(2.5f, result.Length, Delta);
    }

    [TestMethod]
    public void Clamp_WithinMax_Unchanged()
    {
        Point4 p = new(1f, 0f, 0f, 0f);
        Point4 result = p.Clamp(5f);

        Assert.AreEqual(1f, result.Length, Delta);
    }

    // DotProduct

    [TestMethod]
    public void DotProduct_Static_CorrectValue()
    {
        Point4 a = new(1f, 2f, 3f, 4f);
        Point4 b = new(5f, 6f, 7f, 8f);

        Assert.AreEqual(70f, Point4.DotProduct(a, b), Delta);
    }

    [TestMethod]
    public void DotProduct_OrthogonalVectors_ReturnsZero()
    {
        Point4 a = new(1f, 0f, 0f, 0f);
        Point4 b = new(0f, 1f, 0f, 0f);

        Assert.AreEqual(0f, a.DotProduct(b), Delta);
    }

    // Transform

    [TestMethod]
    public void Transform_ByIdentity_Unchanged()
    {
        Matrix4 identity = Matrix4.Identity;
        Point4 p = new(1f, 2f, 3f, 1f);
        Point4 result = p.Transform(identity);

        Assert.AreEqual(1f, result.X, Delta);
        Assert.AreEqual(2f, result.Y, Delta);
        Assert.AreEqual(3f, result.Z, Delta);
    }

    [TestMethod]
    public void Transform_Ref_ByIdentity_Unchanged()
    {
        Matrix4 identity = Matrix4.Identity;
        Point4 p = new(1f, 2f, 3f, 1f);
        Point4 result = p.Transform(ref identity);

        Assert.AreEqual(1f, result.X, Delta);
        Assert.AreEqual(2f, result.Y, Delta);
        Assert.AreEqual(3f, result.Z, Delta);
    }

    // ToPoint

    [TestMethod]
    public void ToPoint_ReturnsXY()
    {
        Point4 p = new(3f, 7f, 9f, 1f);
        var result = p.ToPoint();

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(7f, result.Y, Delta);
    }

    // ToPoint3

    [TestMethod]
    public void ToPoint3_ReturnsXYZ()
    {
        Point4 p = new(1f, 2f, 3f, 4f);
        var result = p.ToPoint3();

        Assert.AreEqual(1f, result.X, Delta);
        Assert.AreEqual(2f, result.Y, Delta);
        Assert.AreEqual(3f, result.Z, Delta);
    }

    // ToArray

    [TestMethod]
    public void ToArray_ReturnsXYZWInOrder()
    {
        Point4 p = new(5f, 6f, 7f, 8f);
        float[] arr = p.ToArray();

        Assert.AreEqual(4, arr.Length);
        Assert.AreEqual(5f, arr[0], Delta);
        Assert.AreEqual(6f, arr[1], Delta);
        Assert.AreEqual(7f, arr[2], Delta);
        Assert.AreEqual(8f, arr[3], Delta);
    }

    // Equals

    [TestMethod]
    public void Equals_SameValues_ReturnsTrue()
    {
        Point4 a = new(1f, 2f, 3f, 4f);
        Point4 b = new(1f, 2f, 3f, 4f);

        Assert.IsTrue(a.Equals(b));
    }

    [TestMethod]
    public void GetHashCode_EqualPoints_SameHash()
    {
        Point4 a = new(1f, 2f, 3f, 4f);
        Point4 b = new(1f, 2f, 3f, 4f);

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equals_DifferentW_ReturnsFalse()
    {
        Point4 a = new(1f, 2f, 3f, 4f);
        Point4 b = new(1f, 2f, 3f, 0f);

        Assert.IsFalse(a.Equals(b));
    }

    // Operators

    [TestMethod]
    public void OperatorAdd_ComponentWise()
    {
        Point4 result = new Point4(1f, 2f, 3f, 4f) + new Point4(5f, 6f, 7f, 8f);

        Assert.AreEqual(6f, result.X, Delta);
        Assert.AreEqual(8f, result.Y, Delta);
        Assert.AreEqual(10f, result.Z, Delta);
        Assert.AreEqual(12f, result.W, Delta);
    }

    [TestMethod]
    public void OperatorSubtract_ComponentWise()
    {
        Point4 result = new Point4(5f, 6f, 7f, 8f) - new Point4(1f, 2f, 3f, 4f);

        Assert.AreEqual(4f, result.X, Delta);
        Assert.AreEqual(4f, result.Y, Delta);
        Assert.AreEqual(4f, result.Z, Delta);
        Assert.AreEqual(4f, result.W, Delta);
    }

    [TestMethod]
    public void OperatorMultiplyScalar_ScalesComponents()
    {
        Point4 result = new Point4(1f, 2f, 3f, 4f) * 2f;

        Assert.AreEqual(2f, result.X, Delta);
        Assert.AreEqual(4f, result.Y, Delta);
        Assert.AreEqual(6f, result.Z, Delta);
        Assert.AreEqual(8f, result.W, Delta);
    }

    [TestMethod]
    public void OperatorDivide_DividesComponents()
    {
        Point4 result = new Point4(4f, 6f, 8f, 10f) / 2f;

        Assert.AreEqual(2f, result.X, Delta);
        Assert.AreEqual(3f, result.Y, Delta);
        Assert.AreEqual(4f, result.Z, Delta);
        Assert.AreEqual(5f, result.W, Delta);
    }

    [TestMethod]
    public void OperatorEquals_SameValues_True()
    {
        Assert.IsTrue(new Point4(1f, 2f, 3f, 4f) == new Point4(1f, 2f, 3f, 4f));
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentValues_True()
    {
        Assert.IsTrue(new Point4(1f, 2f, 3f, 4f) != new Point4(1f, 2f, 3f, 0f));
    }

    // ToString

    [TestMethod]
    public void ToString_ContainsComponents()
    {
        string s = new Point4(1f, 2f, 3f, 4f).ToString();

        Assert.IsTrue(s.Contains('1'));
        Assert.IsTrue(s.Contains('2'));
        Assert.IsTrue(s.Contains('3'));
        Assert.IsTrue(s.Contains('4'));
    }
}

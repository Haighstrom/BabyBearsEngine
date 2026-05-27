using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class Point3Tests
{
    private const float Delta = 0.0001f;

    // Constructor / properties

    [TestMethod]
    public void Constructor_SetsComponents()
    {
        Point3 p = new(1f, 2f, 3f);

        Assert.AreEqual(1f, p.X, Delta);
        Assert.AreEqual(2f, p.Y, Delta);
        Assert.AreEqual(3f, p.Z, Delta);
    }

    [TestMethod]
    public void Zero_AllComponentsZero()
    {
        Assert.AreEqual(0f, Point3.Zero.X, Delta);
        Assert.AreEqual(0f, Point3.Zero.Y, Delta);
        Assert.AreEqual(0f, Point3.Zero.Z, Delta);
    }

    // Length / LengthSquared

    [TestMethod]
    public void Length_KnownVector_CorrectMagnitude()
    {
        Point3 p = new(3f, 0f, 4f);

        Assert.AreEqual(5f, p.Length, Delta);
    }

    [TestMethod]
    public void LengthSquared_KnownVector_CorrectValue()
    {
        Point3 p = new(1f, 2f, 2f);

        Assert.AreEqual(9f, p.LengthSquared, Delta);
    }

    // Normal

    [TestMethod]
    public void Normal_ReturnsUnitVector()
    {
        Point3 p = new(3f, 0f, 4f);
        Point3 n = p.Normal;

        Assert.AreEqual(1f, n.Length, Delta);
    }

    [TestMethod]
    public void Normal_ZeroVector_ReturnsZero()
    {
        Point3 n = Point3.Zero.Normal;

        Assert.AreEqual(0f, n.X, Delta);
        Assert.AreEqual(0f, n.Y, Delta);
        Assert.AreEqual(0f, n.Z, Delta);
    }

    // Normalize

    [TestMethod]
    public void Normalize_SetsLengthToOne()
    {
        Point3 p = new(3f, 0f, 4f);
        p.Normalize();

        Assert.AreEqual(1f, p.Length, Delta);
    }

    [TestMethod]
    public void Normalize_ZeroVector_RemainsZero()
    {
        Point3 p = Point3.Zero;
        p.Normalize();

        Assert.AreEqual(0f, p.X, Delta);
        Assert.AreEqual(0f, p.Y, Delta);
        Assert.AreEqual(0f, p.Z, Delta);
    }

    // Clamp

    [TestMethod]
    public void Clamp_VectorExceedsMax_ClampsLength()
    {
        Point3 p = new(3f, 0f, 4f);
        p.Clamp(2.5f);

        Assert.AreEqual(2.5f, p.Length, Delta);
    }

    [TestMethod]
    public void Clamp_VectorWithinMax_Unchanged()
    {
        Point3 p = new(1f, 0f, 0f);
        p.Clamp(5f);

        Assert.AreEqual(1f, p.Length, Delta);
    }

    // DotProduct

    [TestMethod]
    public void DotProduct_Static_CorrectValue()
    {
        Point3 a = new(1f, 2f, 3f);
        Point3 b = new(4f, 5f, 6f);

        Assert.AreEqual(32f, Point3.DotProduct(a, b), Delta);
    }

    [TestMethod]
    public void DotProduct_Instance_MatchesStatic()
    {
        Point3 a = new(1f, 0f, 0f);
        Point3 b = new(0f, 1f, 0f);

        Assert.AreEqual(0f, a.DotProduct(b), Delta);
    }

    // CrossProduct

    [TestMethod]
    public void CrossProduct_XCrossY_GivesZ()
    {
        Point3 x = new(1f, 0f, 0f);
        Point3 y = new(0f, 1f, 0f);
        var result = Point3.CrossProduct(x, y);

        Assert.AreEqual(0f, result.X, Delta);
        Assert.AreEqual(0f, result.Y, Delta);
        Assert.AreEqual(1f, result.Z, Delta);
    }

    [TestMethod]
    public void CrossProduct_Instance_MatchesStatic()
    {
        Point3 a = new(1f, 0f, 0f);
        Point3 b = new(0f, 1f, 0f);
        Point3 instance = a.CrossProduct(b);
        var stat = Point3.CrossProduct(a, b);

        Assert.AreEqual(stat.X, instance.X, Delta);
        Assert.AreEqual(stat.Y, instance.Y, Delta);
        Assert.AreEqual(stat.Z, instance.Z, Delta);
    }

    // ToPoint

    [TestMethod]
    public void ToPoint_ReturnsXY()
    {
        Point3 p = new(3f, 7f, 9f);
        var result = p.ToPoint();

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(7f, result.Y, Delta);
    }

    // ToPoint4

    [TestMethod]
    public void ToPoint4_SetsWToOne()
    {
        Point3 p = new(1f, 2f, 3f);
        var result = p.ToPoint4();

        Assert.AreEqual(1f, result.x, Delta);
        Assert.AreEqual(2f, result.y, Delta);
        Assert.AreEqual(3f, result.z, Delta);
        Assert.AreEqual(1f, result.w, Delta);
    }

    // ToArray

    [TestMethod]
    public void ToArray_ReturnsXYZInOrder()
    {
        Point3 p = new(5f, 6f, 7f);
        float[] arr = p.ToArray();

        Assert.AreEqual(3, arr.Length);
        Assert.AreEqual(5f, arr[0], Delta);
        Assert.AreEqual(6f, arr[1], Delta);
        Assert.AreEqual(7f, arr[2], Delta);
    }

    // Equals

    [TestMethod]
    public void Equals_SameValues_ReturnsTrue()
    {
        Point3 a = new(1f, 2f, 3f);
        Point3 b = new(1f, 2f, 3f);

        Assert.IsTrue(a.Equals(b));
    }

    [TestMethod]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        Point3 a = new(1f, 2f, 3f);
        Point3 b = new(1f, 2f, 4f);

        Assert.IsFalse(a.Equals(b));
    }

    // Operators

    [TestMethod]
    public void OperatorAdd_ComponentWise()
    {
        Point3 result = new Point3(1f, 2f, 3f) + new Point3(4f, 5f, 6f);

        Assert.AreEqual(5f, result.X, Delta);
        Assert.AreEqual(7f, result.Y, Delta);
        Assert.AreEqual(9f, result.Z, Delta);
    }

    [TestMethod]
    public void OperatorSubtract_ComponentWise()
    {
        Point3 result = new Point3(4f, 5f, 6f) - new Point3(1f, 2f, 3f);

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(3f, result.Y, Delta);
        Assert.AreEqual(3f, result.Z, Delta);
    }

    [TestMethod]
    public void OperatorMultiply_ScalesComponents()
    {
        Point3 result = new Point3(1f, 2f, 3f) * 3f;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(6f, result.Y, Delta);
        Assert.AreEqual(9f, result.Z, Delta);
    }

    [TestMethod]
    public void OperatorDivide_DividesComponents()
    {
        Point3 result = new Point3(6f, 9f, 12f) / 3f;

        Assert.AreEqual(2f, result.X, Delta);
        Assert.AreEqual(3f, result.Y, Delta);
        Assert.AreEqual(4f, result.Z, Delta);
    }

    [TestMethod]
    public void OperatorEquals_SameValues_True()
    {
        Assert.IsTrue(new Point3(1f, 2f, 3f) == new Point3(1f, 2f, 3f));
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentValues_True()
    {
        Assert.IsTrue(new Point3(1f, 2f, 3f) != new Point3(1f, 2f, 4f));
    }

    // GetHashCode

    [TestMethod]
    public void GetHashCode_EqualPoints_SameHash()
    {
        Point3 a = new(1f, 2f, 3f);
        Point3 b = new(1f, 2f, 3f);

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    // ToString

    [TestMethod]
    public void ToString_ContainsComponents()
    {
        string s = new Point3(1f, 2f, 3f).ToString();

        Assert.IsTrue(s.Contains('1'));
        Assert.IsTrue(s.Contains('2'));
        Assert.IsTrue(s.Contains('3'));
    }
}

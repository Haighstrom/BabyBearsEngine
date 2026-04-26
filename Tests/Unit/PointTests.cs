using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PointTests
{
    private const double Delta = 1e-4;

    // Zero

    [TestMethod]
    public void Zero_HasXAndYEqualToZero()
    {
        var zero = Point.Zero;
        Assert.AreEqual(0f, zero.X);
        Assert.AreEqual(0f, zero.Y);
    }

    // Constructor

    [TestMethod]
    public void Constructor_SetsXAndY()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(3f, p.X);
        Assert.AreEqual(4f, p.Y);
    }

    // LengthSquared

    [TestMethod]
    public void LengthSquared_ReturnsXSquaredPlusYSquared()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(25.0, p.LengthSquared, Delta);
    }

    [TestMethod]
    public void LengthSquared_OfZeroVector_IsZero()
    {
        Assert.AreEqual(0.0, Point.Zero.LengthSquared, Delta);
    }

    // Length

    [TestMethod]
    public void Length_ReturnsMagnitudeOfVector()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(5.0, p.Length, Delta);
    }

    [TestMethod]
    public void Length_OfUnitAxisVector_IsOne()
    {
        Assert.AreEqual(1.0, new Point(1f, 0f).Length, Delta);
        Assert.AreEqual(1.0, new Point(0f, 1f).Length, Delta);
    }

    // Normal

    [TestMethod]
    public void Normal_ReturnsUnitVector()
    {
        Point p = new(3f, 4f);
        var normal = p.Normal;
        Assert.AreEqual(0.6, normal.X, Delta);
        Assert.AreEqual(0.8, normal.Y, Delta);
    }

    [TestMethod]
    public void Normal_HasLengthOfOne()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(1.0, p.Normal.Length, Delta);
    }

    [TestMethod]
    public void Normal_OfZeroVector_ReturnsZero()
    {
        var normal = Point.Zero.Normal;
        Assert.AreEqual(0f, normal.X);
        Assert.AreEqual(0f, normal.Y);
    }

    // Perpendicular

    [TestMethod]
    public void Perpendicular_NegatecYAndSwapsComponents()
    {
        Point p = new(3f, 4f);
        var perp = p.Perpendicular;
        Assert.AreEqual(-4f, perp.X);
        Assert.AreEqual(3f, perp.Y);
    }

    [TestMethod]
    public void Perpendicular_IsOrthogonalToOriginal()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(0.0, p.DotProduct(p.Perpendicular), Delta);
    }

    [TestMethod]
    public void Perpendicular_HasSameLengthAsOriginal()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(p.Length, p.Perpendicular.Length, Delta);
    }

    // DotProduct

    [TestMethod]
    public void DotProduct_OfPerpendicularVectors_IsZero()
    {
        Assert.AreEqual(0.0, new Point(1f, 0f).DotProduct(new Point(0f, 1f)), Delta);
    }

    [TestMethod]
    public void DotProduct_OfVectorWithItself_EqualsLengthSquared()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(p.LengthSquared, p.DotProduct(p), Delta);
    }

    [TestMethod]
    public void DotProduct_ReturnsCorrectScalarValue()
    {
        Point a = new(1f, 2f);
        Point b = new(3f, 4f);
        Assert.AreEqual(11.0, a.DotProduct(b), Delta);
    }

    // Clamp

    [TestMethod]
    public void Clamp_WhenVectorShorterThanMin_ScalesUpToMinLength()
    {
        Point p = new(3f, 4f); // length = 5
        var clamped = p.Clamp(10f, 20f);
        Assert.AreEqual(10.0, clamped.Length, Delta);
    }

    [TestMethod]
    public void Clamp_WhenVectorLongerThanMax_ScalesDownToMaxLength()
    {
        Point p = new(3f, 4f); // length = 5
        var clamped = p.Clamp(1f, 3f);
        Assert.AreEqual(3.0, clamped.Length, Delta);
    }

    [TestMethod]
    public void Clamp_WhenVectorWithinRange_IsUnchanged()
    {
        Point p = new(3f, 4f); // length = 5
        var clamped = p.Clamp(3f, 7f);
        Assert.AreEqual(p.X, clamped.X, Delta);
        Assert.AreEqual(p.Y, clamped.Y, Delta);
    }

    [TestMethod]
    public void Clamp_PreservesDirection()
    {
        Point p = new(3f, 4f);
        var clamped = p.Clamp(10f, 20f);
        Assert.AreEqual(p.Normal.X, clamped.Normal.X, Delta);
        Assert.AreEqual(p.Normal.Y, clamped.Normal.Y, Delta);
    }

    // Rotate

    [TestMethod]
    public void Rotate_90DegreesAroundOrigin_RotatesCorrectly()
    {
        Point p = new(1f, 0f);
        var rotated = p.Rotate(90f);
        Assert.AreEqual(0.0, rotated.X, Delta);
        Assert.AreEqual(1.0, rotated.Y, Delta);
    }

    [TestMethod]
    public void Rotate_180DegreesAroundOrigin_NegatatesVector()
    {
        Point p = new(1f, 0f);
        var rotated = p.Rotate(180f);
        Assert.AreEqual(-1.0, rotated.X, Delta);
        Assert.AreEqual(0.0, rotated.Y, Delta);
    }

    [TestMethod]
    public void Rotate_360Degrees_ReturnsToOriginalPosition()
    {
        Point p = new(3f, 4f);
        var rotated = p.Rotate(360f);
        Assert.AreEqual(p.X, rotated.X, Delta);
        Assert.AreEqual(p.Y, rotated.Y, Delta);
    }

    [TestMethod]
    public void Rotate_90DegreesAroundCentre_RotatesCorrectly()
    {
        Point p = new(2f, 1f);
        Point centre = new(1f, 1f);
        var rotated = p.Rotate(90f, centre);
        Assert.AreEqual(1.0, rotated.X, Delta);
        Assert.AreEqual(2.0, rotated.Y, Delta);
    }

    [TestMethod]
    public void Rotate_PreservesDistanceToCentre()
    {
        Point p = new(3f, 4f);
        Point centre = new(1f, 2f);
        float originalDist = (p - centre).Length;
        float rotatedDist = (p.Rotate(45f, centre) - centre).Length;
        Assert.AreEqual(originalDist, rotatedDist, Delta);
    }

    // Scale

    [TestMethod]
    public void Scale_NonUniform_ScalesXAndYByDifferentFactors()
    {
        Point p = new(2f, 3f);
        var scaled = p.Scale(4f, 5f);
        Assert.AreEqual(8.0, scaled.X, Delta);
        Assert.AreEqual(15.0, scaled.Y, Delta);
    }

    [TestMethod]
    public void Scale_Uniform_ScalesBothComponentsByTheSameFactor()
    {
        Point p = new(2f, 3f);
        var scaled = p.Scale(3f);
        Assert.AreEqual(6.0, scaled.X, Delta);
        Assert.AreEqual(9.0, scaled.Y, Delta);
    }

    [TestMethod]
    public void Scale_ByZero_ReturnsZeroVector()
    {
        var scaled = new Point(2f, 3f).Scale(0f);
        Assert.AreEqual(0.0, scaled.X, Delta);
        Assert.AreEqual(0.0, scaled.Y, Delta);
    }

    [TestMethod]
    public void Scale_DoesNotMutateOriginal()
    {
        Point p = new(2f, 3f);
        _ = p.Scale(10f);
        Assert.AreEqual(2f, p.X);
        Assert.AreEqual(3f, p.Y);
    }

    // Shift

    [TestMethod]
    public void Shift_MovesPointByGivenOffsets()
    {
        var shifted = new Point(1f, 2f).Shift(3f, 4f);
        Assert.AreEqual(4f, shifted.X);
        Assert.AreEqual(6f, shifted.Y);
    }

    [TestMethod]
    public void Shift_WithNegativeOffsets_MovesInNegativeDirection()
    {
        var shifted = new Point(5f, 5f).Shift(-2f, -3f);
        Assert.AreEqual(3f, shifted.X);
        Assert.AreEqual(2f, shifted.Y);
    }

    [TestMethod]
    public void Shift_ByZero_IsUnchanged()
    {
        Point p = new(3f, 4f);
        var shifted = p.Shift(0f, 0f);
        Assert.AreEqual(p.X, shifted.X);
        Assert.AreEqual(p.Y, shifted.Y);
    }

    // ToRect

    [TestMethod]
    public void ToRect_ReturnsRectWithWidthAndHeightMatchingXAndY()
    {
        Rect rect = new Point(6f, 4f).ToRect();
        Assert.AreEqual(6f, rect.W);
        Assert.AreEqual(4f, rect.H);
    }

    [TestMethod]
    public void ToRect_RectOriginIsAtZero()
    {
        Rect rect = new Point(6f, 4f).ToRect();
        Assert.AreEqual(0f, rect.X);
        Assert.AreEqual(0f, rect.Y);
    }

    // Equals(IPosition)

    [TestMethod]
    public void EqualsIPosition_WithMatchingCoordinates_ReturnsTrue()
    {
        Point a = new(1f, 2f);
        Point b = new(1f, 2f);
        Assert.IsTrue(a.Equals((IPosition)b));
    }

    [TestMethod]
    public void EqualsIPosition_WithDifferentCoordinates_ReturnsFalse()
    {
        Point a = new(1f, 2f);
        Point b = new(3f, 4f);
        Assert.IsFalse(a.Equals((IPosition)b));
    }

    [TestMethod]
    public void EqualsIPosition_WithNull_ReturnsFalse()
    {
        Assert.IsFalse(new Point(1f, 2f).Equals((IPosition?)null));
    }

    // Operators

    [TestMethod]
    public void AdditionOperator_SumsComponents()
    {
        var result = new Point(1f, 2f) + new Point(3f, 4f);
        Assert.AreEqual(4f, result.X);
        Assert.AreEqual(6f, result.Y);
    }

    [TestMethod]
    public void SubtractionOperator_SubtractsComponents()
    {
        var result = new Point(5f, 7f) - new Point(2f, 3f);
        Assert.AreEqual(3f, result.X);
        Assert.AreEqual(4f, result.Y);
    }

    [TestMethod]
    public void MultiplicationOperator_PointTimesFloat_ScalesComponents()
    {
        var result = new Point(2f, 3f) * 4f;
        Assert.AreEqual(8f, result.X);
        Assert.AreEqual(12f, result.Y);
    }

    [TestMethod]
    public void MultiplicationOperator_FloatTimesPoint_ScalesComponents()
    {
        var result = 4f * new Point(2f, 3f);
        Assert.AreEqual(8f, result.X);
        Assert.AreEqual(12f, result.Y);
    }

    [TestMethod]
    public void MultiplicationOperator_BothOrdersProduceSameResult()
    {
        Point p = new(2f, 3f);
        Assert.AreEqual(p * 4f, 4f * p);
    }

    [TestMethod]
    public void DivisionOperator_DividesComponents()
    {
        var result = new Point(8f, 12f) / 4f;
        Assert.AreEqual(2f, result.X);
        Assert.AreEqual(3f, result.Y);
    }

    [TestMethod]
    public void UnaryNegationOperator_NegatesBothComponents()
    {
        var result = -new Point(3f, -4f);
        Assert.AreEqual(-3f, result.X);
        Assert.AreEqual(4f, result.Y);
    }

    [TestMethod]
    public void UnaryNegation_TwiceReturnsOriginal()
    {
        Point p = new(3f, 4f);
        Assert.AreEqual(p, -(-p));
    }

    // ToString

    [TestMethod]
    public void ToString_ReturnsExpectedFormat()
    {
        Assert.AreEqual("(X:1,Y:2)", new Point(1f, 2f).ToString());
    }
}

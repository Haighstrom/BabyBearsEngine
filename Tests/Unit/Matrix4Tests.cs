using System;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class Matrix4Tests
{
    private const float Delta = 0.0001f;

    // Identity / Zero

    [TestMethod]
    public void Identity_DiagonalOnesOffDiagonalZeros()
    {
        Matrix4 m = Matrix4.Identity;

        for (int col = 0; col < 4; col++)
        {
            for (int row = 0; row < 4; row++)
            {
                float expected = col == row ? 1f : 0f;
                Assert.AreEqual(expected, m[col, row], Delta);
            }
        }
    }

    [TestMethod]
    public void Zero_AllElementsZero()
    {
        Matrix4 m = Matrix4.Zero;
        foreach (float f in m.Values)
        {
            Assert.AreEqual(0f, f, Delta);
        }
    }

    // FlipXYZ properties — no aliasing

    [TestMethod]
    public void FlipXMatrix_ReturnsIndependentInstances()
    {
        Matrix4 a = Matrix4.FlipXMatrix;
        Matrix4 b = Matrix4.FlipXMatrix;
        a.Values[0] = 99f;

        Assert.AreEqual(-1f, b.Values[0], Delta);
    }

    [TestMethod]
    public void FlipYMatrix_ReturnsIndependentInstances()
    {
        Matrix4 a = Matrix4.FlipYMatrix;
        Matrix4 b = Matrix4.FlipYMatrix;
        a.Values[5] = 99f;

        Assert.AreEqual(-1f, b.Values[5], Delta);
    }

    [TestMethod]
    public void FlipZMatrix_ReturnsIndependentInstances()
    {
        Matrix4 a = Matrix4.FlipZMatrix;
        Matrix4 b = Matrix4.FlipZMatrix;
        a.Values[10] = 99f;

        Assert.AreEqual(-1f, b.Values[10], Delta);
    }

    [TestMethod]
    public void FlipXMatrix_NegatesXAxis()
    {
        Matrix4 flip = Matrix4.FlipXMatrix;
        Point4 p = new(3f, 4f, 5f, 1f);
        Point4 result = flip * p;

        Assert.AreEqual(-3f, result.X, Delta);
        Assert.AreEqual(4f, result.Y, Delta);
        Assert.AreEqual(5f, result.Z, Delta);
    }

    [TestMethod]
    public void FlipYMatrix_NegatesYAxis()
    {
        Matrix4 flip = Matrix4.FlipYMatrix;
        Point4 p = new(3f, 4f, 5f, 1f);
        Point4 result = flip * p;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(-4f, result.Y, Delta);
        Assert.AreEqual(5f, result.Z, Delta);
    }

    [TestMethod]
    public void FlipZMatrix_NegatesZAxis()
    {
        Matrix4 flip = Matrix4.FlipZMatrix;
        Point4 p = new(3f, 4f, 5f, 1f);
        Point4 result = flip * p;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(4f, result.Y, Delta);
        Assert.AreEqual(-5f, result.Z, Delta);
    }

    // Copy constructor — no aliasing

    [TestMethod]
    public void CopyConstructor_MutatingCopyDoesNotAffectOriginal()
    {
        Matrix4 original = Matrix4.Identity;
        Matrix4 copy = new(original);
        copy.Values[0] = 99f;

        Assert.AreEqual(1f, original.Values[0], Delta);
    }

    // CreateTranslation

    [TestMethod]
    public void CreateTranslation_XYZ_TranslatesPoint()
    {
        var t = Matrix4.CreateTranslation(3f, 5f, 7f);
        Point4 p = new(0f, 0f, 0f, 1f);
        Point4 result = t * p;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(5f, result.Y, Delta);
        Assert.AreEqual(7f, result.Z, Delta);
    }

    [TestMethod]
    public void CreateTranslation_XY_TranslatesPoint()
    {
        var t = Matrix4.CreateTranslation(2f, 4f);
        Point p = new(0f, 0f);
        Point result = t * p;

        Assert.AreEqual(2f, result.X, Delta);
        Assert.AreEqual(4f, result.Y, Delta);
    }

    // CreateRotationAroundZAxis

    [TestMethod]
    public void CreateRotationAroundZAxis_ZeroDegrees_ReturnsIdentity()
    {
        var m = Matrix4.CreateRotationAroundZAxis(0f);

        Assert.AreEqual(1f, m[0, 0], Delta);
        Assert.AreEqual(0f, m[0, 1], Delta);
        Assert.AreEqual(0f, m[1, 0], Delta);
        Assert.AreEqual(1f, m[1, 1], Delta);
    }

    [TestMethod]
    public void CreateRotationAroundZAxis_NinetyDegrees_RotatesXIntoY()
    {
        var m = Matrix4.CreateRotationAroundZAxis(90f);
        Point4 p = new(1f, 0f, 0f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(0f, result.X, Delta);
        Assert.AreEqual(1f, result.Y, Delta);
        Assert.AreEqual(0f, result.Z, Delta);
    }

    // CreateScale

    [TestMethod]
    public void CreateScale_ScalesDiagonal()
    {
        var m = Matrix4.CreateScale(2f, 3f, 4f);

        Assert.AreEqual(2f, m[0, 0], Delta);
        Assert.AreEqual(3f, m[1, 1], Delta);
        Assert.AreEqual(4f, m[2, 2], Delta);
        Assert.AreEqual(1f, m[3, 3], Delta);
    }

    // Add / Subtract

    [TestMethod]
    public void Add_ComponentWise()
    {
        Matrix4 a = Matrix4.Identity;
        Matrix4 b = Matrix4.Identity;
        Matrix4 result = a + b;

        Assert.AreEqual(2f, result[0, 0], Delta);
        Assert.AreEqual(2f, result[1, 1], Delta);
        Assert.AreEqual(0f, result[0, 1], Delta);
    }

    [TestMethod]
    public void Subtract_ComponentWise()
    {
        Matrix4 a = Matrix4.Identity;
        Matrix4 result = a - a;

        foreach (float f in result.Values)
        {
            Assert.AreEqual(0f, f, Delta);
        }
    }

    // Multiply (matrix × matrix)

    [TestMethod]
    public void Multiply_ByIdentity_Unchanged()
    {
        var m = Matrix4.CreateScale(2f, 3f, 4f);
        Matrix4 result = m * Matrix4.Identity;

        Assert.AreEqual(m[0, 0], result[0, 0], Delta);
        Assert.AreEqual(m[1, 1], result[1, 1], Delta);
        Assert.AreEqual(m[2, 2], result[2, 2], Delta);
    }

    // Multiply (scalar)

    [TestMethod]
    public void Multiply_Scalar_ScalesAllElements()
    {
        Matrix4 m = Matrix4.Identity;
        Matrix4 result = m * 5f;

        Assert.AreEqual(5f, result[0, 0], Delta);
        Assert.AreEqual(5f, result[1, 1], Delta);
        Assert.AreEqual(0f, result[0, 1], Delta);
    }

    // Multiply (Point)

    [TestMethod]
    public void Multiply_Point_AppliesTranslation()
    {
        var t = Matrix4.CreateTranslation(10f, 20f);
        Point p = new(1f, 2f);
        Point result = t * p;

        Assert.AreEqual(11f, result.X, Delta);
        Assert.AreEqual(22f, result.Y, Delta);
    }

    // Multiply (Point4)

    [TestMethod]
    public void Multiply_Point4_TransformsHomogeneous()
    {
        Matrix4 m = Matrix4.Identity;
        Point4 p = new(3f, 5f, 7f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(5f, result.Y, Delta);
        Assert.AreEqual(7f, result.Z, Delta);
        Assert.AreEqual(1f, result.W, Delta);
    }

    // Translate

    [TestMethod]
    public void Translate_ComposesTranslation()
    {
        Matrix4 m = Matrix4.Identity;
        m = Matrix4.Translate(ref m, 5f, 3f, 1f);
        Point4 p = new(0f, 0f, 0f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(5f, result.X, Delta);
        Assert.AreEqual(3f, result.Y, Delta);
        Assert.AreEqual(1f, result.Z, Delta);
    }

    // RotateAroundZ

    [TestMethod]
    public void RotateAroundZ_NinetyDegrees_RotatesX()
    {
        Matrix4 m = Matrix4.Identity;
        m = Matrix4.RotateAroundZ(ref m, 90f);
        Point4 p = new(1f, 0f, 0f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(0f, result.X, Delta);
        Assert.AreEqual(1f, result.Y, Delta);
    }

    // ScaleAroundPoint

    [TestMethod]
    public void ScaleAroundPoint_PreservesZ()
    {
        Matrix4 m = Matrix4.Identity;
        m = Matrix4.ScaleAroundPoint(ref m, scaleX: 2f, scaleY: 3f, x: 10f, y: 20f);
        Point4 p = new(10f, 20f, 5f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(5f, result.Z, Delta);
    }

    [TestMethod]
    public void ScaleAroundPoint_ScalesXAndYAroundCentre()
    {
        Matrix4 m = Matrix4.Identity;
        m = Matrix4.ScaleAroundPoint(ref m, scaleX: 2f, scaleY: 3f, x: 10f, y: 20f);
        Point4 centre = new(10f, 20f, 0f, 1f);
        Point4 offset = new(11f, 21f, 0f, 1f);

        Point4 centreResult = m * centre;
        Point4 offsetResult = m * offset;

        Assert.AreEqual(10f, centreResult.X, Delta);
        Assert.AreEqual(20f, centreResult.Y, Delta);
        Assert.AreEqual(12f, offsetResult.X, Delta);
        Assert.AreEqual(23f, offsetResult.Y, Delta);
    }

    // FlipX / FlipY / FlipZ methods

    [TestMethod]
    public void FlipX_NegatesXOfPoint()
    {
        Matrix4 m = Matrix4.Identity;
        m = Matrix4.FlipX(ref m);
        Point4 p = new(3f, 4f, 5f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(-3f, result.X, Delta);
        Assert.AreEqual(4f, result.Y, Delta);
    }

    [TestMethod]
    public void FlipY_NegatesYOfPoint()
    {
        Matrix4 m = Matrix4.Identity;
        m = Matrix4.FlipY(ref m);
        Point4 p = new(3f, 4f, 5f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(-4f, result.Y, Delta);
    }

    [TestMethod]
    public void FlipZ_NegatesZOfPoint()
    {
        Matrix4 m = Matrix4.Identity;
        m = Matrix4.FlipZ(ref m);
        Point4 p = new(3f, 4f, 5f, 1f);
        Point4 result = m * p;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(4f, result.Y, Delta);
        Assert.AreEqual(-5f, result.Z, Delta);
    }

    // Transpose

    [TestMethod]
    public void Transpose_SwapsOffDiagonalElements()
    {
        var t = Matrix4.CreateTranslation(3f, 5f, 7f);
        Matrix4 transposed = t.Transpose();

        Assert.AreEqual(t[0, 0], transposed[0, 0], Delta);
        Assert.AreEqual(t[3, 0], transposed[0, 3], Delta);
        Assert.AreEqual(t[0, 3], transposed[3, 0], Delta);
    }

    // Invert / Inverse

    [TestMethod]
    public void Inverse_MultiplyByOriginal_GivesIdentity()
    {
        var m = Matrix4.CreateScale(2f, 3f, 4f);
        Matrix4 inv = m.Inverse();
        Matrix4 product = m * inv;

        Assert.AreEqual(1f, product[0, 0], Delta);
        Assert.AreEqual(1f, product[1, 1], Delta);
        Assert.AreEqual(1f, product[2, 2], Delta);
        Assert.AreEqual(0f, product[0, 1], Delta);
    }

    [TestMethod]
    public void Invert_SingularMatrix_Throws()
    {
        Matrix4 singular = Matrix4.Zero;

        Assert.ThrowsExactly<Exception>(() => Matrix4.Invert(singular));
    }

    // Determinant

    [TestMethod]
    public void Determinant_Identity_IsOne()
    {
        Assert.AreEqual(1f, Matrix4.Identity.Determinant, Delta);
    }

    // Indexer

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        Matrix4 m = Matrix4.Identity;

        Assert.ThrowsExactly<Exception>(() => { float _ = m[4, 0]; });
    }

    // ToString

    [TestMethod]
    public void ToString_ContainsMatrixValues()
    {
        string s = Matrix4.Identity.ToString();

        Assert.IsTrue(s.Contains('1'));
    }

    // Struct-copy aliasing regression

    [TestMethod]
    public void StructCopy_IndexerMutation_DoesNotAliasOriginal()
    {
        Matrix4 original = Matrix4.Identity;
        Matrix4 copy = original;
        copy[0, 0] = 99f;

        Assert.AreEqual(1f, original[0, 0], Delta);
    }

    [TestMethod]
    public void StructCopy_ValuesArray_IsIndependent()
    {
        Matrix4 original = Matrix4.Identity;
        float[] arr = original.Values;
        arr[0] = 99f;

        Assert.AreEqual(1f, original[0, 0], Delta);
    }

    [TestMethod]
    public void WriteTo_WritesSameElementsAsValues()
    {
        Matrix4 m = new(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
        Span<float> destination = stackalloc float[16];

        m.WriteTo(destination);

        float[] expected = m.Values;
        for (int i = 0; i < 16; i++)
        {
            Assert.AreEqual(expected[i], destination[i], $"Element {i} mismatch.");
        }
    }

    [TestMethod]
    public void WriteTo_DestinationTooShort_Throws()
    {
        Matrix4 m = Matrix4.Identity;
        float[] tooShort = new float[15];

        Assert.ThrowsExactly<ArgumentException>(() => m.WriteTo(tooShort));
    }
}

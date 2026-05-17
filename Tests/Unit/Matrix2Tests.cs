using System;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class Matrix2Tests
{
    private const float Delta = 0.0001f;

    // Identity / Zero

    [TestMethod]
    public void Identity_DiagonalOnesOffDiagonalZeros()
    {
        Matrix2 m = Matrix2.Identity;

        Assert.AreEqual(1f, m[0, 0], Delta);
        Assert.AreEqual(0f, m[0, 1], Delta);
        Assert.AreEqual(0f, m[1, 0], Delta);
        Assert.AreEqual(1f, m[1, 1], Delta);
    }

    [TestMethod]
    public void Zero_AllElementsZero()
    {
        Matrix2 m = Matrix2.Zero;
        float[] v = m.Values;

        foreach (float f in v)
        {
            Assert.AreEqual(0f, f, Delta);
        }
    }

    [TestMethod]
    public void Identity_ReturnsIndependentInstances()
    {
        Matrix2 a = Matrix2.Identity;
        Matrix2 b = Matrix2.Identity;
        a.Values[0] = 99f;

        Assert.AreEqual(1f, b.Values[0], Delta);
    }

    // CreateRotation

    [TestMethod]
    public void CreateRotation_ZeroDegrees_ReturnsIdentity()
    {
        Matrix2 m = Matrix2.CreateRotation(0f);

        Assert.AreEqual(1f, m[0, 0], Delta);
        Assert.AreEqual(0f, m[0, 1], Delta);
        Assert.AreEqual(0f, m[1, 0], Delta);
        Assert.AreEqual(1f, m[1, 1], Delta);
    }

    [TestMethod]
    public void CreateRotation_NinetyDegrees_RotatesXIntoY()
    {
        Matrix2 m = Matrix2.CreateRotation(90f);
        Point p = new(1f, 0f);
        Point result = m * p;

        Assert.AreEqual(0f, result.X, Delta);
        Assert.AreEqual(1f, result.Y, Delta);
    }

    // CreateScale

    [TestMethod]
    public void CreateScale_ScalesAxesIndependently()
    {
        Matrix2 m = Matrix2.CreateScale(3f, 5f);
        Point p = new(1f, 1f);
        Point result = m * p;

        Assert.AreEqual(3f, result.X, Delta);
        Assert.AreEqual(5f, result.Y, Delta);
    }

    // Add / Subtract

    [TestMethod]
    public void Add_ComponentWise()
    {
        Matrix2 a = new(1, 2, 3, 4);
        Matrix2 b = new(10, 20, 30, 40);
        Matrix2 result = a + b;

        Assert.AreEqual(11f, result[0, 0], Delta);
        Assert.AreEqual(22f, result[0, 1], Delta);
        Assert.AreEqual(33f, result[1, 0], Delta);
        Assert.AreEqual(44f, result[1, 1], Delta);
    }

    [TestMethod]
    public void Subtract_ComponentWise()
    {
        Matrix2 a = new(10, 20, 30, 40);
        Matrix2 b = new(1, 2, 3, 4);
        Matrix2 result = a - b;

        Assert.AreEqual(9f, result[0, 0], Delta);
        Assert.AreEqual(18f, result[0, 1], Delta);
        Assert.AreEqual(27f, result[1, 0], Delta);
        Assert.AreEqual(36f, result[1, 1], Delta);
    }

    // Multiply (matrix × matrix)

    [TestMethod]
    public void Multiply_ByIdentity_Unchanged()
    {
        Matrix2 m = new(1, 2, 3, 4);
        Matrix2 result = m * Matrix2.Identity;

        Assert.AreEqual(m[0, 0], result[0, 0], Delta);
        Assert.AreEqual(m[0, 1], result[0, 1], Delta);
        Assert.AreEqual(m[1, 0], result[1, 0], Delta);
        Assert.AreEqual(m[1, 1], result[1, 1], Delta);
    }

    [TestMethod]
    public void Multiply_TwoKnownMatrices_CorrectResult()
    {
        // Column-major: (m0 m2 / m1 m3)
        // A = (1 3 / 2 4), B = (5 7 / 6 8)
        // A*B = (1*5+3*6  1*7+3*8 / 2*5+4*6  2*7+4*8) = (23 31 / 34 46)
        Matrix2 a = new(1, 2, 3, 4);
        Matrix2 b = new(5, 6, 7, 8);
        Matrix2 result = a * b;

        Assert.AreEqual(23f, result[0, 0], Delta);
        Assert.AreEqual(34f, result[0, 1], Delta);
        Assert.AreEqual(31f, result[1, 0], Delta);
        Assert.AreEqual(46f, result[1, 1], Delta);
    }

    // Multiply (scalar)

    [TestMethod]
    public void Multiply_Scalar_ScalesAllElements()
    {
        Matrix2 m = new(1, 2, 3, 4);
        Matrix2 result = m * 2f;

        Assert.AreEqual(2f, result[0, 0], Delta);
        Assert.AreEqual(4f, result[0, 1], Delta);
        Assert.AreEqual(6f, result[1, 0], Delta);
        Assert.AreEqual(8f, result[1, 1], Delta);
    }

    [TestMethod]
    public void Multiply_ScalarLeft_SameAsScalarRight()
    {
        Matrix2 m = new(1, 2, 3, 4);
        Matrix2 left = 3f * m;
        Matrix2 right = m * 3f;

        Assert.AreEqual(left[0, 0], right[0, 0], Delta);
        Assert.AreEqual(left[1, 1], right[1, 1], Delta);
    }

    // Multiply (Point)

    [TestMethod]
    public void Multiply_Point_TransformsCorrectly()
    {
        Matrix2 m = Matrix2.Identity;
        Point p = new(4f, 7f);
        Point result = m * p;

        Assert.AreEqual(4f, result.X, Delta);
        Assert.AreEqual(7f, result.Y, Delta);
    }

    // Determinant

    [TestMethod]
    public void Determinant_KnownMatrix_CorrectValue()
    {
        // (1 3 / 2 4) → det = 1*4 - 2*3 = -2
        Matrix2 m = new(1, 2, 3, 4);

        Assert.AreEqual(-2f, m.Determinant, Delta);
    }

    [TestMethod]
    public void Determinant_Identity_IsOne()
    {
        Assert.AreEqual(1f, Matrix2.Identity.Determinant, Delta);
    }

    // Transpose

    [TestMethod]
    public void Transpose_SwapsOffDiagonalElements()
    {
        Matrix2 m = new(1, 2, 3, 4);
        Matrix2 t = m.Transpose();

        Assert.AreEqual(m[0, 0], t[0, 0], Delta);
        Assert.AreEqual(m[0, 1], t[1, 0], Delta);
        Assert.AreEqual(m[1, 0], t[0, 1], Delta);
        Assert.AreEqual(m[1, 1], t[1, 1], Delta);
    }

    // Inverse

    [TestMethod]
    public void Inverse_MultiplyByOriginal_GivesIdentity()
    {
        Matrix2 m = new(1, 2, 3, 4);
        Matrix2 inv = Matrix2.Inverse(ref m);
        Matrix2 product = m * inv;

        Assert.AreEqual(1f, product[0, 0], Delta);
        Assert.AreEqual(0f, product[0, 1], Delta);
        Assert.AreEqual(0f, product[1, 0], Delta);
        Assert.AreEqual(1f, product[1, 1], Delta);
    }

    [TestMethod]
    public void Inverse_SingularMatrix_ReturnsSelf()
    {
        Matrix2 singular = new(1, 2, 2, 4);
        Matrix2 result = Matrix2.Inverse(ref singular);

        Assert.AreEqual(singular[0, 0], result[0, 0], Delta);
        Assert.AreEqual(singular[1, 1], result[1, 1], Delta);
    }

    // Indexer

    [TestMethod]
    public void Indexer_SetAndGet_RoundTrips()
    {
        Matrix2 m = Matrix2.Zero;
        m[1, 0] = 7f;

        Assert.AreEqual(7f, m[1, 0], Delta);
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        Matrix2 m = Matrix2.Identity;

        Assert.ThrowsExactly<Exception>(() => { float _ = m[2, 0]; });
    }

    // Constructor (array)

    [TestMethod]
    public void Constructor_Array_NullThrows()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new Matrix2(null!));
    }

    [TestMethod]
    public void Constructor_Array_WrongLengthThrows()
    {
        Assert.ThrowsExactly<ArgumentException>(() => _ = new Matrix2(new float[] { 1, 2, 3 }));
    }

    // RotateAroundZ

    [TestMethod]
    public void RotateAroundZ_AppliesRotationToMatrix()
    {
        Matrix2 m = Matrix2.Identity;
        Matrix2 result = Matrix2.RotateAroundZ(ref m, 90f);
        Point p = new(1f, 0f);
        Point transformed = result * p;

        Assert.AreEqual(0f, transformed.X, Delta);
        Assert.AreEqual(1f, transformed.Y, Delta);
    }

    // ScaleAroundOrigin

    [TestMethod]
    public void ScaleAroundOrigin_AppliesScaleToMatrix()
    {
        Matrix2 m = Matrix2.Identity;
        Matrix2 result = Matrix2.ScaleAroundOrigin(ref m, 2f, 3f);
        Point p = new(1f, 1f);
        Point transformed = result * p;

        Assert.AreEqual(2f, transformed.X, Delta);
        Assert.AreEqual(3f, transformed.Y, Delta);
    }

    // Struct-copy aliasing regression

    [TestMethod]
    public void StructCopy_IndexerMutation_DoesNotAliasOriginal()
    {
        Matrix2 original = Matrix2.Identity;
        Matrix2 copy = original;
        copy[0, 0] = 99f;

        Assert.AreEqual(1f, original[0, 0], Delta);
    }

    [TestMethod]
    public void StructCopy_ValuesArray_IsIndependent()
    {
        Matrix2 original = Matrix2.Identity;
        float[] arr = original.Values;
        arr[0] = 99f;

        Assert.AreEqual(1f, original[0, 0], Delta);
    }

    [TestMethod]
    public void ArrayConstructor_MutatingSourceArray_DoesNotAffectMatrix()
    {
        float[] arr = new float[] { 1, 0, 0, 1 };
        Matrix2 m = new(arr);
        arr[0] = 99f;

        Assert.AreEqual(1f, m[0, 0], Delta);
    }
}

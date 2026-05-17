using System;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class Matrix3Tests
{
    private const double Delta = 1e-4;

    private static void AssertMatricesEqual(Matrix3 expected, Matrix3 actual, double delta = Delta)
    {
        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(expected.Values[i], actual.Values[i], delta, $"Element {i} mismatch.");
        }
    }

    // Identity / Zero / Flip statics

    [TestMethod]
    public void Identity_LeavesPointUnchanged()
    {
        var id = Matrix3.Identity;
        var p = new Point(3f, 4f);
        Assert.AreEqual(p, Matrix3.Multiply(ref id, p));
    }

    [TestMethod]
    public void Zero_MapsAllPointsToOrigin()
    {
        var zero = Matrix3.Zero;
        Assert.AreEqual(new Point(0f, 0f), Matrix3.Multiply(ref zero, new Point(5f, 7f)));
    }

    [TestMethod]
    public void FlipX_NegatesXComponent()
    {
        var flip = Matrix3.FlipXMatrix;
        Assert.AreEqual(new Point(-3f, 4f), Matrix3.Multiply(ref flip, new Point(3f, 4f)));
    }

    [TestMethod]
    public void FlipY_NegatesYComponent()
    {
        var flip = Matrix3.FlipYMatrix;
        Assert.AreEqual(new Point(3f, -4f), Matrix3.Multiply(ref flip, new Point(3f, 4f)));
    }

    // Constructors / indexer

    [TestMethod]
    public void NineElementConstructor_StoresColumnMajor()
    {
        var m = new Matrix3(0, 1, 2, 3, 4, 5, 6, 7, 8);
        Assert.AreEqual(0f, m[0, 0]);
        Assert.AreEqual(1f, m[0, 1]);
        Assert.AreEqual(2f, m[0, 2]);
        Assert.AreEqual(3f, m[1, 0]);
        Assert.AreEqual(4f, m[1, 1]);
        Assert.AreEqual(5f, m[1, 2]);
        Assert.AreEqual(6f, m[2, 0]);
        Assert.AreEqual(7f, m[2, 1]);
        Assert.AreEqual(8f, m[2, 2]);
    }

    [TestMethod]
    public void Indexer_Setter_UpdatesValue()
    {
        var m = Matrix3.Identity;
        m[1, 2] = 99f;
        Assert.AreEqual(99f, m[1, 2]);
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        var m = Matrix3.Identity;
        Assert.ThrowsExactly<ArgumentException>(() => { _ = m[3, 0]; });
        Assert.ThrowsExactly<ArgumentException>(() => { _ = m[0, -1]; });
    }

    // Factories

    [TestMethod]
    public void CreateTranslation_TranslatesPoint()
    {
        var t = Matrix3.CreateTranslation(10f, 20f);
        Assert.AreEqual(new Point(13f, 24f), Matrix3.Multiply(ref t, new Point(3f, 4f)));
    }

    [TestMethod]
    public void CreateScale_ScalesPoint()
    {
        var s = Matrix3.CreateScale(2f, 3f);
        var p = Matrix3.Multiply(ref s, new Point(3f, 4f));
        Assert.AreEqual(6.0, p.X, Delta);
        Assert.AreEqual(12.0, p.Y, Delta);
    }

    [TestMethod]
    public void CreateRotationAroundZAxis_90Degrees_RotatesXAxisToYAxis()
    {
        var r = Matrix3.CreateRotationAroundZAxis(90f);
        var p = Matrix3.Multiply(ref r, new Point(1f, 0f));
        Assert.AreEqual(0.0, p.X, Delta);
        Assert.AreEqual(1.0, p.Y, Delta);
    }

    [TestMethod]
    public void CreateRotationAroundZAxis_360Degrees_IsApproximatelyIdentity()
    {
        var r = Matrix3.CreateRotationAroundZAxis(360f);
        AssertMatricesEqual(Matrix3.Identity, r);
    }

    [TestMethod]
    public void CreateOrtho_MapsScreenSpaceCornersToNDC()
    {
        // Top-left of screen (0, 0) maps to (-1, +1) in NDC; bottom-right (W, H) maps to (+1, -1).
        var o = Matrix3.CreateOrtho(800f, 600f);
        var topLeft = Matrix3.Multiply(ref o, new Point(0f, 0f));
        var bottomRight = Matrix3.Multiply(ref o, new Point(800f, 600f));
        Assert.AreEqual(-1.0, topLeft.X, Delta);
        Assert.AreEqual(1.0, topLeft.Y, Delta);
        Assert.AreEqual(1.0, bottomRight.X, Delta);
        Assert.AreEqual(-1.0, bottomRight.Y, Delta);
    }

    [TestMethod]
    public void CreateFBOOrtho_MapsCornersWithoutYFlip()
    {
        // FBO variant: (0, 0) → (-1, -1), (W, H) → (+1, +1).
        var o = Matrix3.CreateFBOOrtho(800f, 600f);
        var topLeft = Matrix3.Multiply(ref o, new Point(0f, 0f));
        var bottomRight = Matrix3.Multiply(ref o, new Point(800f, 600f));
        Assert.AreEqual(-1.0, topLeft.X, Delta);
        Assert.AreEqual(-1.0, topLeft.Y, Delta);
        Assert.AreEqual(1.0, bottomRight.X, Delta);
        Assert.AreEqual(1.0, bottomRight.Y, Delta);
    }

    // Add / Subtract

    [TestMethod]
    public void Add_SumsComponentWise()
    {
        var a = new Matrix3(1, 1, 1, 1, 1, 1, 1, 1, 1);
        var b = new Matrix3(2, 2, 2, 2, 2, 2, 2, 2, 2);
        var sum = Matrix3.Add(ref a, ref b);
        AssertMatricesEqual(new Matrix3(3, 3, 3, 3, 3, 3, 3, 3, 3), sum);
    }

    [TestMethod]
    public void Subtract_SubtractsComponentWise()
    {
        var a = new Matrix3(5, 5, 5, 5, 5, 5, 5, 5, 5);
        var b = new Matrix3(2, 2, 2, 2, 2, 2, 2, 2, 2);
        var diff = Matrix3.Subtract(ref a, ref b);
        AssertMatricesEqual(new Matrix3(3, 3, 3, 3, 3, 3, 3, 3, 3), diff);
    }

    // Multiply

    [TestMethod]
    public void MultiplyMatrixMatrix_IdentityIsNoOp()
    {
        var id = Matrix3.Identity;
        var m = new Matrix3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        AssertMatricesEqual(m, Matrix3.Multiply(ref m, ref id));
        AssertMatricesEqual(m, Matrix3.Multiply(ref id, ref m));
    }

    [TestMethod]
    public void MultiplyMatrixScalar_ScalesAllElements()
    {
        var m = new Matrix3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var scaled = Matrix3.Multiply(ref m, 2f);
        AssertMatricesEqual(new Matrix3(2, 4, 6, 8, 10, 12, 14, 16, 18), scaled);
    }

    [TestMethod]
    public void MultiplyMatrixPoint3_TransformsHomogeneousPoint()
    {
        var t = Matrix3.CreateTranslation(10f, 20f);
        // For a Point3 with z=1, translation should apply.
        var p = Matrix3.Multiply(ref t, new Point3(3f, 4f, 1f));
        Assert.AreEqual(13.0, p.X, Delta);
        Assert.AreEqual(24.0, p.Y, Delta);
    }

    // Translate / Rotate / Scale composition helpers

    [TestMethod]
    public void Translate_AddsTranslationToMatrix()
    {
        var m = Matrix3.Identity;
        var t = Matrix3.Translate(ref m, 5f, 6f);
        var p = Matrix3.Multiply(ref t, new Point(1f, 2f));
        Assert.AreEqual(6.0, p.X, Delta);
        Assert.AreEqual(8.0, p.Y, Delta);
    }

    [TestMethod]
    public void RotateAroundZ_AppliesRotation()
    {
        var m = Matrix3.Identity;
        var r = Matrix3.RotateAroundZ(ref m, 90f);
        var p = Matrix3.Multiply(ref r, new Point(1f, 0f));
        Assert.AreEqual(0.0, p.X, Delta);
        Assert.AreEqual(1.0, p.Y, Delta);
    }

    [TestMethod]
    public void RotateAroundPoint_RotatesAroundCentre()
    {
        var m = Matrix3.Identity;
        var r = Matrix3.RotateAroundPoint(ref m, 90f, 5f, 0f);
        // A point at (10, 0) rotated 90° around (5, 0) should land at (5, 5).
        var p = Matrix3.Multiply(ref r, new Point(10f, 0f));
        Assert.AreEqual(5.0, p.X, Delta);
        Assert.AreEqual(5.0, p.Y, Delta);
    }

    [TestMethod]
    public void RotateAroundPoint_PointOverload_MatchesXYOverload()
    {
        var m = Matrix3.Identity;
        var fromXY = Matrix3.RotateAroundPoint(ref m, 45f, 3f, 4f);
        var fromPoint = Matrix3.RotateAroundPoint(ref m, 45f, new Point(3f, 4f));
        AssertMatricesEqual(fromXY, fromPoint);
    }

    [TestMethod]
    public void ScaleAroundOrigin_ScalesAroundOrigin()
    {
        var m = Matrix3.Identity;
        var s = Matrix3.ScaleAroundOrigin(ref m, 2f, 3f);
        var p = Matrix3.Multiply(ref s, new Point(1f, 1f));
        Assert.AreEqual(2.0, p.X, Delta);
        Assert.AreEqual(3.0, p.Y, Delta);
    }

    [TestMethod]
    public void ScaleAroundPoint_KeepsCentreFixed()
    {
        var m = Matrix3.Identity;
        var s = Matrix3.ScaleAroundPoint(ref m, 2f, 2f, 5f, 5f);
        // Centre (5, 5) should map to itself.
        var p = Matrix3.Multiply(ref s, new Point(5f, 5f));
        Assert.AreEqual(5.0, p.X, Delta);
        Assert.AreEqual(5.0, p.Y, Delta);
    }

    [TestMethod]
    public void FlipX_AppliedToIdentity_NegatesX()
    {
        var m = Matrix3.Identity;
        var flipped = Matrix3.FlipX(ref m);
        var p = Matrix3.Multiply(ref flipped, new Point(3f, 4f));
        Assert.AreEqual(-3.0, p.X, Delta);
        Assert.AreEqual(4.0, p.Y, Delta);
    }

    [TestMethod]
    public void FlipY_AppliedToIdentity_NegatesY()
    {
        var m = Matrix3.Identity;
        var flipped = Matrix3.FlipY(ref m);
        var p = Matrix3.Multiply(ref flipped, new Point(3f, 4f));
        Assert.AreEqual(3.0, p.X, Delta);
        Assert.AreEqual(-4.0, p.Y, Delta);
    }

    // Determinant / Inverse / Transpose

    [TestMethod]
    public void Determinant_OfIdentity_IsOne()
    {
        Assert.AreEqual(1.0, Matrix3.Identity.Determinant, Delta);
    }

    [TestMethod]
    public void Determinant_OfZero_IsZero()
    {
        Assert.AreEqual(0.0, Matrix3.Zero.Determinant, Delta);
    }

    [TestMethod]
    public void Inverse_OfIdentity_IsIdentity()
    {
        AssertMatricesEqual(Matrix3.Identity, Matrix3.Identity.Inverse());
    }

    [TestMethod]
    public void Inverse_TimesOriginal_IsIdentity()
    {
        var m = new Matrix3(2, 0, 0, 0, 3, 0, 1, 1, 1);
        var inv = m.Inverse();
        var product = m * inv;
        AssertMatricesEqual(Matrix3.Identity, product, 1e-3);
    }

    [TestMethod]
    public void Invert_ByValueAndByRef_AgreeOnNonSingularMatrix()
    {
        var m = new Matrix3(2, 0, 0, 0, 3, 0, 1, 1, 1);
        var byValue = Matrix3.Invert(m);
        var byRef = Matrix3.Invert(ref m);
        AssertMatricesEqual(byValue, byRef);
    }

    [TestMethod]
    public void Transpose_MirrorsAcrossDiagonal()
    {
        var m = new Matrix3(0, 1, 2, 3, 4, 5, 6, 7, 8);
        var t = m.Transpose();
        // Column-major (0,3,6 / 1,4,7 / 2,5,8) transposed swaps off-diagonal pairs.
        Assert.AreEqual(0f, t[0, 0]);
        Assert.AreEqual(3f, t[0, 1]);
        Assert.AreEqual(6f, t[0, 2]);
        Assert.AreEqual(1f, t[1, 0]);
        Assert.AreEqual(4f, t[1, 1]);
        Assert.AreEqual(7f, t[1, 2]);
    }

    [TestMethod]
    public void Transpose_OfIdentity_IsIdentity()
    {
        AssertMatricesEqual(Matrix3.Identity, Matrix3.Identity.Transpose());
    }

    // Operators

    [TestMethod]
    public void OperatorMultiply_ScalarLeft_AndRight_AgreeWithMultiplyHelper()
    {
        var m = new Matrix3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        AssertMatricesEqual(2f * m, m * 2f);
    }

    [TestMethod]
    public void OperatorMultiply_TwoMatrices_ComposesTransforms()
    {
        // Translate then scale should equal the composed matrix applied to a point.
        var t = Matrix3.CreateTranslation(1f, 2f);
        var s = Matrix3.CreateScale(2f, 3f);
        var composed = s * t; // s applied after t
        var p = Matrix3.Multiply(ref composed, new Point(0f, 0f));
        Assert.AreEqual(2.0, p.X, Delta);
        Assert.AreEqual(6.0, p.Y, Delta);
    }

    [TestMethod]
    public void OperatorMultiply_MatrixPoint_AppliesTransform()
    {
        var t = Matrix3.CreateTranslation(5f, 6f);
        var p = t * new Point(1f, 2f);
        Assert.AreEqual(6.0, p.X, Delta);
        Assert.AreEqual(8.0, p.Y, Delta);
    }

    [TestMethod]
    public void OperatorAdd_SumsComponentWise()
    {
        var a = new Matrix3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Matrix3(9, 8, 7, 6, 5, 4, 3, 2, 1);
        AssertMatricesEqual(new Matrix3(10, 10, 10, 10, 10, 10, 10, 10, 10), a + b);
    }

    [TestMethod]
    public void OperatorSubtract_SubtractsComponentWise()
    {
        var a = new Matrix3(10, 10, 10, 10, 10, 10, 10, 10, 10);
        var b = new Matrix3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        AssertMatricesEqual(new Matrix3(9, 8, 7, 6, 5, 4, 3, 2, 1), a - b);
    }

    // Struct-copy aliasing regression

    [TestMethod]
    public void StructCopy_IndexerMutation_DoesNotAliasOriginal()
    {
        var original = Matrix3.Identity;
        var copy = original;
        copy[0, 0] = 99f;

        Assert.AreEqual(1f, original[0, 0]);
    }

    [TestMethod]
    public void StructCopy_ValuesArray_IsIndependent()
    {
        var original = Matrix3.Identity;
        float[] arr = original.Values;
        arr[0] = 99f;

        Assert.AreEqual(1f, original[0, 0]);
    }

    [TestMethod]
    public void ArrayConstructor_MutatingSourceArray_DoesNotAffectMatrix()
    {
        float[] arr = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        var m = new Matrix3(arr);
        arr[0] = 99f;

        Assert.AreEqual(1f, m[0, 0]);
    }
}

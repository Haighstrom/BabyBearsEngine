using System.Linq;
using System.Runtime.InteropServices;

namespace BabyBearsEngine.Geometry;

/// <summary>
/// A 2×2 matrix used for 2D linear transformations (rotation, scale).
/// Elements are stored column-major in <see cref="Values"/>:
/// <code>
/// (0  2)
/// (1  3)
/// </code>
/// Most static helpers take <c>ref Matrix2</c> for performance — they return new matrices and never mutate the inputs.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Matrix2
{
    /// <summary>The identity matrix — leaves any vector or matrix unchanged when multiplied. Returns a fresh instance per access.</summary>
    public static Matrix2 Identity => new(1, 0, 0, 1);

    /// <summary>The zero matrix — all elements are zero. Returns a fresh instance per access.</summary>
    public static Matrix2 Zero => new(0, 0, 0, 0);

    /// <summary>Returns a rotation matrix that rotates points around the Z axis by <paramref name="angleInDegrees"/> (counter-clockwise in a right-handed coordinate system).</summary>
    public static Matrix2 CreateRotation(float angleInDegrees)
    {
        double radians = Math.PI * angleInDegrees / 180.0;
        return new Matrix2((float)Math.Cos(radians), (float)Math.Sin(radians), -(float)Math.Sin(radians), (float)Math.Cos(radians));
    }

    /// <summary>Returns a scale matrix that scales X by <paramref name="scaleX"/> and Y by <paramref name="scaleY"/>.</summary>
    public static Matrix2 CreateScale(float scaleX, float scaleY)
    {
        return new Matrix2(scaleX, 0, 0, scaleY);
    }

    /// <summary>Returns a new matrix whose elements are the component-wise sum of <paramref name="mat1"/> and <paramref name="mat2"/>.</summary>
    public static Matrix2 Add(ref Matrix2 mat1, ref Matrix2 mat2)
    {
        float[] values = mat1._values.Zip(mat2._values, (a, b) => a + b).ToArray();
        return new Matrix2(values);
    }

    /// <summary>Returns a new matrix whose elements are the component-wise difference of <paramref name="mat1"/> and <paramref name="mat2"/>.</summary>
    public static Matrix2 Subtract(ref Matrix2 mat1, ref Matrix2 mat2) => new(mat1._values.Zip(mat2._values, (a, b) => a - b).ToArray());

    /// <summary>Returns the matrix product <paramref name="mat1"/> × <paramref name="mat2"/>. Composition order: <paramref name="mat2"/> is applied first.</summary>
    public static Matrix2 Multiply(ref Matrix2 mat1, ref Matrix2 mat2)
    {
        return new Matrix2
            (
                mat1._values[0] * mat2._values[0] + mat1._values[2] * mat2._values[1],
                mat1._values[1] * mat2._values[0] + mat1._values[3] * mat2._values[1],
                mat1._values[0] * mat2._values[2] + mat1._values[2] * mat2._values[3],
                mat1._values[1] * mat2._values[2] + mat1._values[3] * mat2._values[3]

            );
    }

    /// <summary>Returns a new matrix with every element of <paramref name="mat"/> multiplied by the scalar <paramref name="f"/>.</summary>
    public static Matrix2 Multiply(ref Matrix2 mat, float f)
    {
        return new Matrix2
            (
                mat._values[0] * f,
                mat._values[1] * f,
                mat._values[2] * f,
                mat._values[3] * f
            );
    }

    /// <summary>Transforms <paramref name="p"/> by <paramref name="mat"/>, returning a new <see cref="Point"/>.</summary>
    public static Point Multiply(ref Matrix2 mat, Point p)
    {
        return new Point
            (
                p.X * mat._values[0] + p.Y * mat._values[2],
                p.X * mat._values[1] + p.Y * mat._values[3]
            );
    }

    /// <summary>Returns a new matrix that is <paramref name="mat"/> rotated around the Z axis by <paramref name="angleInDegrees"/>.</summary>
    public static Matrix2 RotateAroundZ(ref Matrix2 mat, float angleInDegrees)
    {
        var rotMat = CreateRotation(angleInDegrees);
        return Multiply(ref mat, ref rotMat);
    }

    /// <summary>Returns a new matrix that is <paramref name="mat"/> scaled around the origin by (<paramref name="scaleX"/>, <paramref name="scaleY"/>).</summary>
    public static Matrix2 ScaleAroundOrigin(ref Matrix2 mat, float scaleX, float scaleY)
    {
        var scaleMat = CreateScale(scaleX, scaleY);
        return Multiply(ref mat, ref scaleMat);
    }

    /// <summary>
    /// Returns the inverse of <paramref name="mat"/>. If the matrix is singular (determinant is zero), returns <paramref name="mat"/> unchanged.
    /// </summary>
    public static Matrix2 Inverse(ref Matrix2 mat)
    {
        float det = mat.Determinant;

        if (det == 0)
        {
            return mat;
        }

        float invDet = 1 / det;

        return new Matrix2
            (
                mat._values[3] * invDet,
                -mat._values[1] * invDet,
                -mat._values[2] * invDet,
                mat._values[0] * invDet
            );
    }


    private float[] _values;


    /// <summary>Initialises a new <see cref="Matrix2"/> with individual column-major element values.</summary>
    /// <param name="m0">Element [0,0]</param>
    /// <param name="m1">Element [0,1]</param>
    /// <param name="m2">Element [1,0]</param>
    /// <param name="m3">Element [1,1]</param>
    public Matrix2(float m0, float m1, float m2, float m3)
    {
        _values = new float[4] { m0, m1, m2, m3 };
    }

    /// <summary>Initialises a new <see cref="Matrix2"/> from a 4-element column-major array. The array is used directly — callers should clone if needed.</summary>
    /// <param name="values">Array of 4 floats in column-major order.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> does not have exactly 4 elements.</exception>
    public Matrix2(float[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (values.Length != 4)
        {
            throw new ArgumentException($"Did not supply 4 values, but {values.Length}.", nameof(values));
        }

        _values = values;
    }


    /// <summary>
    /// Gets or sets a matrix element by column (<paramref name="x"/>) and row (<paramref name="y"/>), both in the range [0, 1].
    /// </summary>
    /// <exception cref="Exception">Thrown when either index is out of range.</exception>
    public float this[int x, int y]
    {
        get
        {
            if (x < 0 || x > 1 || y < 0 || y > 1)
            {
                throw new Exception($"Requested an invalid Matrix2 index:{x},{y}");
            }

            return _values[x * 2 + y];
        }
        set
        {
            if (x < 0 || x > 1 || y < 0 || y > 1)
            {
                throw new Exception($"Requested an invalid Matrix2 index:{x},{y}");
            }

            _values[x * 2 + y] = value;
        }
    }


    /// <summary>
    /// Matrix elements, a 4 element array indexed as
    /// (0 2)
    /// (1 3)
    /// </summary>
    public float[] Values
    {
        get => _values;
        set => _values = value;
    }

    /// <summary>Gets the determinant of this matrix.</summary>
    public float Determinant => _values[0] * _values[3] - _values[1] * _values[2];

    /// <summary>Returns the transpose of this matrix — elements mirrored across the main diagonal. Does not modify this instance.</summary>
    public Matrix2 Transpose()
    {
        return new Matrix2
            (
                _values[0],
                _values[2],
                _values[1],
                _values[3]
            );
    }

    /// <summary>Scalar multiplication.</summary>
    /// <param name="left">The scalar.</param>
    /// <param name="right">The matrix.</param>
    /// <returns>A new <see cref="Matrix2"/> with every element multiplied by <paramref name="left"/>.</returns>
    public static Matrix2 operator *(float left, Matrix2 right) => Multiply(ref right, left);

    /// <summary>Scalar multiplication.</summary>
    /// <param name="left">The matrix.</param>
    /// <param name="right">The scalar.</param>
    /// <returns>A new <see cref="Matrix2"/> with every element multiplied by <paramref name="right"/>.</returns>
    public static Matrix2 operator *(Matrix2 left, float right) => Multiply(ref left, right);

    /// <summary>Transforms <paramref name="right"/> by <paramref name="left"/>.</summary>
    public static Point operator *(Matrix2 left, Point right) => Multiply(ref left, right);

    /// <summary>Matrix multiplication.</summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>A new <see cref="Matrix2"/> holding the result of the multiplication.</returns>
    public static Matrix2 operator *(Matrix2 left, Matrix2 right) => Multiply(ref left, ref right);

    /// <summary>Matrix addition.</summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>A new <see cref="Matrix2"/> holding the component-wise sum.</returns>
    public static Matrix2 operator +(Matrix2 left, Matrix2 right) => Add(ref left, ref right);

    /// <summary>Matrix subtraction.</summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>A new <see cref="Matrix2"/> holding the component-wise difference.</returns>
    public static Matrix2 operator -(Matrix2 left, Matrix2 right) => Subtract(ref left, ref right);

}

using System.Linq;

namespace BabyBearsEngine.Geometry;

/// <summary>
/// A 4×4 matrix used for 3D affine transformations (translation, rotation, scale) and projection.
/// Elements are stored column-major in <see cref="Values"/>:
/// <code>
/// (0   4   8  12)
/// (1   5   9  13)
/// (2   6  10  14)
/// (3   7  11  15)
/// </code>
/// Multiplication composes transforms: <c>a * b</c> applies <c>b</c> first then <c>a</c>.
/// Most static helpers take <c>ref Matrix4</c> for performance — they return new matrices and never mutate the inputs.
/// </summary>
public struct Matrix4
{
    /// <summary>The identity matrix — leaves any vector or matrix unchanged when multiplied. Returns a fresh instance per access.</summary>
    public static Matrix4 Identity => new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    /// <summary>The zero matrix — all elements are zero. Returns a fresh instance per access.</summary>
    public static Matrix4 Zero => new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>A reflection across the Y axis (negates X). Returns a fresh instance per access (the underlying array is not shared).</summary>
    public static Matrix4 FlipXMatrix => new(-1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    /// <summary>A reflection across the X axis (negates Y). Returns a fresh instance per access (the underlying array is not shared).</summary>
    public static Matrix4 FlipYMatrix => new(1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    /// <summary>A reflection across the Z axis (negates Z). Returns a fresh instance per access (the underlying array is not shared).</summary>
    public static Matrix4 FlipZMatrix => new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1);

    /// <summary>Returns a translation matrix that moves points by (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>).</summary>
    public static Matrix4 CreateTranslation(float x, float y, float z)
    {
        return new Matrix4(1, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, 1, 0,
                            x, y, z, 1);
    }

    /// <summary>Returns a translation matrix that moves points by (<paramref name="x"/>, <paramref name="y"/>, 0).</summary>
    public static Matrix4 CreateTranslation(float x, float y)
    {
        return new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, x, y, 0, 1);
    }

    /// <summary>Returns a rotation matrix that rotates points around the Z axis by <paramref name="angleInDegrees"/> (counter-clockwise in a right-handed coordinate system).</summary>
    public static Matrix4 CreateRotationAroundZAxis(float angleInDegrees)
    {
        double radians = Math.PI * angleInDegrees / 180.0;
        return new Matrix4((float)Math.Cos(radians), (float)Math.Sin(radians), 0, 0, -(float)Math.Sin(radians), (float)Math.Cos(radians), 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
    }

    /// <summary>Returns a scale matrix that scales X by <paramref name="scaleX"/>, Y by <paramref name="scaleY"/>, and Z by <paramref name="scaleZ"/> (default 1).</summary>
    public static Matrix4 CreateScale(float scaleX, float scaleY, float scaleZ = 1) => new(scaleX, 0, 0, 0, 0, scaleY, 0, 0, 0, 0, scaleZ, 0, 0, 0, 0, 1);

    /// <summary>
    /// Returns an orthographic projection matrix mapping a (<paramref name="width"/>, <paramref name="height"/>) screen-space rectangle
    /// (top-left origin, Y down) onto NDC (-1..1, Y up). Use this when rendering directly to the screen.
    /// </summary>
    public static Matrix4 CreateOrtho(float width, float height)
    {
        var mat = Identity;
        mat = ScaleAroundOrigin(ref mat, 2 / width, 2 / height, 1);
        mat = FlipY(ref mat);
        mat = Translate(ref mat, -width / 2, -height / 2, 0);
        return mat;
    }

    /// <summary>
    /// Same as <see cref="CreateOrtho"/> but without the Y flip. Use this when rendering to a framebuffer texture
    /// rather than directly to the screen, since the framebuffer's Y axis already matches OpenGL's convention.
    /// </summary>
    public static Matrix4 CreateFBOOrtho(float width, float height)
    {
        var mat = Identity;
        mat = ScaleAroundOrigin(ref mat, 2 / width, 2 / height, 1);
        mat = Translate(ref mat, -width / 2, -height / 2, 0);
        return mat;
    }

    /// <summary>Returns a new matrix whose elements are the component-wise sum of <paramref name="mat1"/> and <paramref name="mat2"/>.</summary>
    public static Matrix4 Add(ref Matrix4 mat1, ref Matrix4 mat2)
    {
        float[] values = mat1._values.Zip(mat2._values, (a, b) => a + b).ToArray();
        return new Matrix4(values);
    }

    /// <summary>Returns a new matrix whose elements are the component-wise difference of <paramref name="mat1"/> and <paramref name="mat2"/>.</summary>
    public static Matrix4 Subtract(ref Matrix4 mat1, ref Matrix4 mat2)
    {
        float[] values = mat1._values.Zip(mat2._values, (a, b) => a - b).ToArray();
        return new Matrix4(values);
    }

    /// <summary>Returns the matrix product <paramref name="mat1"/> × <paramref name="mat2"/>. Composition order: <paramref name="mat2"/> is applied first.</summary>
    public static Matrix4 Multiply(ref Matrix4 mat1, ref Matrix4 mat2)
    {
        float[] values = new float[16] {
     /*0*/  mat1._values[0]*mat2._values[0]+mat1._values[4]*mat2._values[1]+mat1._values[8]*mat2._values[2]+mat1._values[12]*mat2._values[3],
     /*1*/  mat1._values[1]*mat2._values[0]+mat1._values[5]*mat2._values[1]+mat1._values[9]*mat2._values[2]+mat1._values[13]*mat2._values[3],
     /*2*/  mat1._values[2]*mat2._values[0]+mat1._values[6]*mat2._values[1]+mat1._values[10]*mat2._values[2]+mat1._values[14]*mat2._values[3],
     /*3*/  mat1._values[3]*mat2._values[0]+mat1._values[7]*mat2._values[1]+mat1._values[11]*mat2._values[2]+mat1._values[15]*mat2._values[3],
     /*4*/  mat1._values[0]*mat2._values[4]+mat1._values[4]*mat2._values[5]+mat1._values[8]*mat2._values[6]+mat1._values[12]*mat2._values[7],
     /*5*/  mat1._values[1]*mat2._values[4]+mat1._values[5]*mat2._values[5]+mat1._values[9]*mat2._values[6]+mat1._values[13]*mat2._values[7],
     /*6*/  mat1._values[2]*mat2._values[4]+mat1._values[6]*mat2._values[5]+mat1._values[10]*mat2._values[6]+mat1._values[14]*mat2._values[7],
     /*7*/  mat1._values[3]*mat2._values[4]+mat1._values[7]*mat2._values[5]+mat1._values[11]*mat2._values[6]+mat1._values[15]*mat2._values[7],
     /*8*/  mat1._values[0]*mat2._values[8]+mat1._values[4]*mat2._values[9]+mat1._values[8]*mat2._values[10]+mat1._values[12]*mat2._values[11],
     /*9*/  mat1._values[1]*mat2._values[8]+mat1._values[5]*mat2._values[9]+mat1._values[9]*mat2._values[10]+mat1._values[13]*mat2._values[11],
    /*10*/  mat1._values[2]*mat2._values[8]+mat1._values[6]*mat2._values[9]+mat1._values[10]*mat2._values[10]+mat1._values[14]*mat2._values[11],
    /*11*/  mat1._values[3]*mat2._values[8]+mat1._values[7]*mat2._values[9]+mat1._values[11]*mat2._values[10]+mat1._values[15]*mat2._values[11],
    /*12*/  mat1._values[0]*mat2._values[12]+mat1._values[4]*mat2._values[13]+mat1._values[8]*mat2._values[14]+mat1._values[12]*mat2._values[15],
    /*13*/  mat1._values[1]*mat2._values[12]+mat1._values[5]*mat2._values[13]+mat1._values[9]*mat2._values[14]+mat1._values[13]*mat2._values[15],
    /*14*/  mat1._values[2]*mat2._values[12]+mat1._values[6]*mat2._values[13]+mat1._values[10]*mat2._values[14]+mat1._values[14]*mat2._values[15],
    /*15*/  mat1._values[3]*mat2._values[12]+mat1._values[7]*mat2._values[13]+mat1._values[11]*mat2._values[14]+mat1._values[15]*mat2._values[15]
        };
        return new Matrix4(values);
    }

    /// <summary>Returns a new matrix with every element of <paramref name="mat"/> multiplied by the scalar <paramref name="f"/>.</summary>
    public static Matrix4 Multiply(ref Matrix4 mat, float f)
    {
        return new Matrix4
            (
                mat._values[0] * f,
                mat._values[1] * f,
                mat._values[2] * f,
                mat._values[3] * f,
                mat._values[4] * f,
                mat._values[5] * f,
                mat._values[6] * f,
                mat._values[7] * f,
                mat._values[8] * f,
                mat._values[9] * f,
                mat._values[10] * f,
                mat._values[11] * f,
                mat._values[12] * f,
                mat._values[13] * f,
                mat._values[14] * f,
                mat._values[15] * f
            );
    }

    /// <summary>Transforms <paramref name="p"/> by <paramref name="mat"/>, returning a new <see cref="Point4"/>.</summary>
    public static Point4 Multiply(ref Matrix4 mat, Point4 p)
    {
        return new Point4
            (
                mat._values[0] * p.x + mat._values[4] * p.y + mat._values[8] * p.z + mat._values[12] * p.w,
                mat._values[1] * p.x + mat._values[5] * p.y + mat._values[9] * p.z + mat._values[13] * p.w,
                mat._values[2] * p.x + mat._values[6] * p.y + mat._values[10] * p.z + mat._values[14] * p.w,
                mat._values[3] * p.x + mat._values[7] * p.y + mat._values[11] * p.z + mat._values[15] * p.w
            );
    }

    /// <summary>
    /// Transforms <paramref name="p"/> by <paramref name="mat"/>. The point is padded to <c>(x, y, 0, 1)</c> before multiplication;
    /// only the first two components of the result are returned as a <see cref="Point"/>.
    /// </summary>
    public static Point Multiply(ref Matrix4 mat, Point p)
    {
        return new Point
            (
                mat._values[0] * p.X + mat._values[4] * p.Y + mat._values[12],
                mat._values[1] * p.X + mat._values[5] * p.Y + mat._values[13]
            );
    }

    /// <summary>Returns <paramref name="mat"/> composed with a translation by (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>).</summary>
    public static Matrix4 Translate(ref Matrix4 mat, float x, float y, float z)
    {
        var transMat = CreateTranslation(x, y, z);
        return Multiply(ref mat, ref transMat);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a rotation around the Z axis by <paramref name="angleInDegrees"/>.</summary>
    public static Matrix4 RotateAroundZ(ref Matrix4 mat, float angleInDegrees)
    {
        var rotMat = CreateRotationAroundZAxis(angleInDegrees);
        return Multiply(ref mat, ref rotMat);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a rotation by <paramref name="angleInDegrees"/> centred on <paramref name="p"/>.</summary>
    public static Matrix4 RotateAroundPoint(ref Matrix4 mat, float angleInDegrees, Point p) => RotateAroundPoint(ref mat, angleInDegrees, p.X, p.Y);

    /// <summary>Returns <paramref name="mat"/> composed with a rotation by <paramref name="angleInDegrees"/> centred on (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public static Matrix4 RotateAroundPoint(ref Matrix4 mat, float angleInDegrees, float x, float y)
    {
        var translate1 = CreateTranslation(x, y, 0);
        var rotate = CreateRotationAroundZAxis(angleInDegrees);
        var translate2 = CreateTranslation(-x, -y, 0);

        var result = Multiply(ref mat, ref translate1);
        result = Multiply(ref result, ref rotate);
        result = Multiply(ref result, ref translate2);

        return result;
    }

    /// <summary>Returns <paramref name="mat"/> composed with a scale around the origin by (<paramref name="scaleX"/>, <paramref name="scaleY"/>, <paramref name="scaleZ"/>).</summary>
    public static Matrix4 ScaleAroundOrigin(ref Matrix4 mat, float scaleX, float scaleY, float scaleZ)
    {
        var scaleMat = CreateScale(scaleX, scaleY, scaleZ);
        return Multiply(ref mat, ref scaleMat);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a scale centred on (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public static Matrix4 ScaleAroundPoint(ref Matrix4 mat, float scaleX, float scaleY, float x, float y)
    {
        var translate1 = CreateTranslation(x, y, 0);
        var scale = CreateScale(scaleX, scaleY, 0);
        var translate2 = CreateTranslation(-x, -y, 0);

        var result = Multiply(ref mat, ref translate1);
        result = Multiply(ref result, ref scale);
        result = Multiply(ref result, ref translate2);

        return result;
    }

    /// <summary>Returns <paramref name="mat"/> composed with a reflection across the Y axis (negates X).</summary>
    public static Matrix4 FlipX(ref Matrix4 mat)
    {
        var flip = FlipXMatrix;
        return Multiply(ref mat, ref flip);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a reflection across the X axis (negates Y).</summary>
    public static Matrix4 FlipY(ref Matrix4 mat)
    {
        var flip = FlipYMatrix;
        return Multiply(ref mat, ref flip);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a reflection across the Z axis (negates Z).</summary>
    public static Matrix4 FlipZ(ref Matrix4 mat)
    {
        var flip = FlipZMatrix;
        return Multiply(ref mat, ref flip);
    }

    /// <summary>
    /// Returns the inverse of <paramref name="mat"/> using Gauss-Jordan elimination.
    /// If the matrix is singular, returns <paramref name="mat"/> unchanged where singular detection succeeds,
    /// or throws if a zero pivot is encountered.
    /// </summary>
    /// <exception cref="Exception">Thrown when the matrix is singular and cannot be inverted.</exception>
    public static Matrix4 Invert(Matrix4 mat)
    {
        int[] colIdx = { 0, 0, 0, 0 };
        int[] rowIdx = { 0, 0, 0, 0 };
        int[] pivotIdx = { -1, -1, -1, -1 };

        // convert the matrix to an array for easy looping
        float[,] inverse = {{mat._values[0], mat._values[4], mat._values[8], mat._values[12]},
                            {mat._values[1], mat._values[5], mat._values[9], mat._values[13]},
                            {mat._values[2], mat._values[6], mat._values[10], mat._values[14]},
                            {mat._values[3], mat._values[7], mat._values[11], mat._values[15]} };
        int icol = 0;
        int irow = 0;
        for (int i = 0; i < 4; i++)
        {
            // Find the largest pivot value
            float maxPivot = 0.0f;
            for (int j = 0; j < 4; j++)
            {
                if (pivotIdx[j] != 0)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        if (pivotIdx[k] == -1)
                        {
                            float absVal = Math.Abs(inverse[j, k]);
                            if (absVal > maxPivot)
                            {
                                maxPivot = absVal;
                                irow = j;
                                icol = k;
                            }
                        }
                        else if (pivotIdx[k] > 0)
                        {
                            return mat;
                        }
                    }
                }
            }

            ++pivotIdx[icol];

            // Swap rows over so pivot is on diagonal
            if (irow != icol)
            {
                for (int k = 0; k < 4; ++k)
                {
                    float f = inverse[irow, k];
                    inverse[irow, k] = inverse[icol, k];
                    inverse[icol, k] = f;
                }
            }

            rowIdx[i] = irow;
            colIdx[i] = icol;

            float pivot = inverse[icol, icol];
            // check for singular matrix
            if (pivot == 0.0f)
            {
                throw new Exception("Matrix is singular and cannot be inverted.");
            }

            // Scale row so it has a unit diagonal
            float oneOverPivot = 1.0f / pivot;
            inverse[icol, icol] = 1.0f;
            for (int k = 0; k < 4; ++k)
            {
                inverse[icol, k] *= oneOverPivot;
            }

            // Do elimination of non-diagonal elements
            for (int j = 0; j < 4; ++j)
            {
                // check this isn't on the diagonal
                if (icol != j)
                {
                    float f = inverse[j, icol];
                    inverse[j, icol] = 0.0f;
                    for (int k = 0; k < 4; ++k)
                    {
                        inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }
        }

        for (int j = 3; j >= 0; --j)
        {
            int ir = rowIdx[j];
            int ic = colIdx[j];
            for (int k = 0; k < 4; ++k)
            {
                float f = inverse[k, ir];
                inverse[k, ir] = inverse[k, ic];
                inverse[k, ic] = f;
            }
        }

        return new Matrix4(
                            inverse[0, 0],
                            inverse[1, 0],
                            inverse[2, 0],
                            inverse[3, 0],
                            inverse[0, 1],
                            inverse[1, 1],
                            inverse[2, 1],
                            inverse[3, 1],
                            inverse[0, 2],
                            inverse[1, 2],
                            inverse[2, 2],
                            inverse[3, 2],
                            inverse[0, 3],
                            inverse[1, 3],
                            inverse[2, 3],
                            inverse[3, 3]
                           );
    }


    private float[] _values;

    /// <summary>Initialises a new <see cref="Matrix4"/> with individual column-major element values.</summary>
    public Matrix4(float m0, float m1, float m2, float m3, float m4, float m5, float m6, float m7, float m8, float m9, float m10, float m11, float m12, float m13, float m14, float m15)
    {
        _values = new float[16] { m0, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12, m13, m14, m15 };
    }

    /// <summary>Initialises a new <see cref="Matrix4"/> from a 16-element column-major array. The array is used directly — callers should clone if needed.</summary>
    public Matrix4(float[] values)
    {
        _values = values;
    }

    /// <summary>Initialises a new <see cref="Matrix4"/> as a deep copy of <paramref name="matrix"/>. The underlying element array is cloned so mutations to one instance do not affect the other.</summary>
    public Matrix4(Matrix4 matrix)
        : this()
    {
        _values = (float[])matrix.Values.Clone();
    }

    /// <summary>
    /// Gets or sets a matrix element by column (<paramref name="x"/>) and row (<paramref name="y"/>), both in the range [0, 3].
    /// </summary>
    /// <exception cref="Exception">Thrown when either index is out of range.</exception>
    public float this[int x, int y]
    {
        get
        {
            if (x < 0 || x > 3 || y < 0 || y > 3)
            {
                throw new Exception($"Requested an invalid Matrix4 index:{x},{y}");
            }

            return _values[x * 4 + y];
        }
        set
        {
            if (x < 0 || x > 3 || y < 0 || y > 3)
            {
                throw new Exception($"Requested an invalid Matrix4 index:{x},{y}");
            }

            _values[x * 4 + y] = value;
        }
    }

    /// <summary>
    /// Underlying array of matrix elements held by this matrix. Indexed as
    /// (0 4 8 12)
    /// (1 5 9 13)
    /// (2 6 10 14)
    /// (3 7 11 15)
    /// </summary>
    public float[] Values { get { return _values; } set { _values = value; } }

    /// <summary>Gets the determinant of this matrix.</summary>
    public float Determinant
    {
        get
        {
            return
                _values[0] * _values[5] * _values[10] * _values[15] - _values[0] * _values[5] * _values[14] * _values[11] + _values[0] * _values[9] * _values[14] * _values[7] - _values[0] * _values[9] * _values[6] * _values[15]
              + _values[0] * _values[13] * _values[6] * _values[11] - _values[0] * _values[13] * _values[10] * _values[7] - _values[4] * _values[9] * _values[14] * _values[3] + _values[4] * _values[9] * _values[2] * _values[15]
              - _values[4] * _values[13] * _values[2] * _values[11] + _values[4] * _values[13] * _values[10] * _values[3] - _values[4] * _values[1] * _values[10] * _values[15] + _values[4] * _values[1] * _values[14] * _values[11]
              + _values[8] * _values[13] * _values[2] * _values[7] - _values[8] * _values[13] * _values[6] * _values[3] + _values[8] * _values[1] * _values[6] * _values[15] - _values[8] * _values[1] * _values[14] * _values[7]
              + _values[8] * _values[5] * _values[14] * _values[3] - _values[8] * _values[5] * _values[2] * _values[15] - _values[12] * _values[1] * _values[6] * _values[11] + _values[12] * _values[1] * _values[10] * _values[7]
              - _values[12] * _values[5] * _values[10] * _values[3] + _values[12] * _values[5] * _values[2] * _values[11] - _values[12] * _values[9] * _values[2] * _values[7] + _values[12] * _values[9] * _values[6] * _values[3];
        }
    }

    /// <summary>Returns the transpose of this matrix — elements mirrored across the main diagonal. Does not modify this instance.</summary>
    public Matrix4 Transpose()
    {
        return new Matrix4
            (
                _values[0],
                _values[4],
                _values[8],
                _values[12],
                _values[1],
                _values[5],
                _values[9],
                _values[13],
                _values[2],
                _values[6],
                _values[10],
                _values[14],
                _values[3],
                _values[7],
                _values[11],
                _values[15]
            );
    }

    /// <summary>Returns the inverse of this matrix. Singular matrices throw an exception.</summary>
    /// <exception cref="Exception">Thrown when the matrix is singular and cannot be inverted.</exception>
    public Matrix4 Inverse()
    {
        return Invert(this);
    }

    /// <summary>Scalar multiplication.</summary>
    /// <param name="left">The scalar.</param>
    /// <param name="right">The matrix.</param>
    /// <returns>A new <see cref="Matrix4"/> with every element multiplied by <paramref name="left"/>.</returns>
    public static Matrix4 operator *(float left, Matrix4 right)
    {
        return Multiply(ref right, left);
    }

    /// <summary>Scalar multiplication.</summary>
    /// <param name="left">The matrix.</param>
    /// <param name="right">The scalar.</param>
    /// <returns>A new <see cref="Matrix4"/> with every element multiplied by <paramref name="right"/>.</returns>
    public static Matrix4 operator *(Matrix4 left, float right) => Multiply(ref left, right);

    /// <summary>Matrix multiplication.</summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>A new <see cref="Matrix4"/> holding the result of the multiplication.</returns>
    public static Matrix4 operator *(Matrix4 left, Matrix4 right)
    {
        return Multiply(ref left, ref right);
    }

    /// <summary>Transforms <paramref name="right"/> by <paramref name="left"/>.</summary>
    public static Point4 operator *(Matrix4 left, Point4 right) => Multiply(ref left, right);

    /// <summary>
    /// Transforms <paramref name="right"/> by <paramref name="left"/>. The point is padded to <c>(x, y, 0, 1)</c>;
    /// only the first two components of the result are returned as a <see cref="Point"/>.
    /// </summary>
    public static Point operator *(Matrix4 left, Point right) => Multiply(ref left, right);

    /// <summary>Matrix addition.</summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>A new <see cref="Matrix4"/> holding the component-wise sum.</returns>
    public static Matrix4 operator +(Matrix4 left, Matrix4 right) => Add(ref left, ref right);

    /// <summary>Matrix subtraction.</summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>A new <see cref="Matrix4"/> holding the component-wise difference.</returns>
    public static Matrix4 operator -(Matrix4 left, Matrix4 right) => Subtract(ref left, ref right);

    /// <summary>Returns a human-readable representation of this matrix with one row per line.</summary>
    public override string ToString()
    {
        return string.Format("({0} {4} {8} {12})\n({1} {5} {9} {13})\n({2} {6} {10} {14})\n({3} {7} {11} {15})\n", _values[0], _values[1], _values[2], _values[3], _values[4], _values[5], _values[6], _values[7], _values[8], _values[9], _values[10], _values[11], _values[12], _values[13], _values[14], _values[15]);
    }

}

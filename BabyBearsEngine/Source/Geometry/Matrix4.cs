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
        return new Matrix4(
            mat1._m0  + mat2._m0,  mat1._m1  + mat2._m1,  mat1._m2  + mat2._m2,  mat1._m3  + mat2._m3,
            mat1._m4  + mat2._m4,  mat1._m5  + mat2._m5,  mat1._m6  + mat2._m6,  mat1._m7  + mat2._m7,
            mat1._m8  + mat2._m8,  mat1._m9  + mat2._m9,  mat1._m10 + mat2._m10, mat1._m11 + mat2._m11,
            mat1._m12 + mat2._m12, mat1._m13 + mat2._m13, mat1._m14 + mat2._m14, mat1._m15 + mat2._m15);
    }

    /// <summary>Returns a new matrix whose elements are the component-wise difference of <paramref name="mat1"/> and <paramref name="mat2"/>.</summary>
    public static Matrix4 Subtract(ref Matrix4 mat1, ref Matrix4 mat2)
    {
        return new Matrix4(
            mat1._m0  - mat2._m0,  mat1._m1  - mat2._m1,  mat1._m2  - mat2._m2,  mat1._m3  - mat2._m3,
            mat1._m4  - mat2._m4,  mat1._m5  - mat2._m5,  mat1._m6  - mat2._m6,  mat1._m7  - mat2._m7,
            mat1._m8  - mat2._m8,  mat1._m9  - mat2._m9,  mat1._m10 - mat2._m10, mat1._m11 - mat2._m11,
            mat1._m12 - mat2._m12, mat1._m13 - mat2._m13, mat1._m14 - mat2._m14, mat1._m15 - mat2._m15);
    }

    /// <summary>Returns the matrix product <paramref name="mat1"/> × <paramref name="mat2"/>. Composition order: <paramref name="mat2"/> is applied first.</summary>
    public static Matrix4 Multiply(ref Matrix4 mat1, ref Matrix4 mat2)
    {
        return new Matrix4(
     /*0*/  mat1._m0*mat2._m0+mat1._m4*mat2._m1+mat1._m8*mat2._m2+mat1._m12*mat2._m3,
     /*1*/  mat1._m1*mat2._m0+mat1._m5*mat2._m1+mat1._m9*mat2._m2+mat1._m13*mat2._m3,
     /*2*/  mat1._m2*mat2._m0+mat1._m6*mat2._m1+mat1._m10*mat2._m2+mat1._m14*mat2._m3,
     /*3*/  mat1._m3*mat2._m0+mat1._m7*mat2._m1+mat1._m11*mat2._m2+mat1._m15*mat2._m3,
     /*4*/  mat1._m0*mat2._m4+mat1._m4*mat2._m5+mat1._m8*mat2._m6+mat1._m12*mat2._m7,
     /*5*/  mat1._m1*mat2._m4+mat1._m5*mat2._m5+mat1._m9*mat2._m6+mat1._m13*mat2._m7,
     /*6*/  mat1._m2*mat2._m4+mat1._m6*mat2._m5+mat1._m10*mat2._m6+mat1._m14*mat2._m7,
     /*7*/  mat1._m3*mat2._m4+mat1._m7*mat2._m5+mat1._m11*mat2._m6+mat1._m15*mat2._m7,
     /*8*/  mat1._m0*mat2._m8+mat1._m4*mat2._m9+mat1._m8*mat2._m10+mat1._m12*mat2._m11,
     /*9*/  mat1._m1*mat2._m8+mat1._m5*mat2._m9+mat1._m9*mat2._m10+mat1._m13*mat2._m11,
    /*10*/  mat1._m2*mat2._m8+mat1._m6*mat2._m9+mat1._m10*mat2._m10+mat1._m14*mat2._m11,
    /*11*/  mat1._m3*mat2._m8+mat1._m7*mat2._m9+mat1._m11*mat2._m10+mat1._m15*mat2._m11,
    /*12*/  mat1._m0*mat2._m12+mat1._m4*mat2._m13+mat1._m8*mat2._m14+mat1._m12*mat2._m15,
    /*13*/  mat1._m1*mat2._m12+mat1._m5*mat2._m13+mat1._m9*mat2._m14+mat1._m13*mat2._m15,
    /*14*/  mat1._m2*mat2._m12+mat1._m6*mat2._m13+mat1._m10*mat2._m14+mat1._m14*mat2._m15,
    /*15*/  mat1._m3*mat2._m12+mat1._m7*mat2._m13+mat1._m11*mat2._m14+mat1._m15*mat2._m15
        );
    }

    /// <summary>Returns a new matrix with every element of <paramref name="mat"/> multiplied by the scalar <paramref name="f"/>.</summary>
    public static Matrix4 Multiply(ref Matrix4 mat, float f)
    {
        return new Matrix4
            (
                mat._m0  * f, mat._m1  * f, mat._m2  * f, mat._m3  * f,
                mat._m4  * f, mat._m5  * f, mat._m6  * f, mat._m7  * f,
                mat._m8  * f, mat._m9  * f, mat._m10 * f, mat._m11 * f,
                mat._m12 * f, mat._m13 * f, mat._m14 * f, mat._m15 * f
            );
    }

    /// <summary>Transforms <paramref name="p"/> by <paramref name="mat"/>, returning a new <see cref="Point4"/>.</summary>
    public static Point4 Multiply(ref Matrix4 mat, Point4 p)
    {
        return new Point4
            (
                mat._m0 * p.x + mat._m4 * p.y + mat._m8  * p.z + mat._m12 * p.w,
                mat._m1 * p.x + mat._m5 * p.y + mat._m9  * p.z + mat._m13 * p.w,
                mat._m2 * p.x + mat._m6 * p.y + mat._m10 * p.z + mat._m14 * p.w,
                mat._m3 * p.x + mat._m7 * p.y + mat._m11 * p.z + mat._m15 * p.w
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
                mat._m0 * p.X + mat._m4 * p.Y + mat._m12,
                mat._m1 * p.X + mat._m5 * p.Y + mat._m13
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
        var scale = CreateScale(scaleX, scaleY, 1);
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
        float[,] inverse = {{mat._m0, mat._m4, mat._m8,  mat._m12},
                            {mat._m1, mat._m5, mat._m9,  mat._m13},
                            {mat._m2, mat._m6, mat._m10, mat._m14},
                            {mat._m3, mat._m7, mat._m11, mat._m15}};
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


    private float _m0  = 0f;
    private float _m1  = 0f;
    private float _m2  = 0f;
    private float _m3  = 0f;
    private float _m4  = 0f;
    private float _m5  = 0f;
    private float _m6  = 0f;
    private float _m7  = 0f;
    private float _m8  = 0f;
    private float _m9  = 0f;
    private float _m10 = 0f;
    private float _m11 = 0f;
    private float _m12 = 0f;
    private float _m13 = 0f;
    private float _m14 = 0f;
    private float _m15 = 0f;

    /// <summary>Initialises a new <see cref="Matrix4"/> with individual column-major element values.</summary>
    public Matrix4(float m0, float m1, float m2, float m3, float m4, float m5, float m6, float m7, float m8, float m9, float m10, float m11, float m12, float m13, float m14, float m15)
    {
        _m0  = m0;  _m1  = m1;  _m2  = m2;  _m3  = m3;
        _m4  = m4;  _m5  = m5;  _m6  = m6;  _m7  = m7;
        _m8  = m8;  _m9  = m9;  _m10 = m10; _m11 = m11;
        _m12 = m12; _m13 = m13; _m14 = m14; _m15 = m15;
    }

    /// <summary>Initialises a new <see cref="Matrix4"/> from a 16-element column-major array. Each element is copied — mutations to the array after construction do not affect this matrix.</summary>
    public Matrix4(float[] values)
    {
        _m0  = values[0];  _m1  = values[1];  _m2  = values[2];  _m3  = values[3];
        _m4  = values[4];  _m5  = values[5];  _m6  = values[6];  _m7  = values[7];
        _m8  = values[8];  _m9  = values[9];  _m10 = values[10]; _m11 = values[11];
        _m12 = values[12]; _m13 = values[13]; _m14 = values[14]; _m15 = values[15];
    }

    /// <summary>Initialises a new <see cref="Matrix4"/> as a copy of <paramref name="matrix"/>. Named fields are value types — no aliasing is possible.</summary>
    public Matrix4(Matrix4 matrix)
    {
        _m0  = matrix._m0;  _m1  = matrix._m1;  _m2  = matrix._m2;  _m3  = matrix._m3;
        _m4  = matrix._m4;  _m5  = matrix._m5;  _m6  = matrix._m6;  _m7  = matrix._m7;
        _m8  = matrix._m8;  _m9  = matrix._m9;  _m10 = matrix._m10; _m11 = matrix._m11;
        _m12 = matrix._m12; _m13 = matrix._m13; _m14 = matrix._m14; _m15 = matrix._m15;
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

            return (x * 4 + y) switch
            {
                0  => _m0,  1  => _m1,  2  => _m2,  3  => _m3,
                4  => _m4,  5  => _m5,  6  => _m6,  7  => _m7,
                8  => _m8,  9  => _m9,  10 => _m10, 11 => _m11,
                12 => _m12, 13 => _m13, 14 => _m14, 15 => _m15,
                _ => throw new Exception($"Requested an invalid Matrix4 index:{x},{y}")
            };
        }
        set
        {
            if (x < 0 || x > 3 || y < 0 || y > 3)
            {
                throw new Exception($"Requested an invalid Matrix4 index:{x},{y}");
            }

            switch (x * 4 + y)
            {
                case 0:  _m0  = value; break;
                case 1:  _m1  = value; break;
                case 2:  _m2  = value; break;
                case 3:  _m3  = value; break;
                case 4:  _m4  = value; break;
                case 5:  _m5  = value; break;
                case 6:  _m6  = value; break;
                case 7:  _m7  = value; break;
                case 8:  _m8  = value; break;
                case 9:  _m9  = value; break;
                case 10: _m10 = value; break;
                case 11: _m11 = value; break;
                case 12: _m12 = value; break;
                case 13: _m13 = value; break;
                case 14: _m14 = value; break;
                case 15: _m15 = value; break;
            }
        }
    }

    /// <summary>
    /// Returns the matrix elements as a new array in column-major order, indexed as
    /// (0 4 8 12)
    /// (1 5 9 13)
    /// (2 6 10 14)
    /// (3 7 11 15)
    /// A new array is allocated on each access; the caller may freely mutate it.
    /// </summary>
    public float[] Values => new float[]
    {
        _m0,  _m1,  _m2,  _m3,
        _m4,  _m5,  _m6,  _m7,
        _m8,  _m9,  _m10, _m11,
        _m12, _m13, _m14, _m15
    };

    /// <summary>
    /// Writes the 16 matrix elements in column-major order into <paramref name="destination"/>
    /// without allocating — the non-allocating counterpart to <see cref="Values"/> for hot paths
    /// such as per-draw GL uniform uploads.
    /// </summary>
    /// <param name="destination">Receives the elements; must be at least 16 long.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is shorter than 16.</exception>
    public readonly void WriteTo(Span<float> destination)
    {
        if (destination.Length < 16)
        {
            throw new ArgumentException("Destination must be at least 16 elements long.", nameof(destination));
        }

        destination[0] = _m0;
        destination[1] = _m1;
        destination[2] = _m2;
        destination[3] = _m3;
        destination[4] = _m4;
        destination[5] = _m5;
        destination[6] = _m6;
        destination[7] = _m7;
        destination[8] = _m8;
        destination[9] = _m9;
        destination[10] = _m10;
        destination[11] = _m11;
        destination[12] = _m12;
        destination[13] = _m13;
        destination[14] = _m14;
        destination[15] = _m15;
    }

    /// <summary>Gets the determinant of this matrix.</summary>
    public float Determinant
    {
        get
        {
            return
                _m0 * _m5 * _m10 * _m15 - _m0 * _m5 * _m14 * _m11 + _m0 * _m9 * _m14 * _m7 - _m0 * _m9 * _m6 * _m15
              + _m0 * _m13 * _m6 * _m11 - _m0 * _m13 * _m10 * _m7 - _m4 * _m9 * _m14 * _m3 + _m4 * _m9 * _m2 * _m15
              - _m4 * _m13 * _m2 * _m11 + _m4 * _m13 * _m10 * _m3 - _m4 * _m1 * _m10 * _m15 + _m4 * _m1 * _m14 * _m11
              + _m8 * _m13 * _m2 * _m7  - _m8 * _m13 * _m6 * _m3  + _m8 * _m1 * _m6 * _m15  - _m8 * _m1 * _m14 * _m7
              + _m8 * _m5 * _m14 * _m3  - _m8 * _m5 * _m2 * _m15  - _m12 * _m1 * _m6 * _m11 + _m12 * _m1 * _m10 * _m7
              - _m12 * _m5 * _m10 * _m3 + _m12 * _m5 * _m2 * _m11 - _m12 * _m9 * _m2 * _m7  + _m12 * _m9 * _m6 * _m3;
        }
    }

    /// <summary>Returns the transpose of this matrix — elements mirrored across the main diagonal. Does not modify this instance.</summary>
    public Matrix4 Transpose()
    {
        return new Matrix4
            (
                _m0,  _m4,  _m8,  _m12,
                _m1,  _m5,  _m9,  _m13,
                _m2,  _m6,  _m10, _m14,
                _m3,  _m7,  _m11, _m15
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
        return $"({_m0} {_m4} {_m8} {_m12})\n({_m1} {_m5} {_m9} {_m13})\n({_m2} {_m6} {_m10} {_m14})\n({_m3} {_m7} {_m11} {_m15})\n";
    }

}

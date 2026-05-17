namespace BabyBearsEngine.Geometry;

/// <summary>
/// A 3×3 matrix used for 2D affine transformations (translation, rotation, scale, shear, projection).
/// Elements are stored column-major in <see cref="Values"/>:
/// <code>
/// (0  3  6)
/// (1  4  7)
/// (2  5  8)
/// </code>
/// Multiplication composes transforms: <c>a * b</c> applies <c>b</c> first then <c>a</c>.
/// Most static helpers take <c>ref Matrix3</c> for performance — they return new matrices and never mutate the inputs.
/// </summary>
public struct Matrix3
{
    /// <summary>The identity matrix — leaves any vector or matrix unchanged when multiplied. Returns a fresh instance per access (the underlying array is not shared).</summary>
    public static Matrix3 Identity => new(1, 0, 0, 0, 1, 0, 0, 0, 1);

    /// <summary>The zero matrix — all elements are zero. Returns a fresh instance per access.</summary>
    public static Matrix3 Zero => new(0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>A reflection across the Y axis (negates X). Returns a fresh instance per access.</summary>
    public static Matrix3 FlipXMatrix => new(-1, 0, 0, 0, 1, 0, 0, 0, 1);

    /// <summary>A reflection across the X axis (negates Y). Returns a fresh instance per access.</summary>
    public static Matrix3 FlipYMatrix => new(1, 0, 0, 0, -1, 0, 0, 0, 1);

    /// <summary>Returns a translation matrix that moves points by (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public static Matrix3 CreateTranslation(float x, float y) => new(1, 0, 0, 0, 1, 0, x, y, 1);

    /// <summary>Returns a rotation matrix that rotates points around the Z axis by <paramref name="angleInDegrees"/> (counter-clockwise in a right-handed coordinate system).</summary>
    public static Matrix3 CreateRotationAroundZAxis(float angleInDegrees)
    {
        double radians = Math.PI * angleInDegrees / 180.0;
        return new Matrix3((float)Math.Cos(radians), (float)Math.Sin(radians), 0, -(float)Math.Sin(radians), (float)Math.Cos(radians), 0, 0, 0, 1);
    }

    /// <summary>Returns a scale matrix that scales X by <paramref name="scaleX"/> and Y by <paramref name="scaleY"/>.</summary>
    public static Matrix3 CreateScale(float scaleX, float scaleY) => new(scaleX, 0, 0, 0, scaleY, 0, 0, 0, 1);

    /// <summary>
    /// Returns an orthographic projection matrix mapping a (<paramref name="width"/>, <paramref name="height"/>) screen-space rectangle
    /// (top-left origin, Y down) onto NDC (-1..1, Y up). Used to render directly to the screen.
    /// </summary>
    public static Matrix3 CreateOrtho(float width, float height)
    {
        var mat = Identity;
        mat = ScaleAroundOrigin(ref mat, 2 / width, 2 / height);
        mat = FlipY(ref mat);
        mat = Translate(ref mat, -width / 2, -height / 2);
        return mat;
    }

    /// <summary>
    /// Same as <see cref="CreateOrtho"/> but without the Y flip. Use this when rendering to a framebuffer texture
    /// rather than directly to the screen, since the framebuffer's Y axis already matches OpenGL's convention.
    /// </summary>
    public static Matrix3 CreateFBOOrtho(float width, float height)
    {
        var mat = CreateScale(2 / width, 2 / height);
        mat = Translate(ref mat, -width / 2, -height / 2);
        return mat;
    }

    /// <summary>Returns a new matrix whose elements are the component-wise sum of <paramref name="mat1"/> and <paramref name="mat2"/>.</summary>
    public static Matrix3 Add(ref Matrix3 mat1, ref Matrix3 mat2)
    {
        return new Matrix3(
            mat1._m0 + mat2._m0, mat1._m1 + mat2._m1, mat1._m2 + mat2._m2,
            mat1._m3 + mat2._m3, mat1._m4 + mat2._m4, mat1._m5 + mat2._m5,
            mat1._m6 + mat2._m6, mat1._m7 + mat2._m7, mat1._m8 + mat2._m8);
    }

    /// <summary>Returns a new matrix whose elements are the component-wise difference of <paramref name="mat1"/> and <paramref name="mat2"/>.</summary>
    public static Matrix3 Subtract(ref Matrix3 mat1, ref Matrix3 mat2)
    {
        return new Matrix3(
            mat1._m0 - mat2._m0, mat1._m1 - mat2._m1, mat1._m2 - mat2._m2,
            mat1._m3 - mat2._m3, mat1._m4 - mat2._m4, mat1._m5 - mat2._m5,
            mat1._m6 - mat2._m6, mat1._m7 - mat2._m7, mat1._m8 - mat2._m8);
    }

    /// <summary>Returns the matrix product <paramref name="mat1"/> × <paramref name="mat2"/>. Composition order: <paramref name="mat2"/> is applied first.</summary>
    public static Matrix3 Multiply(ref Matrix3 mat1, ref Matrix3 mat2)
    {
        return new Matrix3
            (
                mat1._m0 * mat2._m0 + mat1._m3 * mat2._m1 + mat1._m6 * mat2._m2,
                mat1._m1 * mat2._m0 + mat1._m4 * mat2._m1 + mat1._m7 * mat2._m2,
                mat1._m2 * mat2._m0 + mat1._m5 * mat2._m1 + mat1._m8 * mat2._m2,
                mat1._m0 * mat2._m3 + mat1._m3 * mat2._m4 + mat1._m6 * mat2._m5,
                mat1._m1 * mat2._m3 + mat1._m4 * mat2._m4 + mat1._m7 * mat2._m5,
                mat1._m2 * mat2._m3 + mat1._m5 * mat2._m4 + mat1._m8 * mat2._m5,
                mat1._m0 * mat2._m6 + mat1._m3 * mat2._m7 + mat1._m6 * mat2._m8,
                mat1._m1 * mat2._m6 + mat1._m4 * mat2._m7 + mat1._m7 * mat2._m8,
                mat1._m2 * mat2._m6 + mat1._m5 * mat2._m7 + mat1._m8 * mat2._m8
            );
    }

    /// <summary>
    /// Transforms a 2D <see cref="Point"/>. The point is padded to <c>(x, y, 1)</c> before multiplication so translation is applied;
    /// only the first two components of the result are returned.
    /// </summary>
    public static Point Multiply(ref Matrix3 mat, Point p)
    {
        return new Point
            (
                mat._m0 * p.X + mat._m3 * p.Y + mat._m6,
                mat._m1 * p.X + mat._m4 * p.Y + mat._m7
            );
    }

    /// <summary>Transforms a homogeneous 3D <see cref="Point3"/> by this matrix.</summary>
    public static Point3 Multiply(ref Matrix3 mat, Point3 p)
    {
        return new Point3
            (
                mat._m0 * p.X + mat._m3 * p.Y + mat._m6 * p.Z,
                mat._m1 * p.X + mat._m4 * p.Y + mat._m7 * p.Z,
                mat._m2 * p.X + mat._m5 * p.Y + mat._m8 * p.Z
            );
    }

    /// <summary>Returns a new matrix with every element scaled by <paramref name="f"/>.</summary>
    public static Matrix3 Multiply(ref Matrix3 mat, float f)
    {
        return new Matrix3
            (
                mat._m0 * f, mat._m1 * f, mat._m2 * f,
                mat._m3 * f, mat._m4 * f, mat._m5 * f,
                mat._m6 * f, mat._m7 * f, mat._m8 * f
            );
    }

    /// <summary>Returns <paramref name="mat"/> composed with a translation by (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public static Matrix3 Translate(ref Matrix3 mat, float x, float y)
    {
        var transMat = CreateTranslation(x, y);
        return Multiply(ref mat, ref transMat);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a rotation around the origin by <paramref name="angleInDegrees"/>.</summary>
    public static Matrix3 RotateAroundZ(ref Matrix3 mat, float angleInDegrees)
    {
        var rotMat = CreateRotationAroundZAxis(angleInDegrees);

        return mat * rotMat;
    }

    /// <summary>Returns <paramref name="mat"/> composed with a rotation by <paramref name="angleInDegrees"/> centred on (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public static Matrix3 RotateAroundPoint(ref Matrix3 mat, float angleInDegrees, float x, float y)
    {
        var translateToOrigin = CreateTranslation(x, y);
        var rotateAroundOrigin = CreateRotationAroundZAxis(angleInDegrees);
        var translateBack = CreateTranslation(-x, -y);

        return mat * translateToOrigin * rotateAroundOrigin * translateBack;
    }

    /// <summary>Returns <paramref name="mat"/> composed with a rotation by <paramref name="angleInDegrees"/> centred on <paramref name="p"/>.</summary>
    public static Matrix3 RotateAroundPoint(ref Matrix3 mat, float angleInDegrees, Point p) => RotateAroundPoint(ref mat, angleInDegrees, p.X, p.Y);

    /// <summary>Returns <paramref name="mat"/> composed with a scale around the origin.</summary>
    public static Matrix3 ScaleAroundOrigin(ref Matrix3 mat, float scaleX, float scaleY)
    {
        var scaleMat = CreateScale(scaleX, scaleY);
        return Multiply(ref mat, ref scaleMat);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a scale centred on (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public static Matrix3 ScaleAroundPoint(ref Matrix3 mat, float scaleX, float scaleY, float x, float y)
    {
        var translate1 = CreateTranslation(x, y);
        var scale = CreateScale(scaleX, scaleY);
        var translate2 = CreateTranslation(-x, -y);

        var result = Multiply(ref mat, ref translate1);
        result = Multiply(ref result, ref scale);
        result = Multiply(ref result, ref translate2);

        return result;
    }

    /// <summary>Returns <paramref name="mat"/> composed with a reflection across the Y axis (negates X).</summary>
    public static Matrix3 FlipX(ref Matrix3 mat)
    {
        var flip = FlipXMatrix;
        return Multiply(ref mat, ref flip);
    }

    /// <summary>Returns <paramref name="mat"/> composed with a reflection across the X axis (negates Y).</summary>
    public static Matrix3 FlipY(ref Matrix3 mat)
    {
        var flip = FlipYMatrix;
        return Multiply(ref mat, ref flip);
    }

    /// <summary>Returns the inverse of <paramref name="mat"/>.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the matrix is singular (has no inverse).</exception>
    public static Matrix3 Invert(Matrix3 mat)
    {
        int[] colIdx = { 0, 0, 0 };
        int[] rowIdx = { 0, 0, 0 };
        int[] pivotIdx = { -1, -1, -1 };

        float[,] inverse = {{mat._m0, mat._m3, mat._m6},
                            {mat._m1, mat._m4, mat._m7},
                            {mat._m2, mat._m5, mat._m8}};

        int icol = 0;
        int irow = 0;
        for (int i = 0; i < 3; i++)
        {
            float maxPivot = 0.0f;
            for (int j = 0; j < 3; j++)
            {
                if (pivotIdx[j] != 0)
                {
                    for (int k = 0; k < 3; ++k)
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

            if (irow != icol)
            {
                for (int k = 0; k < 3; ++k)
                {
                    float f = inverse[irow, k];
                    inverse[irow, k] = inverse[icol, k];
                    inverse[icol, k] = f;
                }
            }

            rowIdx[i] = irow;
            colIdx[i] = icol;

            float pivot = inverse[icol, icol];

            if (pivot == 0.0f)
            {
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
            }

            float oneOverPivot = 1.0f / pivot;
            inverse[icol, icol] = 1.0f;
            for (int k = 0; k < 3; ++k)
            {
                inverse[icol, k] *= oneOverPivot;
            }

            for (int j = 0; j < 3; ++j)
            {
                if (icol != j)
                {
                    float f = inverse[j, icol];
                    inverse[j, icol] = 0.0f;
                    for (int k = 0; k < 3; ++k)
                    {
                        inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }
        }

        for (int j = 2; j >= 0; --j)
        {
            int ir = rowIdx[j];
            int ic = colIdx[j];
            for (int k = 0; k < 3; ++k)
            {
                float f = inverse[k, ir];
                inverse[k, ir] = inverse[k, ic];
                inverse[k, ic] = f;
            }
        }

        return new Matrix3
            (
                inverse[0, 0],
                inverse[1, 0],
                inverse[2, 0],
                inverse[0, 1],
                inverse[1, 1],
                inverse[2, 1],
                inverse[0, 2],
                inverse[1, 2],
                inverse[2, 2]
            );
    }

    /// <summary>Returns the inverse of <paramref name="mat"/>. Pass-by-reference variant for performance.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the matrix is singular (has no inverse).</exception>
    public static Matrix3 Invert(ref Matrix3 mat)
    {
        int[] colIdx = { 0, 0, 0 };
        int[] rowIdx = { 0, 0, 0 };
        int[] pivotIdx = { -1, -1, -1 };

        float[,] inverse = {{mat._m0, mat._m3, mat._m6},
                            {mat._m1, mat._m4, mat._m7},
                            {mat._m2, mat._m5, mat._m8}};

        int icol = 0;
        int irow = 0;
        for (int i = 0; i < 3; i++)
        {
            float maxPivot = 0.0f;
            for (int j = 0; j < 3; j++)
            {
                if (pivotIdx[j] != 0)
                {
                    for (int k = 0; k < 3; ++k)
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

            if (irow != icol)
            {
                for (int k = 0; k < 3; ++k)
                {
                    float f = inverse[irow, k];
                    inverse[irow, k] = inverse[icol, k];
                    inverse[icol, k] = f;
                }
            }

            rowIdx[i] = irow;
            colIdx[i] = icol;

            float pivot = inverse[icol, icol];

            if (pivot == 0.0f)
            {
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
            }

            float oneOverPivot = 1.0f / pivot;
            inverse[icol, icol] = 1.0f;
            for (int k = 0; k < 3; ++k)
            {
                inverse[icol, k] *= oneOverPivot;
            }

            for (int j = 0; j < 3; ++j)
            {
                if (icol != j)
                {
                    float f = inverse[j, icol];
                    inverse[j, icol] = 0.0f;
                    for (int k = 0; k < 3; ++k)
                    {
                        inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }
        }

        for (int j = 2; j >= 0; --j)
        {
            int ir = rowIdx[j];
            int ic = colIdx[j];
            for (int k = 0; k < 3; ++k)
            {
                float f = inverse[k, ir];
                inverse[k, ir] = inverse[k, ic];
                inverse[k, ic] = f;
            }
        }

        return new Matrix3
            (
                inverse[0, 0],
                inverse[1, 0],
                inverse[2, 0],
                inverse[0, 1],
                inverse[1, 1],
                inverse[2, 1],
                inverse[0, 2],
                inverse[1, 2],
                inverse[2, 2]
            );
    }

    private float _m0 = 0f;
    private float _m1 = 0f;
    private float _m2 = 0f;
    private float _m3 = 0f;
    private float _m4 = 0f;
    private float _m5 = 0f;
    private float _m6 = 0f;
    private float _m7 = 0f;
    private float _m8 = 0f;

    /// <summary>Creates a matrix from 9 elements in column-major order (see <see cref="Matrix3"/> for layout).</summary>
    public Matrix3(float m0, float m1, float m2, float m3, float m4, float m5, float m6, float m7, float m8)
    {
        _m0 = m0; _m1 = m1; _m2 = m2;
        _m3 = m3; _m4 = m4; _m5 = m5;
        _m6 = m6; _m7 = m7; _m8 = m8;
    }

    /// <summary>Creates a matrix from a 9-element array in column-major order. Each element is copied — mutations to the array after construction do not affect this matrix.</summary>
    public Matrix3(float[] values)
    {
        _m0 = values[0]; _m1 = values[1]; _m2 = values[2];
        _m3 = values[3]; _m4 = values[4]; _m5 = values[5];
        _m6 = values[6]; _m7 = values[7]; _m8 = values[8];
    }

    /// <summary>Indexer accessing element at column <paramref name="x"/>, row <paramref name="y"/> (0..2 for both).</summary>
    /// <exception cref="ArgumentException">Thrown when either index is outside 0..2.</exception>
    public float this[int x, int y]
    {
        get
        {
            if (x < 0 || x > 2 || y < 0 || y > 2)
            {
                throw new ArgumentException($"Requested an invalid Matrix3 index:{x},{y}");
            }

            return (x * 3 + y) switch
            {
                0 => _m0, 1 => _m1, 2 => _m2,
                3 => _m3, 4 => _m4, 5 => _m5,
                6 => _m6, 7 => _m7, 8 => _m8,
                _ => throw new ArgumentException($"Requested an invalid Matrix3 index:{x},{y}")
            };
        }
        set
        {
            if (x < 0 || x > 2 || y < 0 || y > 2)
            {
                throw new ArgumentException($"Requested an invalid Matrix3 index:{x},{y}");
            }

            switch (x * 3 + y)
            {
                case 0: _m0 = value; break;
                case 1: _m1 = value; break;
                case 2: _m2 = value; break;
                case 3: _m3 = value; break;
                case 4: _m4 = value; break;
                case 5: _m5 = value; break;
                case 6: _m6 = value; break;
                case 7: _m7 = value; break;
                case 8: _m8 = value; break;
            }
        }
    }

    /// <summary>
    /// Returns the matrix elements as a new array in column-major order, indexed as
    /// (0 3 6)
    /// (1 4 7)
    /// (2 5 8)
    /// A new array is allocated on each access; the caller may freely mutate it.
    /// </summary>
    public float[] Values => new float[] { _m0, _m1, _m2, _m3, _m4, _m5, _m6, _m7, _m8 };

    /// <summary>
    /// Gets the determinant of this matrix
    /// </summary>
    public float Determinant
    {
        get
        {
            return _m0 * _m4 * _m8 + _m3 * _m7 * _m2 + _m6 * _m1 * _m5
                 - _m6 * _m4 * _m2 - _m0 * _m7 * _m5 - _m3 * _m1 * _m8;
        }
    }

    /// <summary>
    ///  Returns the transpose of this Matrix3 - all elements mirrored in [1 1] diagonal. The values of this instance will not be altered, returns a new matrix.
    /// </summary>
    /// <returns></returns>
    public Matrix3 Transpose()
    {
        return new Matrix3
            (
                _m0, _m3, _m6,
                _m1, _m4, _m7,
                _m2, _m5, _m8
            );
    }

    /// <summary>
    /// Return a new Matrix that is the inverse of this matrix- singular matrices will throw an exception
    /// </summary>
    /// <returns></returns>
    public Matrix3 Inverse()
    {
        return Invert(this);
    }

    /// <summary>
    /// Scalar multiplication.
    /// </summary>
    /// <param name="left">left-hand operand</param>
    /// <param name="right">right-hand operand</param>
    /// <returns>A new Matrix2 which holds the result of the multiplication</returns>
    public static Matrix3 operator *(float left, Matrix3 right) => Multiply(ref right, left);

    /// <summary>
    /// Scalar multiplication.
    /// </summary>
    /// <param name="left">left-hand operand</param>
    /// <param name="right">right-hand operand</param>
    /// <returns>A new Matrix3 which holds the result of the multiplication</returns>
    public static Matrix3 operator *(Matrix3 left, float right) => Multiply(ref left, right);

    /// <summary>
    /// Matrix multiplication
    /// </summary>
    /// <param name="left">left-hand operand</param>
    /// <param name="right">right-hand operand</param>
    /// <returns>A new Matrix3 which holds the result of the multiplication</returns>
    public static Matrix3 operator *(Matrix3 left, Matrix3 right) => Multiply(ref left, ref right);

    /// <summary>
    /// Multiplying a matrix onto a Point, to return a point
    /// </summary>
    public static Point3 operator *(Matrix3 left, Point3 right) => Multiply(ref left, right);

    /// <summary>
    /// Multiplying a matrix onto a Point, to return a point
    /// </summary>
    public static Point operator *(Matrix3 left, Point right) => Multiply(ref left, right);

    /// <summary>
    /// Matrix addition
    /// </summary>
    /// <param name="left">left-hand operand</param>
    /// <param name="right">right-hand operand</param>
    /// <returns>A new Matrix3 which holds the result of the addition</returns>
    public static Matrix3 operator +(Matrix3 left, Matrix3 right) => Add(ref left, ref right);

    /// <summary>
    /// Matrix subtraction
    /// </summary>
    /// <param name="left">left-hand operand</param>
    /// <param name="right">right-hand operand</param>
    /// <returns>A new Matrix3 which holds the result of the subtraction</returns>
    public static Matrix3 operator -(Matrix3 left, Matrix3 right) => Subtract(ref left, ref right);


}

using System.Runtime.InteropServices;

namespace BabyBearsEngine.Geometry;

/// <summary>
/// A four-component vector (X, Y, Z, W) used for 3D homogeneous coordinates and matrix transformation.
/// </summary>
/// <remarks>Initialises a new <see cref="Point4"/> with the given components.</remarks>
[StructLayout(LayoutKind.Sequential)]
public record struct Point4(float X, float Y, float Z, float W)
{
    /// <summary>A <see cref="Point4"/> with all components set to zero.</summary>
    public static readonly Point4 Zero = new();

    /// <summary>Returns the dot product (scalar product) of <paramref name="p1"/> and <paramref name="p2"/>.</summary>
    public static float DotProduct(Point4 p1, Point4 p2)
    {
        return p1.DotProduct(p2);
    }

    /// <summary>Gets the Euclidean length (magnitude) of this vector.</summary>
    public readonly float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

    /// <summary>Gets the squared Euclidean length of this vector. Cheaper than <see cref="Length"/> when only relative comparisons are needed.</summary>
    public readonly float LengthSquared => X * X + Y * Y + Z * Z + W * W;

    /// <summary>
    /// Returns a copy this Point, but with magnitude 1. Does not modify this Point.
    /// </summary>
    public readonly Point4 Normal
    {
        get
        {
            if (X == 0 && Y == 0 && Z == 0 && W == 0)
            {
                return new Point4();
            }

            float length = Length;

            return new Point4(X / length, Y / length, Z / length, W / length);
        }
    }


    /// <summary>
    /// Normalize this point4 - set it to have magnitude 1. If it's length is zero, it will be set to (0,0,0,0).
    /// </summary>
    public void Normalize()
    {
        float length = Length;
        if (length == 0)
        {
            X = 0;
            Y = 0;
            Z = 0;
            W = 0;
            return;
        }
        X /= length;
        Y /= length;
        Z /= length;
        W /= length;
    }


    /// <summary>
    /// Preserves direction of the point4 but clamps its magnitude to below maxLength
    /// </summary>
    /// <param name="maxLength"></param>
    public Point4 Clamp(float maxLength)
    {
        float length = Length;

        if (length > maxLength)
        {
            X = X * maxLength / length;
            Y = Y * maxLength / length;
            Z = Z * maxLength / length;
            W = W * maxLength / length;
        }

        return this;
    }


    /// <summary>
    /// Returns dot product (scalar product) with another point
    /// </summary>
    public readonly float DotProduct(Point4 other)
    {
        return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
    }


    /// <summary>
    /// Apply Matrix4 transformMatrix to this Point4 to result in a new Point4. This Point4 instance is not modified - a new one is returned.
    /// </summary>
    /// <param name="transformMatrix"></param>
    /// <returns></returns>
    public readonly Point4 Transform(Matrix4 transformMatrix)
    {
        return Matrix4.Multiply(ref transformMatrix, this);
    }

    /// <summary>
    /// Apply Matrix4 transformMatrix to this Point4 to result in a new Point4. This Point4 instance is not modified - a new one is returned.
    /// </summary>
    /// <param name="transformMatrix"></param>
    /// <returns></returns>
    public readonly Point4 Transform(ref Matrix4 transformMatrix)
    {
        return Matrix4.Multiply(ref transformMatrix, this);
    }


    /// <summary>Returns a <see cref="Point"/> containing the X and Y components of this vector.</summary>
    public readonly Point ToPoint() => new(X, Y);

    /// <summary>Returns a <see cref="Point3"/> containing the X, Y, and Z components of this vector.</summary>
    public readonly Point3 ToPoint3() => new(X, Y, Z);

    /// <summary>Returns a new array containing the four components in order: [X, Y, Z, W].</summary>
    public readonly float[] ToArray() => [X, Y, Z, W];

    /// <summary>Component-wise addition.</summary>
    public static Point4 operator +(Point4 p1, Point4 p2) { return new Point4(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z, p1.W + p2.W); }

    /// <summary>Component-wise subtraction.</summary>
    public static Point4 operator -(Point4 p1, Point4 p2) { return new Point4(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z, p1.W - p2.W); }

    /// <summary>Scalar multiplication (scalar on left).</summary>
    public static Point4 operator *(float f, Point4 p) { return new Point4(p.X * f, p.Y * f, p.Z * f, p.W * f); }

    /// <summary>Scalar multiplication (scalar on right).</summary>
    public static Point4 operator *(Point4 p, float f) { return new Point4(p.X * f, p.Y * f, p.Z * f, p.W * f); }

    /// <summary>Scalar division.</summary>
    public static Point4 operator /(Point4 p, float f) { return new Point4(p.X / f, p.Y / f, p.Z / f, p.W / f); }

    /// <summary>Returns a string representation of this vector in the form <c>(X:x,Y:y,Z:z,W:w)</c>.</summary>
    public override readonly string ToString() => FormattableString.Invariant($"(X:{X},Y:{Y},Z:{Z},W:{W})");
}

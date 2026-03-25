using System.Runtime.InteropServices;

namespace BabyBearsEngine.Source.Geometry;

/// <summary>
/// Represents a 3-dimensional point or vector with <c>X</c>, <c>Y</c> and <c>Z</c> components.
/// Provides common vector operations such as dot/cross products, normalization and basic arithmetic.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Point3(float x, float y, float z) : IEquatable<Point3>
{
    /// <summary>
    /// A <see cref="Point3"/> with all components set to zero.
    /// </summary>
    public static readonly Point3 Zero = new();

    /// <summary>
    /// Computes the scalar (dot) product of two <see cref="Point3"/> values.
    /// </summary>
    /// <param name="p1">The first point.</param>
    /// <param name="p2">The second point.</param>
    /// <returns>The dot product of <paramref name="p1"/> and <paramref name="p2"/>.</returns>
    public static float DotProduct(Point3 p1, Point3 p2) => p1.DotProduct(p2);

    /// <summary>
    /// Computes the cross product of two <see cref="Point3"/> values.
    /// </summary>
    /// <param name="a">The left-hand operand.</param>
    /// <param name="b">The right-hand operand.</param>
    /// <returns>The cross product <c>a × b</c>.</returns>
    public static Point3 CrossProduct(Point3 a, Point3 b) =>
        new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );

    /// <summary>
    /// The X component of the point.
    /// </summary>
    public float X { readonly get => x; set => x = value; }

    /// <summary>
    /// The Y component of the point.
    /// </summary>
    public float Y { readonly get => y; set => y = value; }

    /// <summary>
    /// The Z component of the point.
    /// </summary>
    public float Z { readonly get => z; set => z = value; }

    /// <summary>
    /// The Euclidean length (magnitude) of the vector represented by this point.
    /// </summary>
    public readonly float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// The squared length of the vector (avoids the square root operation).
    /// </summary>
    public readonly float LengthSquared => X * X + Y * Y + Z * Z;

    /// <summary>
    /// Returns a normalized copy of this vector (magnitude = 1). If this vector is zero, a zero vector is returned.
    /// </summary>
    public readonly Point3 Normal => (X == 0 && Y == 0 && Z == 0) ? new Point3() : new Point3(X / Length, Y / Length, Z / Length);

    /// <summary>
    /// Normalizes this vector in-place. If the length is zero, the vector is set to zero.
    /// Note: this is a mutating method on a struct; callers should be aware of copies.
    /// </summary>
    public void Normalize()
    {
        var l = Length;

        if (l == 0)
        {
            X = 0;
            Y = 0;
            Z = 0;
            return;
        }

        X /= l;
        Y /= l;
        Z /= l;
    }

    /// <summary>
    /// Clamps the magnitude of this vector to <paramref name="maxLength"/> while preserving direction. Mutates this instance.
    /// </summary>
    /// <param name="maxLength">Maximum allowed length.</param>
    public void Clamp(float maxLength)
    {
        float l = Length;

        if (l > maxLength)
        {
            X = X * maxLength / l;
            Y = Y * maxLength / l;
            Z = Z * maxLength / l;
        }
    }

    /// <summary>
    /// Returns the dot product (scalar product) with another point.
    /// </summary>
    /// <param name="other">The other point.</param>
    public readonly float DotProduct(Point3 other) => X * other.X + Y * other.Y + Z * other.Z;

    /// <summary>
    /// Returns the cross product of this vector with <paramref name="b"/>.
    /// </summary>
    /// <param name="b">The right-hand operand.</param>
    public readonly Point3 CrossProduct(Point3 b) => CrossProduct(this, b);

    /// <summary>
    /// Converts this 3D point to a 2D <see cref="Point"/> by discarding the Z component.
    /// </summary>
    public readonly Point ToPoint() => new(X, Y);

    /// <summary>
    /// Converts this point to a 4D point with a w component of 1.
    /// </summary>
    public readonly Point4 ToPoint4() => new(X, Y, Z, 1);

    /// <summary>
    /// Returns the components as a new float array in the order [X, Y, Z].
    /// </summary>
    public readonly float[] ToArray() => [X, Y, Z];

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="Point3"/>.
    /// Uses exact floating-point equality.
    /// </summary>
    /// <param name="other">The other point to compare.</param>
    public readonly bool Equals(Point3 other) => X == other.X && Y == other.Y && Z == other.Z;

    /// <summary>
    /// Adds two <see cref="Point3"/> values component-wise.
    /// </summary>
    public static Point3 operator +(Point3 p1, Point3 p2) => new(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);

    /// <summary>
    /// Subtracts the second <see cref="Point3"/> from the first component-wise.
    /// </summary>
    public static Point3 operator -(Point3 p1, Point3 p2) => new(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

    /// <summary>
    /// Multiplies a <see cref="Point3"/> by a scalar.
    /// </summary>
    public static Point3 operator *(float f, Point3 p) => new(p.X * f, p.Y * f, p.Z * f);

    /// <summary>
    /// Multiplies a <see cref="Point3"/> by a scalar.
    /// </summary>
    public static Point3 operator *(Point3 p, float f) => new(p.X * f, p.Y * f, p.Z * f);

    /// <summary>
    /// Divides a <see cref="Point3"/> by a scalar.
    /// </summary>
    public static Point3 operator /(Point3 p, float f) => new(p.X / f, p.Y / f, p.Z / f);

    /// <summary>
    /// Determines whether two <see cref="Point3"/> values are equal (component-wise exact equality).
    /// </summary>
    public static bool operator ==(Point3 p1, Point3 p2) => p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;

    /// <summary>
    /// Determines whether two <see cref="Point3"/> values are not equal (component-wise exact inequality).
    /// </summary>
    public static bool operator !=(Point3 p1, Point3 p2) => p1.X != p2.X || p1.Y != p2.Y || p1.Z != p2.Z;

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    public override readonly bool Equals(object? o) => o is Point3 p && Equals(p);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    /// <summary>
    /// Returns a string representation of the point.
    /// </summary>
    public override readonly string ToString() => $"(X : {X} Y : {Y} Z : {Z})";
}

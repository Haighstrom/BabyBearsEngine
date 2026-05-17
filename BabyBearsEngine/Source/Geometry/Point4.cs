using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace BabyBearsEngine.Geometry;

/// <summary>
/// A four-component vector (x, y, z, w) used for 3D homogeneous coordinates and matrix transformation.
/// Floating-point components are exposed as lowercase <c>x</c>/<c>y</c>/<c>z</c>/<c>w</c> properties;
/// integer-truncating aliases are exposed as uppercase <c>X</c>/<c>Y</c>/<c>Z</c>/<c>W</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Point4 : IEquatable<Point4>
{
    /// <summary>A <see cref="Point4"/> with all components set to zero.</summary>
    public static readonly Point4 Zero = new();

    /// <summary>Returns the dot product (scalar product) of <paramref name="p1"/> and <paramref name="p2"/>.</summary>
    public static float DotProduct(Point4 p1, Point4 p2)
    {
        return p1.DotProduct(p2);
    }


    private float _x, _y, _z, _w;


    /// <summary>Initialises a new <see cref="Point4"/> with the given components.</summary>
    public Point4(float x, float y, float z, float w)
    {
        _x = x;
        _y = y;
        _z = z;
        _w = w;
    }


    /// <summary>Gets or sets the X component as a <see langword="float"/>.</summary>
    public float x { get => _x; set => _x = value; }

    /// <summary>Gets or sets the Y component as a <see langword="float"/>.</summary>
    public float y { get => _y; set => _y = value; }

    /// <summary>Gets or sets the Z component as a <see langword="float"/>.</summary>
    public float z { get => _z; set => _z = value; }

    /// <summary>Gets or sets the W component as a <see langword="float"/>.</summary>
    public float w { get => _w; set => _w = value; }

    /// <summary>Gets or sets the X component as a truncated <see langword="int"/>. Setting converts the integer to <see langword="float"/>.</summary>
    [JsonIgnore]
    [XmlIgnore]
    public int X { get => (int)x; set => x = value; }

    /// <summary>Gets or sets the Y component as a truncated <see langword="int"/>. Setting converts the integer to <see langword="float"/>.</summary>
    [JsonIgnore]
    [XmlIgnore]
    public int Y { get => (int)y; set => y = value; }

    /// <summary>Gets or sets the Z component as a truncated <see langword="int"/>. Setting converts the integer to <see langword="float"/>.</summary>
    [JsonIgnore]
    [XmlIgnore]
    public int Z { get => (int)z; set => z = value; }

    /// <summary>Gets or sets the W component as a truncated <see langword="int"/>. Setting converts the integer to <see langword="float"/>.</summary>
    [JsonIgnore]
    [XmlIgnore]
    public int W { get => (int)w; set => w = value; }

    /// <summary>Gets the Euclidean length (magnitude) of this vector.</summary>
    public float Length => (float)Math.Sqrt(x * x + y * y + z * z + w * w);

    /// <summary>Gets the squared Euclidean length of this vector. Cheaper than <see cref="Length"/> when only relative comparisons are needed.</summary>
    public float LengthSquared => x * x + y * y + z * z + w * w;

    /// <summary>
    /// Returns a copy this Point, but with magnitude 1. Does not modify this Point.
    /// </summary>
    public Point4 Normal
    {
        get
        {
            if (x == 0 && y == 0 && z == 0 && w == 0)
            {
                return new Point4();
            }

            float l = Length;

            return new Point4(x / l, y / l, z / l, w / l);
        }
    }


    /// <summary>
    /// Normalize this point4 - set it to have magnitude 1. If it's length is zero, it will be set to (0,0,0,0).
    /// </summary>
    public void Normalize()
    {
        var l = Length;
        if (l == 0)
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
            return;
        }
        x /= l;
        y /= l;
        z /= l;
        w /= l;
    }


    /// <summary>
    /// Preserves direction of the point4 but clamps its magnitude to below maxLength
    /// </summary>
    /// <param name="maxLength"></param>
    public Point4 Clamp(float maxLength)
    {
        float l = Length;

        if (l > maxLength)
        {
            x = x * maxLength / l;
            y = y * maxLength / l;
            z = z * maxLength / l;
            w = w * maxLength / l;
        }

        return this;
    }


    /// <summary>
    /// Returns dot product (scalar product) with another point
    /// </summary>
    public float DotProduct(Point4 other)
    {
        return x * other.x + y * other.y + z * other.z + w * other.w;
    }


    /// <summary>
    /// Apply Matrix4 transformMatrix to this Point4 to result in a new Point4. This Point4 instance is not modified - a new one is returned.
    /// </summary>
    /// <param name="transformMatrix"></param>
    /// <returns></returns>
    public Point4 Transform(Matrix4 transformMatrix)
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
    public Point ToPoint() => new Point(x, y);

    /// <summary>Returns a <see cref="Point3"/> containing the X, Y, and Z components of this vector.</summary>
    public Point3 ToPoint3() => new Point3(x, y, z);

    /// <summary>Returns a new array containing the four components in order: [x, y, z, w].</summary>
    public float[] ToArray() => [x, y, z, w];

    /// <summary>Returns <see langword="true"/> when all four components are equal to those of <paramref name="other"/>.</summary>
    public bool Equals(Point4 other) => x == other.x && y == other.y && z == other.z && w == other.w;


    /// <summary>Component-wise addition.</summary>
    public static Point4 operator +(Point4 p1, Point4 p2) { return new Point4(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z, p1.w + p2.w); }

    /// <summary>Component-wise subtraction.</summary>
    public static Point4 operator -(Point4 p1, Point4 p2) { return new Point4(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z, p1.w - p2.w); }

    /// <summary>Scalar multiplication (scalar on left).</summary>
    public static Point4 operator *(float f, Point4 p) { return new Point4(p.x * f, p.y * f, p.z * f, p.w * f); }

    /// <summary>Scalar multiplication (scalar on right).</summary>
    public static Point4 operator *(Point4 p, float f) { return new Point4(p.x * f, p.y * f, p.z * f, p.w * f); }

    /// <summary>Scalar division.</summary>
    public static Point4 operator /(Point4 p, float f) { return new Point4(p.x / f, p.y / f, p.z / f, p.w / f); }

    /// <summary>Returns <see langword="true"/> when all four components of <paramref name="p1"/> equal those of <paramref name="p2"/>.</summary>
    public static bool operator ==(Point4 p1, Point4 p2) { return p1.x == p2.x && p1.y == p2.y && p1.z == p2.z && p1.w == p2.w; }

    /// <summary>Returns <see langword="true"/> when any component of <paramref name="p1"/> differs from the corresponding component of <paramref name="p2"/>.</summary>
    public static bool operator !=(Point4 p1, Point4 p2) { return p1.x != p2.x || p1.y != p2.y || p1.z != p2.z || p1.w != p2.w; }

    /// <inheritdoc/>
    public override bool Equals(object? o) => o is Point4 other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => base.GetHashCode();

    /// <summary>Returns a string representation of this vector in the form <c>(X : x Y : y Z : z W : w)</c>.</summary>
    public override string ToString() => FormattableString.Invariant($"(X : {x} Y : {y} Z : {z} W : {w})");
}

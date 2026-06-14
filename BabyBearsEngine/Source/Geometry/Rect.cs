using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace BabyBearsEngine.Geometry;

/// <summary>
/// An axis-aligned rectangle defined by its top-left corner (<see cref="X"/>, <see cref="Y"/>) and its size (<see cref="W"/>, <see cref="H"/>).
/// Coordinates use a top-left origin: increasing <see cref="Y"/> moves downward, so <see cref="Top"/> is the smaller Y and <see cref="Bottom"/> is the larger Y.
/// </summary>
public sealed record Rect
{
    /// <summary>
    /// A rectangle with top left (0,0) and nil width and height.
    /// </summary>
    public static Rect EmptyRect => new(0, 0, 0, 0);

    /// <summary>
    /// A rectangle with top left 0,0, width 1 and height 1.
    /// </summary>
    public static Rect UnitRect => new(0, 0, 1, 1);

    /// <summary>
    /// Find the rectangle that represents the intersection area between two rectangles.
    /// </summary>
    /// <param name="first">The first rectangle.</param>
    /// <param name="second">The second rectangle.</param>
    /// <returns>Returns the intersecting rectangle.</returns>
    public static Rect Intersection(Rect first, Rect second)
    {
        // Compute the overlap edges once. If the rectangles don't actually overlap (or only
        // touch at an edge in non-touching mode) the width or height comes out ≤ 0, which is
        // the same condition the old Intersects-then-recompute version checked.
        float x = Math.Max(first.Left, second.Left);
        float y = Math.Max(first.Top, second.Top);
        float w = Math.Min(first.Right, second.Right) - x;
        float h = Math.Min(first.Bottom, second.Bottom) - y;

        return w > 0 && h > 0 ? new Rect(x, y, w, h) : new Rect();
    }

    /// <summary>Creates a rectangle from explicit top-left coordinates and size.</summary>
    /// <param name="x">The X-coordinate of the top-left corner.</param>
    /// <param name="y">The Y-coordinate of the top-left corner.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        W = width;
        H = height;
    }

    /// <summary>Creates an empty rectangle at the origin (X = Y = W = H = 0).</summary>
    public Rect()
        : this(0, 0, 0, 0)
    {
    }

    /// <summary>Creates a rectangle at the origin with the given size.</summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rect(float width, float height)
        : this(0, 0, width, height)
    {
    }

    /// <summary>Creates a rectangle as a copy of another.</summary>
    /// <param name="rect">The rectangle to copy.</param>
    public Rect(Rect rect)
    {
        X = rect.X;
        Y = rect.Y;
        W = rect.W;
        H = rect.H;
    }

    /// <summary>Creates a rectangle from a top-left position and explicit size.</summary>
    /// <param name="position">The top-left corner.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rect(Point position, float width, float height)
        : this(position.X, position.Y, width, height)
    {
    }

    /// <summary>Creates a rectangle at the origin whose width and height are taken from a <see cref="Point"/>.</summary>
    /// <param name="size">A point whose X is the width and Y is the height.</param>
    public Rect(Point size)
        : this(0, 0, size.X, size.Y)
    {
    }

    /// <summary>Creates a rectangle from explicit top-left coordinates and a size taken from a <see cref="Point"/>.</summary>
    /// <param name="x">The X-coordinate of the top-left corner.</param>
    /// <param name="y">The Y-coordinate of the top-left corner.</param>
    /// <param name="size">A point whose X is the width and Y is the height.</param>
    public Rect(float x, float y, Point size)
        : this(x, y, size.X, size.Y)
    {
    }

    /// <summary>Creates a rectangle from a top-left position and a size, both expressed as <see cref="Point"/>s.</summary>
    /// <param name="position">The top-left corner.</param>
    /// <param name="size">A point whose X is the width and Y is the height.</param>
    public Rect(Point position, Point size)
        : this(position.X, position.Y, size.X, size.Y)
    {
    }

    /// <summary>
    /// Parses a rectangle from the format produced by <see cref="ToString"/>: <c>{X=1,Y=2,W=3,H=4}</c>.
    /// Values are parsed as <see cref="float"/> using <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    /// <param name="rectString">A string in the form <c>{X=...,Y=...,W=...,H=...}</c>.</param>
    /// <exception cref="FormatException">Thrown if the input does not match the expected format.</exception>
    public Rect(string rectString)
    {
        ArgumentNullException.ThrowIfNull(rectString);

        if (rectString.Length < 2 || rectString[0] != '{' || rectString[^1] != '}')
        {
            throw new FormatException($"Expected format '{{X=..,Y=..,W=..,H=..}}', got '{rectString}'.");
        }

        string[] parts = rectString[1..^1].Split(',');
        if (parts.Length != 4)
        {
            throw new FormatException($"Expected 4 comma-separated components, got {parts.Length} in '{rectString}'.");
        }

        X = ParseComponent(parts[0], "X");
        Y = ParseComponent(parts[1], "Y");
        W = ParseComponent(parts[2], "W");
        H = ParseComponent(parts[3], "H");
    }

    private static float ParseComponent(string part, string expectedKey)
    {
        string[] kv = part.Split('=');
        if (kv.Length != 2 || kv[0] != expectedKey)
        {
            throw new FormatException($"Expected '{expectedKey}=...', got '{part}'.");
        }
        return float.Parse(kv[1], CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// The X-position of the top left of the rectangle.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// The Y-position of the top left of the rectangle.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public float W { get; set; }

    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public float H { get; set; }

    /// <summary>
    /// The X,Y position of the rectangle represented as a <see cref="Point"/>.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point P
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>
    /// The x-coordinate of the left side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float Left => X;

    /// <summary>
    /// The x-coordinate of the right side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float Right => X + W;

    /// <summary>
    /// The y-coordinate of the top side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float Top => Y;

    /// <summary>
    /// The y-coordinate of the bottom side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float Bottom => Y + H;

    /// <summary>
    /// The area of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float Area => W * H;

    /// <summary>
    /// The width and height of the rectangle represented as a <see cref="Point"/>.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point Size => new(W, H);

    /// <summary>
    /// The length of the smallest side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float SmallestSide => Math.Min(W, H);

    /// <summary>
    /// The length of the biggest side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float BiggestSide => Math.Max(W, H);

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordinates of the top left corner of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point TopLeft => P;

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordintes of the centre of the top side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point TopCentre => new(X + W * 0.5f, Y);

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordinates of the top right corner of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point TopRight => new(X + W, Y);

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordintes of the centre of the left side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point CentreLeft => new(X, Y + H * 0.5f);

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordintes of the centre of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point Centre => new(X + W * 0.5f, Y + H * 0.5f);

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordintes of the centre of the right side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point CentreRight => new(X + W, Y + H * 0.5f);

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordinates of the bottom left corner of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point BottomLeft => new(X, Y + H);

    /// <summary>
    /// Returns a <see cref="Point"/> with the coordintes of the centre of the bottom side of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point BottomCentre => new(X + W * 0.5f, Y + H);

    /// <summary>
    /// A <see cref="Point"/> with the coordinates of the bottom right corner of the rectangle.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Point BottomRight => new(X + W, Y + H);

    /// <summary>
    /// Returns a new rectangle with the same size as this one but with the top-left coordinates at (0,0), i.e. X = Y = 0.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public Rect Zeroed => new(0, 0, W, H);

    /// <summary>
    /// Get a rectangle with the same top-left as this one, but with revised width and height.
    /// </summary>
    /// <param name="newW">The new width.</param>
    /// <param name="newH">The new height.</param>
    /// <returns>Returns a new resized rectangle.</returns>
    public Rect Resize(float newW, float newH) => new(X, Y, newW, newH);

    /// <summary>
    /// Get a rectangle with values shifted (added to) by the values specified. 
    /// </summary>
    /// <param name="x">The amount to add to the X value.</param>
    /// <param name="y">The amount to add to the Y value.</param>
    /// <param name="w">The amount to add to the W value.</param>
    /// <param name="h">The amount to add to the H value.</param>
    /// <returns>Returns a new shifted rectangle.</returns>
    public Rect Shift(float x, float y, float w = 0, float h = 0) => new(X + x, Y + y, W + w, H + h);

    /// <summary>
    /// Returns a new rectangle which is moved by a point/vector. 
    /// </summary>
    /// <param name="vector">The vector representing how far to move the rectangle by.</param>
    public Rect Shift(Point vector) => Shift(vector.X, vector.Y);

    /// <summary>
    /// Get a rectangle which is moved in the direction of a point/vector by a given distance.
    /// </summary>
    /// <param name="direction">A vector representing the direction the rectangle should be moved by.</param>
    /// <param name="distance">The amount the rectangle should be moved by.</param>
    /// <returns>Returns a new rectangle.</returns>
    public Rect Shift(Point direction, float distance) => Shift(direction.Normal * distance);

    /// <summary>
    /// Scale this rectangle by the specified amounts. The X,Y coordinates will be unaffected. Use <see cref="ScaleAround"/> if scaling X,Y is also desired.
    /// </summary>
    /// <param name="scaleX">The amount to scale W by.</param>
    /// <param name="scaleY">The amoune to scale H by.</param>
    /// <returns>Returns a new rectangle.</returns>
    public Rect Scale(float scaleX, float scaleY) => new(X, Y, W * scaleX, H * scaleY);

    /// <summary>
    /// Scales the rectangle around a specified point and by a specified amount.
    /// </summary>
    /// <param name="originX">The x-coordinate to scale around.</param>
    /// <param name="originY">The y-coordinate to scale around.</param>
    /// <param name="scaleX">The amount to scale the X value by.</param>
    /// <param name="scaleY">The amount to scale the Y value by.</param>
    /// <returns>Returns a new scaled rectangle.</returns>
    public Rect ScaleAround(float originX, float originY, float scaleX, float scaleY) => ResizeAround(originX, originY, W * scaleX, H * scaleY);

    /// <summary>
    /// Scales the Rectangle away from centre.
    /// </summary>
    /// <param name="scale">The amount to scale by.</param>
    /// <returns>Returns a new scaled rectangle.</returns>
    public Rect ScaleAroundCentre(float scale) => ScaleAroundCentre(scale, scale);

    /// <summary>
    /// Scales the Rectangle away from centre.
    /// </summary>
    /// <param name="scaleX">The amount to scale in the x direction.</param>
    /// <param name="scaleY">The amount to scale in the y direction.</param>
    /// <returns>Returns a new scaled rectangle.</returns>
    public Rect ScaleAroundCentre(float scaleX, float scaleY) => ScaleAround(Centre.X, Centre.Y, scaleX, scaleY);

    /// <summary>
    /// Resizes a rectangle while scaling its X and Y relative to the point given.
    /// </summary>
    /// <param name="origin">The point to scale around.</param>
    /// <param name="newW">The new width.</param>
    /// <param name="newH">The new height.</param>
    /// <returns>Returns a new resized rectangle.</returns>
    public Rect ResizeAround(Point origin, float newW, float newH) => ResizeAround(origin.X, origin.Y, newW, newH);

    /// <summary>
    /// Resizes a rectangle while scaling its X and Y relative to the coordinates given.
    /// </summary>
    /// <param name="originX">The x-coordinate to scale around.</param>
    /// <param name="originY">The y-coordinate to scale around.</param>
    /// <param name="newW">The new width.</param>
    /// <param name="newH">The new height.</param>
    /// <returns>Returns a new resized rectangle.</returns>
    public Rect ResizeAround(float originX, float originY, float newW, float newH)
    {
        return new(originX - newW * (originX - X) / W,
            originY - newH * (originY - Y) / H,
            newW, newH);
    }

    /// <summary>
    /// Expands the rectangle by the margin specified in all directions.
    /// </summary>
    /// <param name="margin">The amount to grow the rectangle in each direction. If a negative value is provided, the rectangle will shrink.</param>
    /// <returns>Returns a new expanded rectangle.</returns>
    public Rect Grow(float margin) => Grow(margin, margin, margin, margin);

    /// <summary>
    /// Expands the rectangle by the margin specified in each direction.
    /// </summary>
    /// <param name="left">The amount to move the left side further to the left.</param>
    /// <param name="up">The amount to move the top side further up.</param>
    /// <param name="right">The amount to move the right side further to the right.</param>
    /// <param name="down">The amount to move the bottom side further down.</param>
    /// <returns>Returns a new expanded rectangle.</returns>
    public Rect Grow(float left, float up, float right, float down) => new Rect(X - left, Y - up, W + left + right, H + up + down);

    /// <summary>
    /// Check whether this rectangle intersects another one.
    /// </summary>
    /// <param name="other">The rectangle to test again.</param>
    /// <param name="touchingCounts">Whether two rectangles that share a border should be considered intersecting.</param>
    /// <returns>Returns true if the rectangles intersects the other one, false otherwise.</returns>
    public bool Intersects(Rect other, bool touchingCounts = false)
    {
        if (touchingCounts)
        {
            return
                Left <= other.Right &&
                Right >= other.Left &&
                Top <= other.Bottom &&
                Bottom >= other.Top;
        }
        else
        {
            return
                Left < other.Right &&
                Right > other.Left &&
                Top < other.Bottom &&
                Bottom > other.Top;
        }
    }

    /// <summary>
    /// Check whether this rectangle contains another one. They may share borders.
    /// </summary>
    /// <param name="other">The rectangle to check if it is within this one.</param>
    /// <returns>Returns true if the other rectangle is within this one, false otherwise.</returns>
    public bool Contains(Rect other) => X <= other.X && Y <= other.Y && Right >= other.Right && Bottom >= other.Bottom;

    /// <summary>
    /// Check whether this rectangle contains another one. They may share borders.
    /// </summary>
    /// <param name="x">The x position of the rectangle to test.</param>
    /// <param name="y">The y position of the rectangle to test.</param>
    /// <param name="w">The width of the rectangle to test.</param>
    /// <param name="h">The height of the rectangle to test.</param>
    /// <returns>Returns true if the other rectangle is within this one, false otherwise.</returns>
    public bool Contains(float x, float y, float w, float h) => X <= x && Y <= y && X + W >= x + w && Y + H >= y + h;

    /// <summary>
    /// Checks whether this rectangle contains a <see cref="Point"/>.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="onLeftAndTopEdgesCount">Whether the point being on the left or top edge counts as being contained.</param>
    /// <param name="onRightAndBottomEdgesCount">Whether the point being on the right or bottom edge counts as being contained.</param>
    /// <returns>Returns true if the point is within this rectangle, false otherwise.</returns>
    public bool Contains(Point point, bool onLeftAndTopEdgesCount = true, bool onRightAndBottomEdgesCount = false)
    {
        if (onLeftAndTopEdgesCount && onRightAndBottomEdgesCount)
        {
            return X <= point.X && Y <= point.Y && X + W >= point.X && Y + H >= point.Y;
        }
        else if (onLeftAndTopEdgesCount && !onRightAndBottomEdgesCount)
        {
            return X <= point.X && Y <= point.Y && X + W > point.X && Y + H > point.Y;
        }
        else if (!onLeftAndTopEdgesCount && onRightAndBottomEdgesCount)
        {
            return X < point.X && Y < point.Y && X + W >= point.X && Y + H >= point.Y;
        }
        else
        {
            return X < point.X && Y < point.Y && X + W > point.X && Y + H > point.Y;
        }
    }

    /// <summary>
    /// Checks whether this rectangle contains a <see cref="Point"/>.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to test.</param>
    /// <param name="y">The y-coordinate of the point to test.</param>
    /// <param name="onLeftAndTopEdgesCount">Whether the point being on the left or top edge counts as being contained.</param>
    /// <param name="onRightAndBottomEdgesCount">Whether the point being on the right or bottom edge counts as being contained.</param>
    /// <returns>Returns true if the point is within this rectangle, false otherwise.</returns>
    public bool Contains(float x, float y, bool onLeftAndTopEdgesCount = true, bool onRightAndBottomEdgesCount = false)
    {
        if (onLeftAndTopEdgesCount && onRightAndBottomEdgesCount)
        {
            return X <= x && Y <= y && X + W >= x && Y + H >= y;
        }
        else if (onLeftAndTopEdgesCount && !onRightAndBottomEdgesCount)
        {
            return X <= x && Y <= y && X + W > x && Y + H > y;
        }
        else if (!onLeftAndTopEdgesCount && onRightAndBottomEdgesCount)
        {
            return X < x && Y < y && X + W >= x && Y + H >= y;
        }
        else
        {
            return X < x && Y < y && X + W > x && Y + H > y;
        }
    }

    /// <summary>
    /// Get this rectangle as a clockwise list of vertices starting at the top left.
    /// </summary>
    /// <returns>Returns a list of four points.</returns>
    public List<Point> ToVertices() => [TopLeft, TopRight, BottomRight, BottomLeft];

    /// <summary>Returns a new rectangle translated by <paramref name="p"/>. Width and height are unchanged.</summary>
    public static Rect operator +(Rect r, Point p) => new(r.X + p.X, r.Y + p.Y, r.W, r.H);

    /// <summary>Returns a new rectangle whose X, Y, W, H are the component-wise sum of the operands.</summary>
    public static Rect operator +(Rect left, Rect right) => new(left.X + right.X, left.Y + right.Y, left.W + right.W, left.H + right.H);

    /// <summary>Returns a new rectangle whose X, Y, W, H are the component-wise difference of the operands.</summary>
    public static Rect operator -(Rect left, Rect right) => new(left.X - right.X, left.Y - right.Y, left.W - right.W, left.H - right.H);

    /// <summary>
    /// Returns a string of the form <c>{X=1,Y=2,W=3,H=4}</c> using <see cref="CultureInfo.InvariantCulture"/>.
    /// Round-trips through <see cref="Rect(string)"/>.
    /// </summary>
    public override string ToString() => FormattableString.Invariant($"{{X={X},Y={Y},W={W},H={H}}}");
}

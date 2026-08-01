using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Triangulates a simple polygon (convex or concave, non-self-intersecting) via ear clipping.
/// Pure geometry, no GL dependency, so the math is unit testable without an OpenGL context —
/// mirroring how <see cref="RadialFillMeshBuilder"/> is testable independent of
/// <see cref="RadialFillGraphic"/>. Used by <see cref="PolygonGraphic"/>.
/// </summary>
public static class PolygonTriangulator
{
    /// <summary>
    /// Triangulates <paramref name="polygon"/>'s boundary, returning a flat list of triangle
    /// vertices (every 3 consecutive points form one triangle). The polygon must not already
    /// repeat its first point as its last — that's the caller's convention to normalize, not this
    /// method's.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="polygon"/> has fewer than 3 points.</exception>
    public static Point[] Triangulate(IReadOnlyList<Point> polygon)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(polygon.Count, 3, nameof(polygon));

        List<int> remainingIndices = [.. Enumerable.Range(0, polygon.Count)];
        if (SignedArea(polygon) < 0f)
        {
            // Ear-clipping's convexity test assumes a consistent winding; normalize to whichever
            // direction this polygon's own signed area calls "positive" and clip against that.
            remainingIndices.Reverse();
        }

        List<Point> triangleVertices = [];

        while (remainingIndices.Count > 3)
        {
            int earPosition = FindEar(polygon, remainingIndices);
            if (earPosition < 0)
            {
                // Degenerate or self-intersecting input — stop rather than loop forever; whatever
                // has been triangulated so far is returned.
                break;
            }

            int vertexCount = remainingIndices.Count;
            int previousIndex = remainingIndices[(earPosition - 1 + vertexCount) % vertexCount];
            int earIndex = remainingIndices[earPosition];
            int nextIndex = remainingIndices[(earPosition + 1) % vertexCount];

            triangleVertices.Add(polygon[previousIndex]);
            triangleVertices.Add(polygon[earIndex]);
            triangleVertices.Add(polygon[nextIndex]);

            remainingIndices.RemoveAt(earPosition);
        }

        if (remainingIndices.Count == 3)
        {
            triangleVertices.Add(polygon[remainingIndices[0]]);
            triangleVertices.Add(polygon[remainingIndices[1]]);
            triangleVertices.Add(polygon[remainingIndices[2]]);
        }

        return [.. triangleVertices];
    }

    private static float Cross(Point a, Point b, Point c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

    private static int FindEar(IReadOnlyList<Point> polygon, List<int> remainingIndices)
    {
        int vertexCount = remainingIndices.Count;
        for (int candidatePosition = 0; candidatePosition < vertexCount; candidatePosition++)
        {
            int previousIndex = remainingIndices[(candidatePosition - 1 + vertexCount) % vertexCount];
            int candidateIndex = remainingIndices[candidatePosition];
            int nextIndex = remainingIndices[(candidatePosition + 1) % vertexCount];

            Point previous = polygon[previousIndex];
            Point candidate = polygon[candidateIndex];
            Point next = polygon[nextIndex];

            if (Cross(previous, candidate, next) <= 0f)
            {
                continue; // reflex or degenerate vertex — can't be an ear
            }

            bool containsAnotherVertex = false;
            for (int testPosition = 0; testPosition < vertexCount; testPosition++)
            {
                int testIndex = remainingIndices[testPosition];
                if (testIndex == previousIndex || testIndex == candidateIndex || testIndex == nextIndex)
                {
                    continue;
                }

                if (IsPointInTriangle(polygon[testIndex], previous, candidate, next))
                {
                    containsAnotherVertex = true;
                    break;
                }
            }

            if (!containsAnotherVertex)
            {
                return candidatePosition;
            }
        }

        return -1;
    }

    private static bool IsPointInTriangle(Point point, Point a, Point b, Point c)
    {
        float crossA = Cross(a, b, point);
        float crossB = Cross(b, c, point);
        float crossC = Cross(c, a, point);

        bool hasNegative = crossA < 0f || crossB < 0f || crossC < 0f;
        bool hasPositive = crossA > 0f || crossB > 0f || crossC > 0f;

        return !(hasNegative && hasPositive);
    }

    private static float SignedArea(IReadOnlyList<Point> polygon)
    {
        float area = 0f;
        for (int pointIndex = 0; pointIndex < polygon.Count; pointIndex++)
        {
            Point current = polygon[pointIndex];
            Point next = polygon[(pointIndex + 1) % polygon.Count];
            area += current.X * next.Y - next.X * current.Y;
        }

        return area * 0.5f;
    }
}

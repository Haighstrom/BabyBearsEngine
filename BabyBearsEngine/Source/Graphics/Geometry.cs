using System.Collections.Generic;
using BabyBearsEngine.Source.Graphics.Components;

namespace BabyBearsEngine.Source.Graphics;

internal static class Geometry
{
    public static List<Vertex> QuadToTris(Vertex topLeft, Vertex topRight, Vertex bottomLeft, Vertex bottomRight)
    {
        return
        [
            bottomLeft, topRight, topLeft,
            bottomLeft, topRight, bottomRight
        ];
    }

    //new (x + width, y + height, Colour, 1, 1), // top right
    //            new (x + width, y, Colour, 1, 0), // bottom right
    //            new (x, y + height, Colour, 0, 1), // top left
    //            new (x, y, Colour, 0, 0), // bottom left

    ///// <summary>
    ///// Returns a vector of length 1 that points in the angle requested clockwise from up
    ///// </summary>
    ///// <param name="angleInDegrees">The angle of the vector, in degrees.</param>
    ///// <returns>Returns a point of length 1.</returns>
    //public static Point GetUnitVectorFromAngle(float angleInDegrees)
    //{
    //    float x = (float)Math.Sin(angleInDegrees * Math.PI / 180);
    //    float y = (float)Math.Cos(angleInDegrees * Math.PI / 180);
    //    return new(x, y);
    //}
}

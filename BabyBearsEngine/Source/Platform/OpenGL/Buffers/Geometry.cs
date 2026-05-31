using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Platform.OpenGL.Buffers;

internal static class GeometryHelper
{
    public static List<Vertex> QuadToTris(Vertex topLeft, Vertex topRight, Vertex bottomLeft, Vertex bottomRight)
    {
        return
        [
            bottomLeft, topRight, topLeft,
            bottomLeft, topRight, bottomRight
        ];
    }
}

using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace BabyBearsEngine.OpenGL;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct VertexNoTexture(float x, float y, Color4 colour) : IVertex
{
    public static int Stride { get; } = (2 + 4) * sizeof(float);

    public static void SetVertexAttributes()
    {
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Stride, 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Stride, 2 * sizeof(float));
    }

    public float X { get; } = x;

    public float Y { get; } = y;

    public Color4 Colour { get; } = colour;
}

using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics.Components;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Vertex(float x, float y, Color4 colour, float u, float v) : IVertex
{
    public static int Stride { get; } = (2 + 4 + 2) * sizeof(float);

    public float X { get; } = x;

    public float Y { get; } = y;

    public Color4 Colour { get; } = colour;

    public float U { get; } = u;

    public float V { get; } = v;
}

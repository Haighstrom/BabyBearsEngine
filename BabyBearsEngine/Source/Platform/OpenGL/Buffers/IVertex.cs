namespace BabyBearsEngine.OpenGL;

public interface IVertex
{
    static abstract int Stride { get; }

    static abstract void SetVertexAttributes();
}

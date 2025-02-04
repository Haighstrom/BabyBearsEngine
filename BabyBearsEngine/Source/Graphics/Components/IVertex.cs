namespace BabyBearsEngine.Source.Graphics.Components;

public interface IVertex
{
    static abstract int Stride { get; }

    static abstract void SetVertexAttributes();
}

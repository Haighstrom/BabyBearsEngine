namespace BabyBearsEngine.Source.Graphics.Components;

internal interface IVBO : IOpenGLObject
{
    void BufferData<TVertex>(TVertex[] vertices) where TVertex : struct, IVertex;
}

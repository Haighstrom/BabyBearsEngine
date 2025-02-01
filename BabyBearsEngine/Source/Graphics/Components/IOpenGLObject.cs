namespace BabyBearsEngine.Source.Graphics.Components;

public interface IOpenGLObject : IDisposable
{
    int Handle { get; }

    void Bind();

    void Unbind();
}

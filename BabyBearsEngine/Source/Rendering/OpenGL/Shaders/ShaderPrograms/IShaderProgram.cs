namespace BabyBearsEngine.OpenGL;

public interface IShaderProgram : IDisposable
{
    int Handle { get; }

    void Bind();
}

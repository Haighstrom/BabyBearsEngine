namespace BabyBearsEngine.Source.Graphics.Shaders;

public interface IShaderProgram : IDisposable
{
    int Handle { get; }

    void Bind();
}

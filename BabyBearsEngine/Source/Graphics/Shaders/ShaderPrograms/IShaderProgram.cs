namespace BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;

public interface IShaderProgram : IDisposable
{
    int Handle { get; }

    void Bind();
}

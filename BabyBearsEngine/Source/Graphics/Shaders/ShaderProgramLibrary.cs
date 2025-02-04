using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class ShaderProgramLibrary(GameWindow window)
{
    public DefaultShaderProgram DefaultShaderProgram { get; } = new(window);

    public SolidColourShaderProgram SolidColourShaderProgram { get; } = new(window);

    public PointShaderProgram PointShaderProgram { get; } = new(window);

    public StandardMatrixShaderProgram StandardMatrixShaderProgram { get; } = new(window);
}

using BabyBearsEngine.Source.Core;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;

public sealed class SolidColourShaderProgram : ShaderProgramBase
{
    private static readonly Lazy<SolidColourShaderProgram> s_instance = new(() => new SolidColourShaderProgram());

    public static SolidColourShaderProgram Instance => s_instance.Value;

    private readonly int _windowSizeLocation;

    private SolidColourShaderProgram()
        :base(VertexShaders.NoMatrixSolidColour, FragmentShaders.SolidColour)
    {
        _windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");

        SetWindowSize(Window.Width, Window.Height);

        Window.Resize += args => SetWindowSize(args.Width, args.Height);
    }

    private void SetWindowSize(int width, int height)
    {
        Bind();
        GL.Uniform2(_windowSizeLocation, new Vector2(width, height));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Window.Resize -= args => SetWindowSize(args.Width, args.Height);
        }

        base.Dispose(disposing);
    }
}

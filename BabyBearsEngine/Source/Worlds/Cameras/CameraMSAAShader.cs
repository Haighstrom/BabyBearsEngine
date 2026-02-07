using BabyBearsEngine.OpenGL;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Worlds.Cameras;

/// <summary>
/// Shader used with the multisample FBO for the MSAA antialiasing pass only
/// </summary>
public class CameraMSAAShader : ShaderProgramBase
{
    private readonly int _locationMVMatrix;
    private readonly int _locationPMatrix;
    private readonly int _locationPosition;
    private readonly int _locationTexture;
    private readonly int _locationSamplesUniform;

    public CameraMSAAShader()
        : base(VertexShaders.CameraMSAA, FragmentShaders.CameraMSAA)
    {
        _locationMVMatrix = GL.GetUniformLocation(Handle, "MVMatrix");
        _locationPMatrix = GL.GetUniformLocation(Handle, "PMatrix");
        _locationPosition = GL.GetAttribLocation(Handle, "Position");
        _locationTexture = GL.GetAttribLocation(Handle, "TexCoord");
        _locationSamplesUniform = GL.GetUniformLocation(Handle, "MSAASamples");
    }


    public MsaaSamples Samples { get; set; }

    public void Render(ref Matrix3 projection, ref Matrix3 modelView, int verticesLength, PrimitiveType drawType)
    {
        Bind();

        GL.UniformMatrix3(_locationMVMatrix, true, ref modelView);
        GL.UniformMatrix3(_locationPMatrix, true, ref projection);

        //Bind MSAA sample numbers uniform
        GL.Uniform1(_locationSamplesUniform, (int)Samples);

        GL.EnableVertexAttribArray(_locationPosition);
        GL.VertexAttribPointer(_locationPosition, 2, VertexAttribPointerType.Float, false, Vertex.Stride, 0);

        GL.EnableVertexAttribArray(_locationTexture);
        GL.VertexAttribPointer(_locationTexture, 2, VertexAttribPointerType.Float, false, Vertex.Stride, 12);

        GL.DrawArrays(drawType, 0, verticesLength);

        GL.DisableVertexAttribArray(_locationPosition);
        GL.DisableVertexAttribArray(_locationTexture);
    }
}

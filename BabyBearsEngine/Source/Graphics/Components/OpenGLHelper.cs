using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics.Components;

internal static class OpenGLHelper
{
    public static int CreateShader(string source, ShaderType shaderType)
    {
        var handle = GL.CreateShader(shaderType);

        GL.ShaderSource(handle, source);

        GL.CompileShader(handle);

        GL.GetShader(handle, ShaderParameter.CompileStatus, out var success);

        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(handle);

            System.Console.WriteLine(infoLog);
        }

        return handle;
    }

    public static int CreateShaderProgram(int vertexShader, int fragmentShader, bool deleteShaders = true)
    {
        var handle = GL.CreateProgram();

        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);

        GL.LinkProgram(handle);

        GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out var success);

        if (success == 0)
        {
            var infoLog = GL.GetProgramInfoLog(handle);

            Console.WriteLine(infoLog);
        }

        if (deleteShaders)
        {
            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }

        return handle;
    }

    public static void CheckError()
    {
        var error = GL.GetError();

        if (error != ErrorCode.NoError)
        {
            Console.WriteLine($"OpenGL Error: {error}");
        }
    }
}

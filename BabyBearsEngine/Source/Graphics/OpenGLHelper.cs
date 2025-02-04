using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

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
            GL.DeleteShader(handle);
            throw new Exception($"Shader compilation failed ({shaderType}): {infoLog}");
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
            GL.DeleteProgram(handle);
            throw new Exception($"Shader linking failed: {infoLog}");
        }

        if (deleteShaders)
        {
            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
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

    public static Matrix3 CreateOrthographicProjectionMatrix(int width, int height)
    {
        var halfWidth = width / 2f;
        var halfHeight = height / 2f;

        Matrix3 scale = new(1 / halfWidth, 0, 0, 0, 1 / halfHeight, 0, 0, 0, 1);
        Matrix3 flipY = new(1, 0, 0, 0, -1, 0, 0, 0, 1);
        Matrix3 translate = new(1, 0, -halfWidth, 0, 1, -halfHeight, 0, 0, 1);

        var ortho = scale * flipY * translate;

        return ortho;
    }
}

using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Graphics;

internal static class OpenGLHelper
{
    private static int s_lastBoundShader = -1;
    private static int s_lastBoundVAO = -1;
    private static int s_lastBoundVBO = -1;
    private static int s_lastBoundEBO = -1;
    private static int s_lastBoundTexture = -1;
    private static TextureUnit s_lastActiveTextureUnit = TextureUnit.Texture0;

    public static void BindEBO(EBO eBO)
    {
        if (s_lastBoundEBO != eBO.Handle)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eBO.Handle);
            s_lastBoundEBO = eBO.Handle;
        }
    }

    public static void BindShader(ShaderProgramBase shader)
    {
        if (s_lastBoundShader != shader.Handle)
        {
            GL.UseProgram(shader.Handle);
            s_lastBoundShader = shader.Handle;
        }
    }

    public static void BindTexture(ITexture texture, TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0)
    {
        if (s_lastActiveTextureUnit != textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            s_lastActiveTextureUnit = textureUnit;
        }

        if (s_lastBoundTexture != texture.Handle)
        {
            GL.BindTexture(textureTarget, texture.Handle);
            s_lastBoundTexture = texture.Handle;
        }
    }

    public static void BindVAO(VAO vAO)
    {
        if (s_lastBoundVAO != vAO.Handle)
        {
            GL.BindVertexArray(vAO.Handle);
            s_lastBoundVAO = vAO.Handle;
        }
    }

    public static void BindVBO(VBO vBO)
    {
        if (s_lastBoundVBO != vBO.Handle)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vBO.Handle);
            s_lastBoundVBO = vBO.Handle;
        }
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
    
    public static void UnbindEBO()
    {
        if (s_lastBoundEBO != 0)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            s_lastBoundEBO = 0;
        }
    }

    public static void UnbindShader()
    {
        if (s_lastBoundShader != 0)
        {
            GL.UseProgram(0);
            s_lastBoundShader = 0;
        }
    }

    public static void UnbindTexture()
    {
        if (s_lastBoundTexture != 0)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            s_lastBoundTexture = 0;
        }
    }

    public static void UnbindVAO()
    {
        if (s_lastBoundVAO != 0)
        {
            GL.BindVertexArray(0);
            s_lastBoundVAO = 0;
        }
    }

    public static void UnbindVBO()
    {
        if (s_lastBoundVBO != 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            s_lastBoundVBO = 0;
        }
    }
}

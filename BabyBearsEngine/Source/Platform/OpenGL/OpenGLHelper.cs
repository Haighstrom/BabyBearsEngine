using OpenTK.Mathematics;

namespace BabyBearsEngine.OpenGL;

public static class OpenGLHelper
{
    private static int s_lastBoundShader = -1;
    private static int s_lastBoundVAO = -1;
    private static int s_lastBoundVBO = -1;
    private static int s_lastBoundEBO = -1;
    private static int s_lastBoundTexture = -1;
    private static TextureUnit s_lastActiveTextureUnit = TextureUnit.Texture0;

    public static int LastBoundFBO { get; set; }

    public static void BindFBO(int fboHandle)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboHandle);
        LastBoundFBO = fboHandle;
    }

    public static (int X, int Y, int Width, int Height) GetViewport()
    {
        int[] ints = new int[4];
        GL.GetInteger(GetPName.Viewport, ints);
        return (ints[0], ints[1], ints[2], ints[3]);
    }

    public static void BindEBO(int eBOHandle)
    {
        //if (s_lastBoundEBO != eBOHandle)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eBOHandle);
            s_lastBoundEBO = eBOHandle;
        }
    }

    public static void BindShader(int shaderHandle)
    {
        //if (s_lastBoundShader != shaderHandle)
        {
            GL.UseProgram(shaderHandle);
            s_lastBoundShader = shaderHandle;
        }
    }

    public static void BindTexture(int textureHandle, TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0)
    {
        //if (s_lastActiveTextureUnit != textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            s_lastActiveTextureUnit = textureUnit;
        }

        //if (s_lastBoundTexture != textureHandle)
        {
            GL.BindTexture(textureTarget, textureHandle);
            s_lastBoundTexture = textureHandle;
        }
    }

    public static void BindVAO(int vAOHandle)
    {
        //if (s_lastBoundVAO != vAOHandle)
        {
            GL.BindVertexArray(vAOHandle);
            s_lastBoundVAO = vAOHandle;
        }
    }

    public static void BindVBO(int vBOHandle)
    {
        //if (s_lastBoundVBO != vBOHandle)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vBOHandle);
            s_lastBoundVBO = vBOHandle;
        }
    }

    public static void CheckError()
    {
        var error = GL.GetError();

        if (error != ErrorCode.NoError)
        {
            System.Console.WriteLine($"OpenGL Error: {error}");
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

    // Same as CreateOrtho but without the FlipY stage. Use for makign projection matrices when rendering to a framebuffer rather than screen.
    public static Matrix3 CreateFBOOrthographicProjectionMatrix(int width, int height)
    {
        var halfWidth = width / 2f;
        var halfHeight = height / 2f;

        Matrix3 scale = new(1 / halfWidth, 0, 0, 0, 1 / halfHeight, 0, 0, 0, 1);
        Matrix3 translate = new(1, 0, -halfWidth, 0, 1, -halfHeight, 0, 0, 1);

        var ortho = scale * translate;

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
        var programHandle = GL.CreateProgram();

        var programBuiltSuccessfully = false;
        var shadersAttached = false;

        try
        {
            GL.AttachShader(programHandle, vertexShader);
            GL.AttachShader(programHandle, fragmentShader);
            GL.LinkProgram(programHandle);
            GL.GetProgram(programHandle, GetProgramParameterName.LinkStatus, out var linkStatus);

            if (linkStatus == 0)
            {
                var infoLog = GL.GetProgramInfoLog(programHandle);
                throw new Exception($"Shader linking failed: {infoLog}");
            }

            programBuiltSuccessfully = true;

            return programHandle;
        }
        finally
        {
            if (deleteShaders)
            {
                if (shadersAttached)
                {
                    GL.DetachShader(programHandle, vertexShader);
                    GL.DetachShader(programHandle, fragmentShader);
                }

                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
            }

            if (!programBuiltSuccessfully)
            {
                GL.DeleteProgram(programHandle);
            }
        }
    }

    public static void UnbindShader()
    {
        //if (s_lastBoundShader != 0)
        {
            GL.UseProgram(0);
            s_lastBoundShader = 0;
        }
    }

    public static void UnbindTexture()
    {
        //if (s_lastBoundTexture != 0)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            s_lastBoundTexture = 0;
        }
    }

    public static void UnbindVertexArray()
    {
        //if (s_lastBoundVAO != 0)
        {
            GL.BindVertexArray(0);
            s_lastBoundVAO = 0;
        }
    }

    public static void UnbindVBO()
    {
        //if (s_lastBoundVBO != 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            s_lastBoundVBO = 0;
        }
    }

    public static void UnbindEBO()
    {
        //if (s_lastBoundEBO != 0)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            s_lastBoundEBO = 0;
        }
    }

    public static float[] GetValues(this Matrix3 matrix)
    {
        return [matrix.M11, matrix.M12, matrix.M13, matrix.M21, matrix.M22, matrix.M23, matrix.M31, matrix.M32, matrix.M33];
    }

    public static void UniformMatrix3(int location, Matrix3 matrix)
    {
        unsafe
        {
            fixed (float* valuePointer = matrix.GetValues())
            {
                GL.UniformMatrix3(location, 1, false, valuePointer);
            }
        }
    }
}

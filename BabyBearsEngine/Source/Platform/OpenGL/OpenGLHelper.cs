using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

internal static class OpenGLHelper
{
    private static int s_lastBoundShader = -1;
    private static int s_lastBoundVAO = -1;
    private static int s_lastBoundVBO = -1;
    private static int s_lastBoundEBO = -1;
    private static int s_lastBoundTexture = -1;
    private static TextureUnit s_lastActiveTextureUnit = TextureUnit.Texture0;

    public static int LastBoundFBO { get; set; }

    /// <summary>
    /// Resets the bind cache to its initial uninitialised state. Must be called whenever the
    /// GL context is recreated (e.g. between consecutive test runs each spinning up their own
    /// <see cref="GameLauncher.Run"/>) — otherwise a stale handle from the previous context
    /// can match a freshly-generated handle in the new context and cause Bind calls to be
    /// silently skipped.
    /// </summary>
    public static void ResetCache()
    {
        s_lastBoundShader = -1;
        s_lastBoundVAO = -1;
        s_lastBoundVBO = -1;
        s_lastBoundEBO = -1;
        s_lastBoundTexture = -1;
        s_lastActiveTextureUnit = TextureUnit.Texture0;
        LastBoundFBO = 0;
    }

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
        if (s_lastBoundEBO != eBOHandle)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eBOHandle);
            s_lastBoundEBO = eBOHandle;
        }
    }

    public static void BindShader(int shaderHandle)
    {
        if (s_lastBoundShader != shaderHandle)
        {
            GL.UseProgram(shaderHandle);
            s_lastBoundShader = shaderHandle;
        }
    }

    // The cache holds a single "last bound texture" handle, which is correct as long as we
    // stay on one texture unit. When the active unit changes we invalidate the cache so the
    // next bind always issues, because we don't track what's bound on the unit we're switching to.
    public static void BindTexture(int textureHandle, TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0)
    {
        if (s_lastActiveTextureUnit != textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            s_lastActiveTextureUnit = textureUnit;
            s_lastBoundTexture = -1;
        }

        if (s_lastBoundTexture != textureHandle)
        {
            GL.BindTexture(textureTarget, textureHandle);
            s_lastBoundTexture = textureHandle;
        }
    }

    public static void BindVAO(int vAOHandle)
    {
        if (s_lastBoundVAO != vAOHandle)
        {
            GL.BindVertexArray(vAOHandle);
            s_lastBoundVAO = vAOHandle;
        }
    }

    public static void BindVBO(int vBOHandle)
    {
        if (s_lastBoundVBO != vBOHandle)
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

    public static int CreateShader(string source, ShaderType shaderType)
    {
        int handle = GL.CreateShader(shaderType);
        GL.ShaderSource(handle, source);
        GL.CompileShader(handle);

        GL.GetShader(handle, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(handle);
            GL.DeleteShader(handle);
            throw new Exception($"Shader compilation failed ({shaderType}): {infoLog}");
        }

        return handle;
    }

    public static int CreateShaderProgram(int vertexShader, int fragmentShader, bool deleteShaders = true)
    {
        return CreateShaderProgram(vertexShader, geometryShader: 0, fragmentShader, deleteShaders);
    }

    public static int CreateShaderProgram(int vertexShader, int geometryShader, int fragmentShader, bool deleteShaders = true)
    {
        int programHandle = GL.CreateProgram();

        bool programBuiltSuccessfully = false;
        bool shadersAttached = false;

        try
        {
            GL.AttachShader(programHandle, vertexShader);
            if (geometryShader != 0)
            {
                GL.AttachShader(programHandle, geometryShader);
            }
            GL.AttachShader(programHandle, fragmentShader);
            shadersAttached = true;
            GL.LinkProgram(programHandle);
            GL.GetProgram(programHandle, GetProgramParameterName.LinkStatus, out int linkStatus);

            if (linkStatus == 0)
            {
                string infoLog = GL.GetProgramInfoLog(programHandle);
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
                    if (geometryShader != 0)
                    {
                        GL.DetachShader(programHandle, geometryShader);
                    }
                    GL.DetachShader(programHandle, fragmentShader);
                }

                GL.DeleteShader(vertexShader);
                if (geometryShader != 0)
                {
                    GL.DeleteShader(geometryShader);
                }
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
        if (s_lastBoundShader != 0)
        {
            GL.UseProgram(0);
            s_lastBoundShader = 0;
        }
    }

    public static void UnbindTexture(TextureTarget textureTarget = TextureTarget.Texture2D)
    {
        if (s_lastBoundTexture != 0)
        {
            GL.BindTexture(textureTarget, 0);
            s_lastBoundTexture = 0;
        }
    }

    public static void UnbindVertexArray()
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

    public static void UnbindEBO()
    {
        if (s_lastBoundEBO != 0)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            s_lastBoundEBO = 0;
        }
    }

    public static void UnbindFBO()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        LastBoundFBO = 0;
    }

    public static void SaveTextureToFile(Texture t, string filePath)
    {
        var b = TextureToBitmap(t);
        SaveBitmapToPNGFile(b, filePath);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static System.Drawing.Bitmap TextureToBitmap(Texture t)
    {
        System.Drawing.Bitmap bmp = new(t.Width, t.Height);
        BindTexture(t.Handle);
        var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);
        return bmp;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static void SaveBitmapToPNGFile(System.Drawing.Bitmap bmp, string filePath)
    {
        //Make sure the filepath has exactly one png extension
        if (Path.GetExtension(filePath) == null)
        {
            filePath += ".png";
        }

        //Save the bitmap
        bmp.Save(filePath);
    }
}

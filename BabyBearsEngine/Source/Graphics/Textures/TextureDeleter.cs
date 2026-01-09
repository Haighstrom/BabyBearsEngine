using System.Collections.Generic;
using BabyBearsEngine.Source.Graphics.Components;

namespace BabyBearsEngine.Source.Graphics.Textures;

internal static class TextureDeleter
{
    private static readonly List<ITexture> s_texturesToDelete = [];
    private static readonly List<VBO> s_vBOsToDelete = [];
    private static readonly List<VAO> s_vAOsToDelete = [];

    public static void RequestDelete(ITexture texture)
    {
        if (!s_texturesToDelete.Contains(texture))
        {
            s_texturesToDelete.Add(texture);
        }
    }

    public static void RequestDelete(VBO vbo)
    {
        if (!s_vBOsToDelete.Contains(vbo))
        {
            s_vBOsToDelete.Add(vbo);
        }
    }

    public static void RequestDelete(VAO vao)
    {
        if (!s_vAOsToDelete.Contains(vao))
        {
            s_vAOsToDelete.Add(vao);
        }
    }

    public static void ProcessDeletes()
    {
        foreach (var texture in s_texturesToDelete)
        {
            OpenGLHelper.UnbindTexture();
            GL.DeleteTexture(texture.Handle);
        }

        s_texturesToDelete.Clear();

        foreach (var vbo in s_vBOsToDelete)
        {
            OpenGLHelper.UnbindVBO();
            GL.DeleteBuffer(vbo.Handle);
        }

        s_vBOsToDelete.Clear();

        foreach (var vao in s_vAOsToDelete)
        {
            OpenGLHelper.UnbindVAO();
            GL.DeleteVertexArray(vao.Handle);
        }

        s_vAOsToDelete.Clear();
    }
}

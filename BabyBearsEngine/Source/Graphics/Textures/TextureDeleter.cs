using System.Collections.Generic;

namespace BabyBearsEngine.Source.Graphics.Textures;

internal static class TextureDeleter
{
    private static readonly List<ITexture> s_texturesToDelete = [];

    public static void RequestDelete(ITexture texture)
    {
        if (!s_texturesToDelete.Contains(texture))
        {
            s_texturesToDelete.Add(texture);
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
    }
}

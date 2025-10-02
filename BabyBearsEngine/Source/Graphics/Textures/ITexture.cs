namespace BabyBearsEngine.Source.Graphics.Textures;

public interface ITexture : IDisposable
{
    int Handle { get; }

    /// <summary>
    /// The original image's width, in px
    /// </summary>
    int Width { get; }

    /// <summary>
    /// The original image's height, in px
    /// </summary>
    int Height { get; }

    void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0);
}

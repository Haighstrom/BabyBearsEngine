namespace BabyBearsEngine.Source.Graphics.Components;

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
}

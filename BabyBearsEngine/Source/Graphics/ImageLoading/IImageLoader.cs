namespace BabyBearsEngine.Source.Graphics.ImageLoading;

internal interface IImageLoader
{
    Rgba8ImageData GetImageData(string path);
}

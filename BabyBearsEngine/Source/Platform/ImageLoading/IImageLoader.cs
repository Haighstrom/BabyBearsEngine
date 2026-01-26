namespace BabyBearsEngine.Source.Platform.ImageLoading;

internal interface IImageLoader
{
    Rgba8ImageData GetImageData(string path);
}

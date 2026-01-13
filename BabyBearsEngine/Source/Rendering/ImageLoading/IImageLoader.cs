namespace BabyBearsEngine.Source.Rendering.ImageLoading;

internal interface IImageLoader
{
    Rgba8ImageData GetImageData(string path);
}

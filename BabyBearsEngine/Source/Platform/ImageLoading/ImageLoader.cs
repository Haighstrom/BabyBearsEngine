using System.IO;
using StbImageSharp;

namespace BabyBearsEngine.Platform.ImageLoading;

public static class ImageLoader
{
    public static Rgba8ImageData LoadAsRgba8(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        return new Rgba8ImageData(image.Width, image.Height, image.Data);
    }
}

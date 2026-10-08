using System.IO;
using StbImageWriteSharp;

namespace BabyBearsEngine.Platform.ImageLoading;

public static class ImageSaver
{
    /// <summary>
    /// Writes tightly-packed, top-row-first RGBA8 pixel data to <paramref name="filePath"/> as a PNG,
    /// overwriting any existing file.
    /// </summary>
    public static void SaveAsPng(Rgba8ImageData imageData, string filePath)
    {
        using FileStream stream = File.Create(filePath);
        ImageWriter writer = new();
        writer.WritePng(imageData.Data, imageData.Width, imageData.Height, ColorComponents.RedGreenBlueAlpha, stream);
    }
}

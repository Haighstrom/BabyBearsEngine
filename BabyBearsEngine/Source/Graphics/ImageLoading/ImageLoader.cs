using SixLabors.ImageSharp.PixelFormats;

namespace BabyBearsEngine.Source.Graphics.ImageLoading;

public static class ImageLoader
{
    public static Rgba8ImageData LoadAsRgba8(string filePath)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath);

        int width = image.Width;
        int height = image.Height;

        byte[] pixelBytes = new byte[width * height * 4];
        image.CopyPixelDataTo(pixelBytes);

        return new Rgba8ImageData(width, height, pixelBytes);
    }
}

namespace BabyBearsEngine.Platform.ImageLoading;

internal static class Rgba8Tools
{
    // Multiplies each pixel's RGB channels by its alpha in place. Required before uploading
    // textures used with the engine's (GL_ONE, GL_ONE_MINUS_SRC_ALPHA) premultiplied-alpha
    // blend mode — without it, partially-transparent pixels render as too-bright halos.
    public static void PremultiplyAlphaInPlace(byte[] rgbaData)
    {
        ArgumentNullException.ThrowIfNull(rgbaData);

        if (rgbaData.Length % 4 != 0)
        {
            throw new ArgumentException("Buffer length must be a multiple of 4 (RGBA8).", nameof(rgbaData));
        }

        for (int byteIndex = 0; byteIndex < rgbaData.Length; byteIndex += 4)
        {
            byte alpha = rgbaData[byteIndex + 3];
            if (alpha == 255)
            {
                continue;
            }

            rgbaData[byteIndex] = (byte)(rgbaData[byteIndex] * alpha / 255);
            rgbaData[byteIndex + 1] = (byte)(rgbaData[byteIndex + 1] * alpha / 255);
            rgbaData[byteIndex + 2] = (byte)(rgbaData[byteIndex + 2] * alpha / 255);
        }
    }
}

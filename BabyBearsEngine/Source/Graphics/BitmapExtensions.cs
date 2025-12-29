namespace BabyBearsEngine.Source.Graphics;

internal static class BitmapExtensions
{
    public static (int x, int y, int width, int height) NonZeroAlphaRegion(this System.Drawing.Bitmap bitmap)
    {
        int firstPixelX = bitmap.Width;
        int lastPixelX = 0;
        int firstPixelY = bitmap.Height;
        int lastPixelY = 0;

        for (int i = 0; i < bitmap.Width; i++)
            for (int j = 0; j < bitmap.Height; j++)
            {
                if (bitmap.GetPixel(i, j).A > 0)
                {
                    if (i < firstPixelX)
                    { 
                        firstPixelX = i; 
                    }
                    if (i > lastPixelX)
                    {
                        lastPixelX = i;
                    }
                    if (j < firstPixelY)
                    {
                        firstPixelY = j;
                    }
                    if (j > lastPixelY)
                    {
                        lastPixelY = j;
                    }
                }
            }

        if (firstPixelX <= lastPixelX && firstPixelY <= lastPixelY)
        {
            return (firstPixelX, firstPixelY, lastPixelX - firstPixelX + 1, lastPixelY - firstPixelY + 1);
        }
        else
        {
            return default;
        }
    }
}

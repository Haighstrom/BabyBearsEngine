namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKScreenCaptureAdapter(OpenTKGameEngine engine) : IScreenCapture
{
    private Colour[,] _frame = new Colour[0, 0];
    private Colour[] _readBuffer = [];

    public Colour[,] LatestFrame => _frame;

    public void CaptureCurrentBackbuffer()
    {
        int width = engine.ClientSize.X;
        int height = engine.ClientSize.Y;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        EnsureBuffersSized(width, height);

        GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
        GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, _readBuffer);

        for (int row = 0; row < height; row++)
        {
            int srcStart = (height - 1 - row) * width;

            for (int col = 0; col < width; col++)
            {
                _frame[row, col] = _readBuffer[srcStart + col];
            }
        }
    }

    public Colour GetPixel(int x, int y)
    {
        int height = _frame.GetLength(0);
        int width = _frame.GetLength(1);

        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"Pixel ({x},{y}) is outside the captured frame area ({width}x{height}). Has a frame been captured yet?");
        }

        return _frame[y, x];
    }

    private void EnsureBuffersSized(int width, int height)
    {
        if (_frame.GetLength(0) != height || _frame.GetLength(1) != width)
        {
            _frame = new Colour[height, width];
        }

        int total = width * height;

        if (_readBuffer.Length != total)
        {
            _readBuffer = new Colour[total];
        }
    }
}

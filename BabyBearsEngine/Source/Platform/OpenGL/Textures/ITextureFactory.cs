namespace BabyBearsEngine.OpenGL;

public interface ITextureFactory
{
    ITexture CreateTextureFromImageFile(string filePath, bool linearFilter = true);
    ISpriteTexture CreateSpriteTextureFromImageFile(string filePath, int rows, int columns, bool linearFilter = false);
    ITexture GenBorderedRectangle(int width, int height, int borderThickness, Colour fillColour, Colour borderColour);
    ITexture GenTexture(System.Drawing.Bitmap bmp, bool linearFilter = true);
    ITexture GenTexture(Colour[,] pixels, bool linearFilter = true);
}

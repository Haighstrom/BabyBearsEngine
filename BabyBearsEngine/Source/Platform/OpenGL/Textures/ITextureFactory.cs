namespace BabyBearsEngine.OpenGL;

public interface ITextureFactory
{
    ITexture CreateTextureFromImageFile(string filePath);
    ISpriteTexture CreateSpriteTextureFromImageFile(string filePath, int columns, int rows);
    ITexture GenBorderedRectangle(int width, int height, int borderThickness, Colour fillColour, Colour borderColour);
    ITexture GenTexture(System.Drawing.Bitmap bmp);
    ITexture GenTexture(Colour[,] pixels);
}

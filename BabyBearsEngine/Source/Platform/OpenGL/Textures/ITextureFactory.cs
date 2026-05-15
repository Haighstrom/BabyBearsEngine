namespace BabyBearsEngine.OpenGL;

public interface ITextureFactory
{
    ITexture CreateTextureFromImageFile(string filePath);
    ITexture GenBorderedRectangle(int width, int height, int borderThickness, Colour fillColour, Colour borderColour);
    ITexture GenTexture(System.Drawing.Bitmap bmp);
    ITexture GenTexture(Colour[,] pixels);
}

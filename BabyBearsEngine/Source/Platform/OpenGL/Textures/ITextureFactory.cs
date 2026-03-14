namespace BabyBearsEngine.OpenGL;

public interface ITextureFactory
{
    ITexture CreateTextureFromImageFile(string filePath);
    ITexture GenTexture(System.Drawing.Bitmap bmp);
}

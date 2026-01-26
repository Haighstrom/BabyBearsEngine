namespace BabyBearsEngine.OpenGL;

internal interface ITextureFactory
{
    ITexture CreateTextureFromImageFile(string filePath);
    ITexture GenTexture(System.Drawing.Bitmap bmp);
}

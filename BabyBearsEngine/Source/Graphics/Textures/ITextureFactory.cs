namespace BabyBearsEngine.Source.Graphics.Textures;

internal interface ITextureFactory
{
    ITexture CreateTextureFromImageFile(string filePath);
    ITexture GenTexture(System.Drawing.Bitmap bmp);
}

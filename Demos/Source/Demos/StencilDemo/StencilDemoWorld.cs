using System;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.StencilDemo;

internal class StencilDemoWorld : DemoWorld
{
    private const int PanelW = 200;
    private const int PanelH = 180;
    private const int PanelGap = 40;
    private const int LeftMargin = 20;
    private const int Row1Y = 70;
    private const int LabelH = 20;
    private const int Row2Y = Row1Y + PanelH + LabelH + 25;
    private const int TextureSize = 256;
    private const int TileSize = 32;

    public override string Name => "Stencil";

    public StencilDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(80, 80, 80);

        AddGeneratedExample(Row1Y);
        AddImageFileExample(Row2Y);
    }

    private void AddGeneratedExample(int rowY)
    {
        var image = Textures.CreateTexture(MakeCheckerboard(TextureSize, TextureSize, TileSize));
        var mask = Textures.CreateTexture(MakeCircleMask(TextureSize));
        AddRow(rowY, image, mask, "Checkerboard (generated)", "Circle mask (generated)", "Result");
    }

    private void AddImageFileExample(int rowY)
    {
        var image = Textures.CreateFromFile("Assets/Checkerboard.jpg");
        var mask = Textures.CreateFromFile("Assets/Jigsaw.jpg");
        AddRow(rowY, image, mask, "Checkerboard (file)", "Jigsaw mask (file)", "Result", threshold: 0.5f);
    }

    private void AddRow(int rowY, ITexture image, ITexture mask, string imageLabel, string maskLabel, string resultLabel, float threshold = 0f)
    {
        int imageX = LeftMargin;
        int resultX = imageX + PanelW + PanelGap;
        int maskX = resultX + PanelW + PanelGap;
        int labelY = rowY + PanelH + 5;

        Add(new TextureGraphic(image, imageX, rowY, PanelW, PanelH));
        Add(MakeLabel(imageLabel, imageX, labelY));

        Add(new StencilGraphic(image, mask, resultX, rowY, PanelW, PanelH) { Threshold = threshold });
        Add(MakeLabel(resultLabel, resultX, labelY));

        Add(new TextureGraphic(mask, maskX, rowY, PanelW, PanelH));
        Add(MakeLabel(maskLabel, maskX, labelY));
    }

    private static TextGraphic MakeLabel(string text, float x, float y)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 14), text, Colour.White, x, y, PanelW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Top,
        };
    }

    // White tiles on black background — red channel is 1.0 inside tiles, 0.0 outside.
    internal static Colour[,] MakeCheckerboard(int width, int height, int tileSize)
    {
        var pixels = new Colour[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool light = ((x / tileSize) + (y / tileSize)) % 2 == 0;
                pixels[x, y] = light ? new Colour(255, 220, 80) : new Colour(60, 100, 220);
            }
        }

        return pixels;
    }

    // White (R=1) inside the circle, black (R=0) outside — red channel used as mask, matching
    // the convention of the Jigsaw.jpg stencil (white = show, black = discard).
    internal static Colour[,] MakeCircleMask(int size)
    {
        var pixels = new Colour[size, size];
        float cx = (size - 1) / 2f;
        float cy = (size - 1) / 2f;
        float radius = (size - 1) / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                bool inside = MathF.Sqrt(dx * dx + dy * dy) <= radius;
                pixels[x, y] = inside ? new Colour(255, 255, 255) : new Colour(0, 0, 0);
            }
        }

        return pixels;
    }
}

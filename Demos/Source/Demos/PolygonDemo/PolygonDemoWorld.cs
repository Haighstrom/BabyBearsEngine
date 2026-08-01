using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.PolygonDemo;

internal class PolygonDemoWorld : DemoWorld
{
    private static readonly Colour[] s_palette = [Colour.MediumTurquoise, Colour.Orange, Colour.BlueViolet];

    private static readonly FontDefinition s_font = new("Times New Roman", 13);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private readonly PolygonGraphic[] _polygons = new PolygonGraphic[3];
    private int _paletteOffset = 0;

    public PolygonDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(240, 240, 240);

        Add(new TextGraphic(s_titleFont, "Polygon Demo", Colour.DimGray, 0f, 50f, 800f, 24f)
        {
            HAlignment = HAlignment.Centred,
        });
        Add(new TextGraphic(s_font, "A list of points, auto-closed and triangulated (ear clipping) to fill", Colour.DimGray, 0f, 88f, 800f, 18f)
        {
            HAlignment = HAlignment.Centred,
        });

        AddCaption(20f, 130f, "Hexagon (convex)");
        AddCaption(300f, 130f, "Star (concave)");
        AddCaption(560f, 130f, "L-shape (concave)");

        _polygons[0] = new PolygonGraphic(HexagonPoints(), s_palette[0]);
        _polygons[1] = new PolygonGraphic(StarPoints(), s_palette[1]);
        _polygons[2] = new PolygonGraphic(LShapePoints(), s_palette[2]);
        foreach (PolygonGraphic polygon in _polygons)
        {
            Add(polygon);
        }

        Button cycleColour = new(350f, 520f, 100f, 30f, ButtonTheme.FromColour(new Colour(80, 120, 200)), "Cycle Colour");
        cycleColour.LeftClicked += (_, _) => CyclePalette();
        Add(cycleColour);
    }

    public override string Name => "Polygon";

    private static Point[] HexagonPoints()
    {
        const int SideCount = 6;
        Point centre = new(150f, 330f);
        const float Radius = 130f;

        Point[] points = new Point[SideCount];
        for (int sideIndex = 0; sideIndex < SideCount; sideIndex++)
        {
            float angleRadians = (sideIndex * (360f / SideCount) - 90f) * MathF.PI / 180f;
            points[sideIndex] = new Point(centre.X + Radius * MathF.Cos(angleRadians), centre.Y + Radius * MathF.Sin(angleRadians));
        }

        return points;
    }

    private static Point[] LShapePoints() =>
    [
        new(600f, 190f), new(780f, 190f), new(780f, 330f),
        new(690f, 330f), new(690f, 470f), new(600f, 470f),
    ];

    private static Point[] StarPoints()
    {
        const int PointCount = 5;
        Point centre = new(400f, 330f);
        const float OuterRadius = 130f;
        const float InnerRadius = 52f;

        Point[] points = new Point[PointCount * 2];
        for (int vertexIndex = 0; vertexIndex < points.Length; vertexIndex++)
        {
            float radius = vertexIndex % 2 == 0 ? OuterRadius : InnerRadius;
            float angleRadians = (vertexIndex * (360f / points.Length) - 90f) * MathF.PI / 180f;
            points[vertexIndex] = new Point(centre.X + radius * MathF.Cos(angleRadians), centre.Y + radius * MathF.Sin(angleRadians));
        }

        return points;
    }

    private void AddCaption(float x, float y, string text)
    {
        Add(new TextGraphic(s_font, text, new Colour(110, 110, 110), x, y, 240f, 16f)
        {
            HAlignment = HAlignment.Centred,
        });
    }

    private void CyclePalette()
    {
        _paletteOffset = (_paletteOffset + 1) % s_palette.Length;
        for (int polygonIndex = 0; polygonIndex < _polygons.Length; polygonIndex++)
        {
            _polygons[polygonIndex].Colour = s_palette[(polygonIndex + _paletteOffset) % s_palette.Length];
        }
    }
}

using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

/// <summary>
/// Demonstrates GridLayout with Fixed and Weighted columns, padding, gaps, and
/// alignment modes. A top 3×3 grid shows all nine point-alignment modes; a bottom
/// 1-row weighted grid shows proportional column sizing.
/// </summary>
internal class GridLayoutDemoWorld : DemoWorld
{
    private static readonly FontDefinition LabelFont = new("Times New Roman", 14);
    private static readonly FontDefinition HeadingFont = new("Times New Roman", 20);

    public override string Name => "Grid Layout";

    public GridLayoutDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        AddHeading();
        AddAlignmentGrid();
        AddWeightedGrid();
    }

    private void AddHeading()
    {
        Add(new TextGraphic(HeadingFont, "GridLayout demo", Colour.Black, 0, 30, 800, 30)
        {
            HAlignment = HAlignment.Centred,
        });
    }

    // 3×3 grid with fixed columns — demonstrates nine point-alignment options.
    // Each cell is 80×80; children are 60×40 so the placement is clearly visible.
    private void AddAlignmentGrid()
    {
        Add(MakeLabel(20, 75, 760, 20,
            "Fixed 3x3 (5 px padding, 4 px gap) -- nine alignment modes"));

        GridCellSize[] cols =
        [
            GridCellSize.Fixed(220f),
            GridCellSize.Fixed(220f),
            GridCellSize.Fixed(220f),
        ];
        GridCellSize[] rows =
        [
            GridCellSize.Fixed(80f),
            GridCellSize.Fixed(80f),
            GridCellSize.Fixed(80f),
        ];

        var grid = new GridLayout(20f, 95f, 680f, 268f, cols, rows, padding: 5f, gap: 4f);
        grid.Add(new ColourGraphic(new Colour(220, 220, 225), 0f, 0f, 680f, 268f));
        Add(grid);

        (GridAlignment alignment, string label)[] cells =
        [
            (GridAlignment.TopLeft,    "TopLeft"),
            (GridAlignment.TopCenter,  "TopCenter"),
            (GridAlignment.TopRight,   "TopRight"),
            (GridAlignment.MiddleLeft, "MiddleLeft"),
            (GridAlignment.Center,     "Center"),
            (GridAlignment.MiddleRight,"MiddleRight"),
            (GridAlignment.BottomLeft, "BottomLeft"),
            (GridAlignment.BottomCenter,"BottomCenter"),
            (GridAlignment.BottomRight,"BottomRight"),
        ];

        foreach (var (alignment, label) in cells)
        {
            grid.AddChild(MakeFixedCell(label), alignment);
        }
    }

    // 1-row weighted grid — columns sized 1:2:1, showing proportional distribution.
    private void AddWeightedGrid()
    {
        Add(MakeLabel(20, 375, 760, 20,
            "Weighted columns (1 : 2 : 1), Fill alignment, 8 px gap"));

        GridCellSize[] cols =
        [
            GridCellSize.Weighted(1f),
            GridCellSize.Weighted(2f),
            GridCellSize.Weighted(1f),
        ];
        GridCellSize[] rows = [GridCellSize.Fixed(80f)];

        var grid = new GridLayout(20f, 400f, 680f, 80f, cols, rows, gap: 8f);
        Add(grid);

        (Colour colour, string label)[] cells =
        [
            (new Colour(255, 180, 180), "Weight 1"),
            (new Colour(180, 220, 160), "Weight 2  (double width)"),
            (new Colour(180, 180, 255), "Weight 1"),
        ];

        foreach (var (colour, label) in cells)
        {
            grid.AddChild(new ResizablePanel(colour, label, LabelFont), GridAlignment.Fill);
        }
    }

    // Fixed-size 60×40 blue tile — used in the alignment grid where the cell is 80×80.
    private static Entity MakeFixedCell(string text)
    {
        Entity cell = new(0f, 0f, 60f, 40f);
        cell.Add(new ColourGraphic(new Colour(80, 140, 210), 0f, 0f, 60f, 40f));
        cell.Add(new TextGraphic(LabelFont, text, Colour.White, 0f, 0f, 60f, 40f)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
        return cell;
    }

    private static TextGraphic MakeLabel(int x, int y, int w, int h, string text)
    {
        return new TextGraphic(LabelFont, text, Colour.Black, x, y, w, h)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }

    // Entity that propagates its own size changes to its background and label children,
    // so it renders correctly when GridLayout sets its Width/Height via Fill alignment.
    private sealed class ResizablePanel : Entity
    {
        private readonly ColourGraphic _background;
        private readonly TextGraphic _label;

        public ResizablePanel(Colour colour, string text, FontDefinition font)
            : base(0f, 0f, 1f, 1f)
        {
            _background = new ColourGraphic(colour, 0f, 0f, 1f, 1f);
            _label = new TextGraphic(font, text, Colour.Black, 0f, 0f, 1f, 1f)
            {
                HAlignment = HAlignment.Centred,
                VAlignment = VAlignment.Centred,
            };
            Add(_background);
            Add(_label);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            _background.Width  = Width;
            _background.Height = Height;
            _label.Width  = Width;
            _label.Height = Height;
        }
    }
}

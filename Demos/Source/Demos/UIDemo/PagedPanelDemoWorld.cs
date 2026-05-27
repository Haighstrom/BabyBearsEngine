using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class PagedPanelDemoWorld : DemoWorld
{
    private static readonly Colour[] s_pageColours =
    [
        new("#cfe8ff"),
        new("#d6f5d6"),
        new("#ffe3cf"),
        new("#f3d6f5"),
    ];

    private static readonly Rect s_pageArea = new(190, 110, 420, 230);
    private static readonly Rect s_previousButton = new(190, 360, 50, 40);
    private static readonly Rect s_pageCounter = new(270, 362, 260, 36);
    private static readonly Rect s_nextButton = new(560, 360, 50, 40);

    private readonly TextGraphic _statusLabel;

    public override string Name => "Paged Panel";

    public PagedPanelDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeLabel(20, 55, 760, 40, fontSize: 18,
            "PagedPanel shows one page at a time. Use the buttons to flip through."));

        List<Entity> pages = [];

        for (int pageNumber = 0; pageNumber < s_pageColours.Length; pageNumber++)
        {
            Entity page = new(0, 0, 0, 0);
            page.Add(new Panel(s_pageArea, s_pageColours[pageNumber]));
            page.Add(MakeLabel(s_pageArea.X, s_pageArea.Y, s_pageArea.W, s_pageArea.H, fontSize: 48, $"Page {pageNumber + 1}"));
            pages.Add(page);
        }

        PagedPanel pagedPanel = new(
            pages,
            s_previousButton, s_nextButton, s_pageCounter,
            ButtonTheme.Default,
            ButtonTheme.Default,
            TextTheme.Default,
            previousButtonText: "<",
            nextButtonText: ">");
        pagedPanel.PageChanged += OnPageChanged;
        Add(pagedPanel);

        _statusLabel = MakeLabel(s_pageArea.X, 415, s_pageArea.W, 30, fontSize: 16, "Showing page 1.");
        Add(_statusLabel);
    }

    private void OnPageChanged(object? sender, PageChangedEventArgs e)
    {
        _statusLabel.Text = $"Page changed from {e.PreviousPage + 1} to {e.CurrentPage + 1}.";
    }

    private static TextGraphic MakeLabel(float x, float y, float width, float height, int fontSize, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", fontSize), text, Colour.Black, x, y, width, height)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
    }
}

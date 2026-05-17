using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.ScrollListDemo;

internal class ScrollListDemoWorld : DemoWorld
{
    private const float ItemHeight = 44f;
    private const float PanelHeight = 300f;
    private const float PanelWidth = 300f;
    private const float PanelX = 250f;
    private const float PanelY = 130f;
    private const int TotalItems = 15;

    private static readonly FontDefinition s_font = new("Times New Roman", 15);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 18);

    private static readonly (string Label, Colour Bg, Colour Fg)[] s_items =
    [
        ("Albatross",    new Colour(200, 210, 220), Colour.Black),
        ("Blue Jay",     new Colour(60,  110, 190), Colour.White),
        ("Canary",       new Colour(240, 220, 50),  Colour.Black),
        ("Dove",         new Colour(210, 200, 195), Colour.Black),
        ("Eagle",        new Colour(100, 70,  30),  Colour.White),
        ("Flamingo",     new Colour(240, 130, 160), Colour.White),
        ("Goldfinch",    new Colour(210, 185, 40),  Colour.Black),
        ("Hummingbird",  new Colour(60,  170, 130), Colour.White),
        ("Ibis",         new Colour(230, 90,  80),  Colour.White),
        ("Jay",          new Colour(80,  120, 200), Colour.White),
        ("Kingfisher",   new Colour(40,  130, 170), Colour.White),
        ("Lark",         new Colour(160, 140, 90),  Colour.White),
        ("Magpie",       new Colour(40,  40,  40),  Colour.White),
        ("Nightingale",  new Colour(130, 105, 70),  Colour.White),
        ("Owl",          new Colour(100, 85,  55),  Colour.White),
    ];

    public ScrollListDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(230, 230, 230);

        Add(new TextGraphic(s_titleFont, "Scrolling List Demo", Colour.DimGray, 0f, 50f, 800f, 26f)
        {
            HAlignment = HAlignment.Centred,
        });

        Add(new TextGraphic(s_font, "Drag the scrollbar thumb to scroll", new Colour(100, 100, 100),
            0f, 82f, 800f, 20f)
        {
            HAlignment = HAlignment.Centred,
        });

        ScrollingListPanelTheme theme = ScrollingListPanelTheme.FromColours(
            background: new Colour(45, 45, 45),
            track: new Colour(30, 30, 30),
            thumb: new Colour(130, 130, 130));

        ScrollingListPanel panel = new(PanelX, PanelY, PanelWidth, PanelHeight, theme);

        float itemWidth = PanelWidth - 20f; // leave room for scrollbar
        for (int i = 0; i < TotalItems; i++)
        {
            var (label, bg, fg) = s_items[i];
            float y = i * ItemHeight;

            TextLabel item = new(0f, y, itemWidth, ItemHeight - 2f,
                new TextTheme(s_font, fg),
                $"{i + 1,2}. {label}",
                backgroundColour: bg);

            panel.AddItem(item);
        }

        panel.ContentHeight = TotalItems * ItemHeight;
        Add(panel);
    }

    public override string Name => "Scroll List Demo";
}

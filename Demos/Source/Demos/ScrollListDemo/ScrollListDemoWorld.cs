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
        ("Apple",      new Colour(220, 80,  80),  Colour.White),
        ("Banana",     new Colour(220, 200, 60),  Colour.Black),
        ("Cherry",     new Colour(180, 40,  80),  Colour.White),
        ("Dragonfruit",new Colour(230, 80,  160), Colour.White),
        ("Elderberry", new Colour(90,  40,  120), Colour.White),
        ("Fig",        new Colour(100, 70,  50),  Colour.White),
        ("Grape",      new Colour(100, 60,  160), Colour.White),
        ("Honeydew",   new Colour(160, 210, 120), Colour.Black),
        ("Ita Palm",   new Colour(50,  130, 80),  Colour.White),
        ("Jackfruit",  new Colour(210, 150, 40),  Colour.Black),
        ("Kiwi",       new Colour(110, 160, 60),  Colour.Black),
        ("Lemon",      new Colour(240, 230, 60),  Colour.Black),
        ("Mango",      new Colour(230, 140, 50),  Colour.Black),
        ("Nectarine",  new Colour(220, 100, 60),  Colour.White),
        ("Orange",     new Colour(230, 130, 30),  Colour.Black),
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

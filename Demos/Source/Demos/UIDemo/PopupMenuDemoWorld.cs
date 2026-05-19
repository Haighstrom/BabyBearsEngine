using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.UIDemo;

internal class PopupMenuDemoWorld : DemoWorld
{
    private const int PopupX = 190;
    private const int PopupY = 85;
    private const int PopupW = 420;
    private const int PopupH = 300;

    private const int ButtonY = 430;
    private const int ButtonW = 200;
    private const int ButtonH = 42;

    public override string Name => "Popup Menus";

    public PopupMenuDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        Add(MakeText(0, 20, 800, 30, "Popup Menus", 22, Colour.Black, HAlignment.Centred));
        Add(MakeText(0, 58, 800, 22, "Each button opens its menu and closes any other — click again to toggle closed.", 14, new Colour(80, 80, 80), HAlignment.Centred));

        Popup statsPopup = BuildStatsPopup();
        Popup inventoryPopup = BuildInventoryPopup();
        Popup mapPopup = BuildMapPopup();

        MenuGroup menuGroup = new();
        menuGroup.Register(statsPopup, inventoryPopup, mapPopup);

        // Popups live on the overlay so they render above all other widgets.
        Overlay.Add(statsPopup, inventoryPopup, mapPopup);

        Button statsButton = new(55, ButtonY, ButtonW, ButtonH,
            ButtonTheme.FromColour(new Colour(130, 160, 215)), "Character Stats");
        Button inventoryButton = new(300, ButtonY, ButtonW, ButtonH,
            ButtonTheme.FromColour(new Colour(110, 185, 130)), "Inventory");
        Button mapButton = new(545, ButtonY, ButtonW, ButtonH,
            ButtonTheme.FromColour(new Colour(210, 175, 100)), "World Map");
        Button closeAllButton = new(310, 495, 180, 36,
            ButtonTheme.FromColour(new Colour(200, 100, 100)), "Close All");

        statsButton.LeftClicked     += (_, _) => menuGroup.Toggle(statsPopup);
        inventoryButton.LeftClicked += (_, _) => menuGroup.Toggle(inventoryPopup);
        mapButton.LeftClicked       += (_, _) => menuGroup.Toggle(mapPopup);
        closeAllButton.LeftClicked  += (_, _) => menuGroup.CloseAll();

        Add(statsButton, inventoryButton, mapButton, closeAllButton);
    }

    private static Popup BuildStatsPopup()
    {
        Popup popup = new(PopupX, PopupY, PopupW, PopupH, new Colour(75, 105, 160));
        popup.Add(MakeText(0, 8, PopupW, 36, "Character Stats", 20, Colour.White, HAlignment.Centred));
        AddRow(popup, 60,  "Strength", "14");
        AddRow(popup, 102, "Defence",  "8");
        AddRow(popup, 144, "Magic",    "3");
        AddRow(popup, 186, "Speed",    "11");
        AddRow(popup, 228, "HP",       "84 / 100");
        return popup;
    }

    private static Popup BuildInventoryPopup()
    {
        Popup popup = new(PopupX, PopupY, PopupW, PopupH, new Colour(65, 130, 80));
        popup.Add(MakeText(0, 8, PopupW, 36, "Inventory", 20, Colour.White, HAlignment.Centred));
        AddRow(popup, 60,  "Iron Sword",     "equipped");
        AddRow(popup, 102, "Leather Shield", "equipped");
        AddRow(popup, 144, "Health Potion",  "x3");
        AddRow(popup, 186, "Magic Scroll",   "x1");
        AddRow(popup, 228, "Gold",           "247");
        return popup;
    }

    private static Popup BuildMapPopup()
    {
        Popup popup = new(PopupX, PopupY, PopupW, PopupH, new Colour(155, 120, 55));
        popup.Add(MakeText(0, 8, PopupW, 36, "World Map", 20, Colour.White, HAlignment.Centred));
        AddRow(popup, 60,  "Current zone",  "Thornwood Forest");
        AddRow(popup, 102, "Next zone",     "Dragon's Peak");
        AddRow(popup, 144, "Waypoints set", "2");
        AddRow(popup, 186, "Quests active", "3");
        return popup;
    }

    private static void AddRow(Popup popup, int y, string label, string value)
    {
        int col1W = PopupW / 2 - 20;
        popup.Add(MakeText(20, y, col1W, 34, label, 16, Colour.White, HAlignment.Left));
        popup.Add(MakeText(20 + col1W, y, col1W, 34, value, 16, Colour.White, HAlignment.Right));
    }

    private static TextGraphic MakeText(int x, int y, int w, int h, string text, int size, Colour colour, HAlignment hAlign)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", size), text, colour, x, y, w, h)
        {
            HAlignment = hAlign,
            VAlignment = VAlignment.Centred,
        };
    }
}

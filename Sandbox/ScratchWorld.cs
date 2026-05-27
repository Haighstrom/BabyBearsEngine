using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Sandbox;

// Alpha / transparency investigation.
//
// Row 1 - baseline: transparent quad directly on top of grey background.
//   Expected: grey shows through (discard preserves grey in framebuffer).
//   This tests whether alpha=0 -> discard is working at all.
//
// Row 2 - the TextLabel bug scenario: grey background, then black full rect on top,
//   then transparent inset rect on top of that (mirroring what BorderedRectangleGraphic does).
//   Expected (naive): grey center, black frame.
//   Actual: solid black — the transparent inset can only reveal what is currently in the
//   framebuffer at those pixels, which is already black from the border quad.
//
// Row 3 - the fix: same as row 2 but fill colour = grey (opaque) instead of transparent.
//   Expected: grey center, black frame. This is what BorderedRectangleGraphic needs.
//
// Row 4 - semi-transparency check: red rect at 50% opacity over grey.
//   Expected: pinkish-grey blend (premultiplied alpha blending should work here).

internal class ScratchWorld : World
{
    private const int RowY = 80;
    private const int RowSpacing = 80;
    private const int RectX = 50;
    private const int RectW = 300;
    private const int RectH = 50;
    private const int BorderThickness = 8;

    public ScratchWorld()
    {
        BackgroundColour = new Colour(200, 200, 200);

        AddRow(0, "Transparent on green (expect: green)");
        AddRow(1, "Transparent inset over black (expect: solid black - bug)");
        AddRow(2, "Opaque inset over black (expect: frame - fix)");
        AddRow(3, "50% red on green (expect: blend)");
    }

    private void AddRow(int row, string label)
    {
        int y = RowY + row * RowSpacing;
        FontDefinition font = new("Arial", 11);
        Add(new TextGraphic(font, label, Colour.Black, RectX, y - 18, RectW, 18));

        switch (row)
        {
            case 0:
                // Grey base, then transparent quad on top — discard should leave grey visible.
                Add(new ColourGraphic(new Colour(100, 180, 100), RectX, y, RectW, RectH));
                Add(new ColourGraphic(new Colour(0, 0, 0, 0), RectX, y, RectW, RectH));
                break;

            case 1:
                // Grey base → black full rect → transparent inset. Mirrors the old TextLabel bug.
                Add(new ColourGraphic(new Colour(100, 180, 100), RectX, y, RectW, RectH));
                Add(new ColourGraphic(Colour.Black, RectX, y, RectW, RectH));
                Add(new ColourGraphic(new Colour(0, 0, 0, 0),
                    RectX + BorderThickness, y + BorderThickness,
                    RectW - 2 * BorderThickness, RectH - 2 * BorderThickness));
                break;

            case 2:
                // Grey base → black full rect → opaque-grey inset. The correct fix.
                Add(new ColourGraphic(new Colour(100, 180, 100), RectX, y, RectW, RectH));
                Add(new ColourGraphic(Colour.Black, RectX, y, RectW, RectH));
                Add(new ColourGraphic(new Colour(100, 180, 100),
                    RectX + BorderThickness, y + BorderThickness,
                    RectW - 2 * BorderThickness, RectH - 2 * BorderThickness));
                break;

            case 3:
                // Grey base, then 50% red on top. Tests partial alpha blending.
                Add(new ColourGraphic(new Colour(100, 180, 100), RectX, y, RectW, RectH));
                Add(new ColourGraphic(new Colour(255, 0, 0, 128), RectX, y, RectW, RectH));
                break;
        }
    }
}

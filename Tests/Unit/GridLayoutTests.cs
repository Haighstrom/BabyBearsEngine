using System;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class GridLayoutTests
{
    private const float Delta = 0.0001f;

    // -------------------------------------------------------------------------
    // ComputeSizes — fixed only

    [TestMethod]
    public void ComputeSizes_AllFixed_ReturnExactValues()
    {
        GridCellSize[] specs = [GridCellSize.Fixed(100f), GridCellSize.Fixed(200f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 400f, gap: 0f);

        Assert.AreEqual(100f, sizes[0], Delta);
        Assert.AreEqual(200f, sizes[1], Delta);
    }

    [TestMethod]
    public void ComputeSizes_AllFixed_GapDoesNotAffectFixedSizes()
    {
        GridCellSize[] specs = [GridCellSize.Fixed(50f), GridCellSize.Fixed(50f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 200f, gap: 10f);

        Assert.AreEqual(50f, sizes[0], Delta);
        Assert.AreEqual(50f, sizes[1], Delta);
    }

    // -------------------------------------------------------------------------
    // ComputeSizes — weighted only

    [TestMethod]
    public void ComputeSizes_EqualWeights_SplitAvailableEvenly()
    {
        GridCellSize[] specs = [GridCellSize.Weighted(1f), GridCellSize.Weighted(1f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 200f, gap: 0f);

        Assert.AreEqual(100f, sizes[0], Delta);
        Assert.AreEqual(100f, sizes[1], Delta);
    }

    [TestMethod]
    public void ComputeSizes_TwoToOne_SecondIsHalfFirst()
    {
        GridCellSize[] specs = [GridCellSize.Weighted(2f), GridCellSize.Weighted(1f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 300f, gap: 0f);

        Assert.AreEqual(200f, sizes[0], Delta);
        Assert.AreEqual(100f, sizes[1], Delta);
    }

    [TestMethod]
    public void ComputeSizes_WeightedWithGap_GapDeducedBeforeWeightedDistribution()
    {
        // 2 columns, 10px gap → 190px available for cells; split evenly → 95 each
        GridCellSize[] specs = [GridCellSize.Weighted(1f), GridCellSize.Weighted(1f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 200f, gap: 10f);

        Assert.AreEqual(95f, sizes[0], Delta);
        Assert.AreEqual(95f, sizes[1], Delta);
    }

    // -------------------------------------------------------------------------
    // ComputeSizes — mixed

    [TestMethod]
    public void ComputeSizes_FixedAndWeighted_FixedFirst_WeightedGetsRemainder()
    {
        GridCellSize[] specs = [GridCellSize.Fixed(60f), GridCellSize.Weighted(1f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 200f, gap: 0f);

        Assert.AreEqual(60f, sizes[0], Delta);
        Assert.AreEqual(140f, sizes[1], Delta);
    }

    [TestMethod]
    public void ComputeSizes_TwoWeightedOnFixed_WeightedShareRemainder()
    {
        // Fixed(100) leaves 200 for two Weighted(1) columns: 100 each
        GridCellSize[] specs =
        [
            GridCellSize.Weighted(1f),
            GridCellSize.Fixed(100f),
            GridCellSize.Weighted(1f),
        ];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 300f, gap: 0f);

        Assert.AreEqual(100f, sizes[0], Delta);
        Assert.AreEqual(100f, sizes[1], Delta);
        Assert.AreEqual(100f, sizes[2], Delta);
    }

    [TestMethod]
    public void ComputeSizes_SingleFixed_ReturnsExactSize()
    {
        GridCellSize[] specs = [GridCellSize.Fixed(75f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 200f, gap: 0f);

        Assert.AreEqual(75f, sizes[0], Delta);
    }

    [TestMethod]
    public void ComputeSizes_SingleWeighted_GetsEntireAvailable()
    {
        GridCellSize[] specs = [GridCellSize.Weighted(1f)];
        float[] sizes = GridLayout.ComputeSizes(specs, available: 300f, gap: 0f);

        Assert.AreEqual(300f, sizes[0], Delta);
    }

    // -------------------------------------------------------------------------
    // ComputeOffsets

    [TestMethod]
    public void ComputeOffsets_NoPaddingNoGap_StartsAtZero()
    {
        float[] sizes = [100f, 200f];
        float[] offsets = GridLayout.ComputeOffsets(sizes, padding: 0f, gap: 0f);

        Assert.AreEqual(0f, offsets[0], Delta);
        Assert.AreEqual(100f, offsets[1], Delta);
    }

    [TestMethod]
    public void ComputeOffsets_WithPadding_FirstOffsetEqualsPadding()
    {
        float[] sizes = [80f, 80f];
        float[] offsets = GridLayout.ComputeOffsets(sizes, padding: 10f, gap: 0f);

        Assert.AreEqual(10f, offsets[0], Delta);
        Assert.AreEqual(90f, offsets[1], Delta);
    }

    [TestMethod]
    public void ComputeOffsets_WithGap_OffsetsIncludeGap()
    {
        float[] sizes = [50f, 50f, 50f];
        float[] offsets = GridLayout.ComputeOffsets(sizes, padding: 0f, gap: 5f);

        Assert.AreEqual(0f,   offsets[0], Delta);
        Assert.AreEqual(55f,  offsets[1], Delta);
        Assert.AreEqual(110f, offsets[2], Delta);
    }

    [TestMethod]
    public void ComputeOffsets_WithPaddingAndGap_CombinesCorrectly()
    {
        float[] sizes = [100f, 100f];
        float[] offsets = GridLayout.ComputeOffsets(sizes, padding: 20f, gap: 10f);

        Assert.AreEqual(20f,  offsets[0], Delta);
        Assert.AreEqual(130f, offsets[1], Delta);
    }

    // -------------------------------------------------------------------------
    // GridLayout construction

    [TestMethod]
    public void Constructor_StoresDimensions()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f), GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(50f)];
        var grid = new GridLayout(10f, 20f, 200f, 50f, cols, rows);

        Assert.AreEqual(2, grid.ColumnCount);
        Assert.AreEqual(1, grid.RowCount);
        Assert.AreEqual(10f, grid.X, Delta);
        Assert.AreEqual(20f, grid.Y, Delta);
    }

    // -------------------------------------------------------------------------
    // AddChild explicit placement — Fill alignment

    [TestMethod]
    public void AddChild_FillAlignment_SetsChildToFullCellSize()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(200f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 1f, 1f);

        grid.AddChild(child, col: 0, row: 0, GridAlignment.Fill);

        Assert.AreEqual(0f,   child.X,      Delta);
        Assert.AreEqual(0f,   child.Y,      Delta);
        Assert.AreEqual(200f, child.Width,  Delta);
        Assert.AreEqual(100f, child.Height, Delta);
    }

    [TestMethod]
    public void AddChild_SecondColumn_CorrectXOffset()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f), GridCellSize.Fixed(150f)];
        GridCellSize[] rows = [GridCellSize.Fixed(80f)];
        var grid = new GridLayout(0f, 0f, 250f, 80f, cols, rows);
        var child = new Entity(0f, 0f, 1f, 1f);

        grid.AddChild(child, col: 1, row: 0);

        Assert.AreEqual(100f, child.X, Delta);
        Assert.AreEqual(150f, child.Width, Delta);
    }

    [TestMethod]
    public void AddChild_SecondRow_CorrectYOffset()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(60f), GridCellSize.Fixed(90f)];
        var grid = new GridLayout(0f, 0f, 100f, 150f, cols, rows);
        var child = new Entity(0f, 0f, 1f, 1f);

        grid.AddChild(child, col: 0, row: 1);

        Assert.AreEqual(60f, child.Y,      Delta);
        Assert.AreEqual(90f, child.Height, Delta);
    }

    // -------------------------------------------------------------------------
    // AddChild explicit placement — alignment variants

    [TestMethod]
    public void AddChild_CenterAlignment_CentersChildInCell()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(200f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 40f, 20f);

        grid.AddChild(child, 0, 0, GridAlignment.Center);

        Assert.AreEqual(80f, child.X, Delta);
        Assert.AreEqual(40f, child.Y, Delta);
        Assert.AreEqual(40f, child.Width,  Delta);
        Assert.AreEqual(20f, child.Height, Delta);
    }

    [TestMethod]
    public void AddChild_TopLeft_PlacesAtCellOrigin()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(200f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 30f, 20f);

        grid.AddChild(child, 0, 0, GridAlignment.TopLeft);

        Assert.AreEqual(0f,  child.X, Delta);
        Assert.AreEqual(0f,  child.Y, Delta);
        Assert.AreEqual(30f, child.Width,  Delta);
        Assert.AreEqual(20f, child.Height, Delta);
    }

    [TestMethod]
    public void AddChild_BottomRight_PlacesAtCellBottomRight()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(200f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 30f, 20f);

        grid.AddChild(child, 0, 0, GridAlignment.BottomRight);

        Assert.AreEqual(170f, child.X, Delta);
        Assert.AreEqual(80f,  child.Y, Delta);
    }

    [TestMethod]
    public void AddChild_StretchHorizontally_StretchesWidthCentersHeight()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(200f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 50f, 40f);

        grid.AddChild(child, 0, 0, GridAlignment.StretchHorizontally);

        Assert.AreEqual(0f,   child.X,      Delta);
        Assert.AreEqual(30f,  child.Y,      Delta);
        Assert.AreEqual(200f, child.Width,  Delta);
        Assert.AreEqual(40f,  child.Height, Delta);
    }

    [TestMethod]
    public void AddChild_StretchVertically_StretchesHeightCentersWidth()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(200f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 50f, 40f);

        grid.AddChild(child, 0, 0, GridAlignment.StretchVertically);

        Assert.AreEqual(75f,  child.X,      Delta);
        Assert.AreEqual(0f,   child.Y,      Delta);
        Assert.AreEqual(50f,  child.Width,  Delta);
        Assert.AreEqual(100f, child.Height, Delta);
    }

    // -------------------------------------------------------------------------
    // AddChild auto-placement

    [TestMethod]
    public void AddChild_AutoPlacement_FillsLeftToRightThenTopToBottom()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f), GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(50f), GridCellSize.Fixed(50f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);

        var a = new Entity(0f, 0f, 1f, 1f);
        var b = new Entity(0f, 0f, 1f, 1f);
        var c = new Entity(0f, 0f, 1f, 1f);
        var d = new Entity(0f, 0f, 1f, 1f);

        grid.AddChild(a);
        grid.AddChild(b);
        grid.AddChild(c);
        grid.AddChild(d);

        // a → (0,0), b → (1,0), c → (0,1), d → (1,1)
        Assert.AreEqual(0f,   a.X, Delta);
        Assert.AreEqual(0f,   a.Y, Delta);
        Assert.AreEqual(100f, b.X, Delta);
        Assert.AreEqual(0f,   b.Y, Delta);
        Assert.AreEqual(0f,   c.X, Delta);
        Assert.AreEqual(50f,  c.Y, Delta);
        Assert.AreEqual(100f, d.X, Delta);
        Assert.AreEqual(50f,  d.Y, Delta);
    }

    [TestMethod]
    public void AddChild_AutoPlacement_AllCellsFilled_Throws()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 100f, 100f, cols, rows);
        grid.AddChild(new Entity(0f, 0f, 1f, 1f));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => grid.AddChild(new Entity(0f, 0f, 1f, 1f)));
    }

    // -------------------------------------------------------------------------
    // AddChild out-of-range

    [TestMethod]
    public void AddChild_ColOutOfRange_Throws()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 100f, 100f, cols, rows);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => grid.AddChild(new Entity(0f, 0f, 1f, 1f), col: 1, row: 0));
    }

    [TestMethod]
    public void AddChild_RowOutOfRange_Throws()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 100f, 100f, cols, rows);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => grid.AddChild(new Entity(0f, 0f, 1f, 1f), col: 0, row: 1));
    }

    // -------------------------------------------------------------------------
    // Padding and gap

    [TestMethod]
    public void AddChild_WithPadding_CellOriginOffsetByPadding()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(180f)];
        GridCellSize[] rows = [GridCellSize.Fixed(80f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows, padding: 10f);
        var child = new Entity(0f, 0f, 1f, 1f);

        grid.AddChild(child, 0, 0, GridAlignment.Fill);

        Assert.AreEqual(10f,  child.X,      Delta);
        Assert.AreEqual(10f,  child.Y,      Delta);
        Assert.AreEqual(180f, child.Width,  Delta);
        Assert.AreEqual(80f,  child.Height, Delta);
    }

    [TestMethod]
    public void AddChild_WithGap_SecondCellOffsetByGap()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(90f), GridCellSize.Fixed(90f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 190f, 100f, cols, rows, gap: 10f);
        var childA = new Entity(0f, 0f, 1f, 1f);
        var childB = new Entity(0f, 0f, 1f, 1f);

        grid.AddChild(childA, 0, 0);
        grid.AddChild(childB, 1, 0);

        Assert.AreEqual(0f,   childA.X, Delta);
        Assert.AreEqual(100f, childB.X, Delta);
    }

    // -------------------------------------------------------------------------
    // Weighted columns with actual grid

    [TestMethod]
    public void AddChild_WeightedColumns_ChildWidthMatchesWeightedSize()
    {
        // 400px wide, two equal Weighted cols → 200 each
        GridCellSize[] cols = [GridCellSize.Weighted(1f), GridCellSize.Weighted(1f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 400f, 100f, cols, rows);
        var childA = new Entity(0f, 0f, 1f, 1f);
        var childB = new Entity(0f, 0f, 1f, 1f);

        grid.AddChild(childA, 0, 0);
        grid.AddChild(childB, 1, 0);

        Assert.AreEqual(200f, childA.Width, Delta);
        Assert.AreEqual(200f, childB.Width, Delta);
        Assert.AreEqual(0f,   childA.X,     Delta);
        Assert.AreEqual(200f, childB.X,     Delta);
    }

    // -------------------------------------------------------------------------
    // RemoveChild / RemoveAllChildren

    [TestMethod]
    public void RemoveChild_ChildNoLongerReplacedOnResize()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 100f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 1f, 1f);
        grid.AddChild(child, 0, 0);

        grid.RemoveChild(child);
        child.X = 999f;

        // Resize the grid — if child were still tracked its X would be reset
        grid.Width = 200f;

        Assert.AreEqual(999f, child.X, Delta);
    }

    [TestMethod]
    public void RemoveAllChildren_ResetsAutoPlacementCursor()
    {
        GridCellSize[] cols = [GridCellSize.Fixed(100f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 100f, 100f, cols, rows);
        grid.AddChild(new Entity(0f, 0f, 1f, 1f));

        grid.RemoveAllChildren();

        // After reset, first cell should be usable again
        var fresh = new Entity(0f, 0f, 1f, 1f);
        grid.AddChild(fresh);

        Assert.AreEqual(0f, fresh.X, Delta);
        Assert.AreEqual(0f, fresh.Y, Delta);
    }

    // -------------------------------------------------------------------------
    // OnSizeChanged

    [TestMethod]
    public void SizeChange_RecalculatesChildPositions()
    {
        GridCellSize[] cols = [GridCellSize.Weighted(1f)];
        GridCellSize[] rows = [GridCellSize.Fixed(100f)];
        var grid = new GridLayout(0f, 0f, 200f, 100f, cols, rows);
        var child = new Entity(0f, 0f, 1f, 1f);
        grid.AddChild(child, 0, 0, GridAlignment.Fill);

        grid.Width = 400f;

        Assert.AreEqual(400f, child.Width, Delta);
    }
}

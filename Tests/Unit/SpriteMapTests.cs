using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class SpriteMapTests
{
    // ComputeVisibleTileRange is the testable core of the DrawArea culling; SpriteMap itself
    // allocates GL resources in its constructor and so can't be exercised in a unit test.

    [TestMethod]
    public void ComputeVisibleTileRange_NullDrawArea_ReturnsFullGrid()
    {
        var range = SpriteMap.ComputeVisibleTileRange(drawArea: null, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(10, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(8, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_AreaCoveringWholeMap_ReturnsFullGrid()
    {
        Rect area = new(0, 0, 160, 128);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(10, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(8, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_AreaLargerThanMap_ClampsToMap()
    {
        Rect area = new(-100, -100, 1000, 1000);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(10, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(8, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_PartialOverlap_ReturnsClippedRange()
    {
        // Area covers x [20, 60), y [10, 50) — tiles are 16×16.
        // Cols: floor(20/16)=1, ceil(60/16)=4  → [1, 4)
        // Rows: floor(10/16)=0, ceil(50/16)=4  → [0, 4)
        Rect area = new(20, 10, 40, 40);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(1, range.colMin);
        Assert.AreEqual(4, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(4, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_AreaAlignedToTileBoundaries_DoesNotIncludeAdjacentTiles()
    {
        // Area covers exactly tiles (col 2..3) × (row 1..2) — half-open boundary at tile edges.
        Rect area = new(32, 16, 32, 32);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(2, range.colMin);
        Assert.AreEqual(4, range.colExclusiveMax);
        Assert.AreEqual(1, range.rowMin);
        Assert.AreEqual(3, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_AreaCompletelyOutsideMap_ReturnsEmpty()
    {
        // Sits beyond the right edge of the 10×8 grid (which ends at x=160).
        Rect area = new(200, 0, 50, 50);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(0, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(0, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_NegativeArea_ReturnsEmpty()
    {
        // Sits entirely before the top-left of the map.
        Rect area = new(-100, -100, 50, 50);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(0, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(0, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_ZeroWidthArea_ReturnsEmpty()
    {
        Rect area = new(20, 20, 0, 40);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(0, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(0, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_ZeroHeightArea_ReturnsEmpty()
    {
        Rect area = new(20, 20, 40, 0);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(0, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(0, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_PartialTileOverlap_IncludesTouchedTile()
    {
        // Area covers x [15, 17) — overlaps tile col 0 (right edge) and tile col 1 (left edge).
        Rect area = new(15, 15, 2, 2);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 10, rows: 8, tileWidth: 16, tileHeight: 16);

        Assert.AreEqual(0, range.colMin);
        Assert.AreEqual(2, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(2, range.rowExclusiveMax);
    }

    [TestMethod]
    public void ComputeVisibleTileRange_NonSquareTiles_UsesCorrectAxisScale()
    {
        // Tiles are 32×8 here. Area covers x [40, 96) y [4, 20).
        // Cols: floor(40/32)=1, ceil(96/32)=3  → [1, 3)
        // Rows: floor(4/8)=0,  ceil(20/8)=3   → [0, 3)
        Rect area = new(40, 4, 56, 16);

        var range = SpriteMap.ComputeVisibleTileRange(area, columns: 5, rows: 5, tileWidth: 32, tileHeight: 8);

        Assert.AreEqual(1, range.colMin);
        Assert.AreEqual(3, range.colExclusiveMax);
        Assert.AreEqual(0, range.rowMin);
        Assert.AreEqual(3, range.rowExclusiveMax);
    }
}

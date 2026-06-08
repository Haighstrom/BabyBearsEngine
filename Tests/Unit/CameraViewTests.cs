using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CameraViewTests
{
    // WorldToLocal — tested via FixedTileSizeCameraView (concrete, no OpenGL)

    [TestMethod]
    public void WorldToLocal_AtOrigin_NoScroll_NoScale_ReturnsWorldPoint()
    {
        FixedTileSizeCameraView view = new(1f, 1f, () => 100f, () => 100f);

        var (x, y) = view.WorldToLocal(10f, 5f);

        Assert.AreEqual(10f, x);
        Assert.AreEqual(5f, y);
    }

    [TestMethod]
    public void WorldToLocal_WithTileScale_ScalesCoordinates()
    {
        FixedTileSizeCameraView view = new(2f, 3f, () => 800f, () => 600f);

        var (x, y) = view.WorldToLocal(10f, 5f);

        Assert.AreEqual(20f, x);
        Assert.AreEqual(15f, y);
    }

    [TestMethod]
    public void WorldToLocal_WithScroll_OffsetsByScrollPosition()
    {
        FixedTileSizeCameraView view = new(1f, 1f, () => 100f, () => 100f);
        view.X = 5f;
        view.Y = 3f;

        var (x, y) = view.WorldToLocal(10f, 5f);

        Assert.AreEqual(5f, x);
        Assert.AreEqual(2f, y);
    }

    [TestMethod]
    public void WorldToLocal_WithScrollAndScale_AppliesBoth()
    {
        FixedTileSizeCameraView view = new(2f, 2f, () => 800f, () => 600f);
        view.X = 5f;
        view.Y = 3f;

        var (x, y) = view.WorldToLocal(10f, 5f);

        Assert.AreEqual(10f, x);
        Assert.AreEqual(4f, y);
    }

    [TestMethod]
    public void WorldToLocal_PointAtViewOrigin_ReturnsZero()
    {
        FixedTileSizeCameraView view = new(32f, 32f, () => 800f, () => 600f);
        view.X = 7f;
        view.Y = 4f;

        var (x, y) = view.WorldToLocal(7f, 4f);

        Assert.AreEqual(0f, x);
        Assert.AreEqual(0f, y);
    }

    // FixedTileSizeCameraView

    [TestMethod]
    public void FixedTileSize_ViewWidth_ReturnsCameraWidthDividedByTileWidth()
    {
        FixedTileSizeCameraView view = new(32f, 16f, () => 800f, () => 600f);

        Assert.AreEqual(25f, view.ViewWidth);
    }

    [TestMethod]
    public void FixedTileSize_ViewHeight_ReturnsCameraHeightDividedByTileHeight()
    {
        FixedTileSizeCameraView view = new(32f, 20f, () => 800f, () => 600f);

        Assert.AreEqual(30f, view.ViewHeight);
    }

    [TestMethod]
    public void FixedTileSize_ViewWidthUpdatesWhenCameraWidthChanges()
    {
        float cameraW = 800f;
        FixedTileSizeCameraView view = new(32f, 32f, () => cameraW, () => 600f);

        cameraW = 640f;

        Assert.AreEqual(20f, view.ViewWidth);
    }

    [TestMethod]
    public void FixedTileSize_SetTileWidth_RaisesViewChanged()
    {
        FixedTileSizeCameraView view = new(32f, 32f, () => 800f, () => 600f);
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.TileWidth = 16f;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void FixedTileSize_SetTileHeight_RaisesViewChanged()
    {
        FixedTileSizeCameraView view = new(32f, 32f, () => 800f, () => 600f);
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.TileHeight = 16f;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void FixedTileSize_SetX_RaisesViewChanged()
    {
        FixedTileSizeCameraView view = new(32f, 32f, () => 800f, () => 600f);
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.X = 10f;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void FixedTileSize_SetX_ToSameValue_DoesNotRaiseViewChanged()
    {
        FixedTileSizeCameraView view = new(32f, 32f, () => 800f, () => 600f);
        view.X = 10f;
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.X = 10f;

        Assert.AreEqual(0, raised);
    }

    [TestMethod]
    public void FixedTileSize_SetY_ToSameValue_DoesNotRaiseViewChanged()
    {
        FixedTileSizeCameraView view = new(32f, 32f, () => 800f, () => 600f);
        view.Y = 10f;
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.Y = 10f;

        Assert.AreEqual(0, raised);
    }

    // FreeCameraView

    [TestMethod]
    public void Free_TileWidth_ReturnsCameraWidthDividedByViewWidth()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);

        Assert.AreEqual(2f, view.TileWidth);
    }

    [TestMethod]
    public void Free_TileHeight_ReturnsCameraHeightDividedByViewHeight()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);

        Assert.AreEqual(2f, view.TileHeight);
    }

    [TestMethod]
    public void Free_SetTileWidth_UpdatesViewWidth()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);

        view.TileWidth = 4f;

        Assert.AreEqual(200f, view.ViewWidth);
    }

    [TestMethod]
    public void Free_SetTileHeight_UpdatesViewHeight()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);

        view.TileHeight = 3f;

        Assert.AreEqual(200f, view.ViewHeight);
    }

    [TestMethod]
    public void Free_SetViewWidth_UpdatesTileWidth()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);

        view.ViewWidth = 200f;

        Assert.AreEqual(4f, view.TileWidth);
    }

    [TestMethod]
    public void Free_SetViewHeight_UpdatesTileHeight()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);

        view.ViewHeight = 200f;

        Assert.AreEqual(3f, view.TileHeight);
    }

    [TestMethod]
    public void Free_SetViewWidth_RaisesViewChanged()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.ViewWidth = 200f;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void Free_SetViewHeight_RaisesViewChanged()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.ViewHeight = 200f;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void Free_SetX_RaisesViewChanged()
    {
        FreeCameraView view = new(0f, 0f, 400f, 300f, () => 800f, () => 600f);
        int raised = 0;
        view.ViewChanged += (_, _) => raised++;

        view.X = 50f;

        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void Free_ConstructorSetsScrollPosition()
    {
        FreeCameraView view = new(10f, 20f, 400f, 300f, () => 800f, () => 600f);

        Assert.AreEqual(10f, view.X);
        Assert.AreEqual(20f, view.Y);
    }
}

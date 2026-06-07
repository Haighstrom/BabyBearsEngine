using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class PanelGraphicTests
{
    private const int TextureSize = 150;
    private const float BorderSize = 50f;

    private static PanelSlice[] NewLayoutBuffer() => new PanelSlice[9];

    [TestMethod]
    public void ComputeSliceLayout_FillsAllNineSlots()
    {
        PanelSlice[] layout = NewLayoutBuffer();

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 200f, panelHeight: 200f,
            BorderSize, TextureSize, TextureSize);

        // Sanity: every slot was assigned non-default UVs spanning [0, 1].
        for (int sliceIndex = 0; sliceIndex < 9; sliceIndex++)
        {
            Assert.IsGreaterThanOrEqualTo(0f, layout[sliceIndex].U1);
            Assert.IsLessThanOrEqualTo(1f, layout[sliceIndex].U2);
            Assert.IsGreaterThanOrEqualTo(0f, layout[sliceIndex].V1);
            Assert.IsLessThanOrEqualTo(1f, layout[sliceIndex].V2);
        }
    }

    [TestMethod]
    public void ComputeSliceLayout_AtNativeSize_PaintsTextureOneForOne()
    {
        PanelSlice[] layout = NewLayoutBuffer();

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: TextureSize, panelHeight: TextureSize,
            BorderSize, TextureSize, TextureSize);

        // The middle slice should be the central third in both source and destination.
        PanelSlice middle = layout[4];
        Assert.AreEqual(BorderSize, middle.X);
        Assert.AreEqual(BorderSize, middle.Y);
        Assert.AreEqual(BorderSize, middle.Width);
        Assert.AreEqual(BorderSize, middle.Height);
        Assert.AreEqual(1f / 3f, middle.U1, delta: 1e-5f);
        Assert.AreEqual(2f / 3f, middle.U2, delta: 1e-5f);
        Assert.AreEqual(1f / 3f, middle.V1, delta: 1e-5f);
        Assert.AreEqual(2f / 3f, middle.V2, delta: 1e-5f);
    }

    [TestMethod]
    public void ComputeSliceLayout_CornersKeepBorderSize_RegardlessOfPanelSize()
    {
        PanelSlice[] layout = NewLayoutBuffer();

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 400f, panelHeight: 250f,
            BorderSize, TextureSize, TextureSize);

        // Top-left, top-right, bottom-left, bottom-right corners — all BorderSize × BorderSize.
        PanelSlice topLeft = layout[0];
        PanelSlice topRight = layout[2];
        PanelSlice bottomLeft = layout[6];
        PanelSlice bottomRight = layout[8];

        Assert.AreEqual(BorderSize, topLeft.Width);
        Assert.AreEqual(BorderSize, topLeft.Height);
        Assert.AreEqual(BorderSize, topRight.Width);
        Assert.AreEqual(BorderSize, topRight.Height);
        Assert.AreEqual(BorderSize, bottomLeft.Width);
        Assert.AreEqual(BorderSize, bottomLeft.Height);
        Assert.AreEqual(BorderSize, bottomRight.Width);
        Assert.AreEqual(BorderSize, bottomRight.Height);
    }

    [TestMethod]
    public void ComputeSliceLayout_TopRightCorner_SitsAtRightEdge()
    {
        PanelSlice[] layout = NewLayoutBuffer();
        const float panelWidth = 400f;

        PanelGraphic.ComputeSliceLayout(layout, panelWidth, panelHeight: 250f,
            BorderSize, TextureSize, TextureSize);

        PanelSlice topRight = layout[2];
        Assert.AreEqual(panelWidth - BorderSize, topRight.X);
        Assert.AreEqual(0f, topRight.Y);
    }

    [TestMethod]
    public void ComputeSliceLayout_BottomLeftCorner_SitsAtBottomEdge()
    {
        PanelSlice[] layout = NewLayoutBuffer();
        const float panelHeight = 250f;

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 400f, panelHeight,
            BorderSize, TextureSize, TextureSize);

        PanelSlice bottomLeft = layout[6];
        Assert.AreEqual(0f, bottomLeft.X);
        Assert.AreEqual(panelHeight - BorderSize, bottomLeft.Y);
    }

    [TestMethod]
    public void ComputeSliceLayout_TopEdge_StretchesHorizontallyKeepsCornerHeight()
    {
        PanelSlice[] layout = NewLayoutBuffer();
        const float panelWidth = 400f;

        PanelGraphic.ComputeSliceLayout(layout, panelWidth, panelHeight: 250f,
            BorderSize, TextureSize, TextureSize);

        PanelSlice topMiddle = layout[1];
        Assert.AreEqual(BorderSize, topMiddle.X);
        Assert.AreEqual(0f, topMiddle.Y);
        Assert.AreEqual(panelWidth - 2f * BorderSize, topMiddle.Width);
        Assert.AreEqual(BorderSize, topMiddle.Height);
    }

    [TestMethod]
    public void ComputeSliceLayout_LeftEdge_StretchesVerticallyKeepsCornerWidth()
    {
        PanelSlice[] layout = NewLayoutBuffer();
        const float panelHeight = 250f;

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 400f, panelHeight,
            BorderSize, TextureSize, TextureSize);

        PanelSlice middleLeft = layout[3];
        Assert.AreEqual(0f, middleLeft.X);
        Assert.AreEqual(BorderSize, middleLeft.Y);
        Assert.AreEqual(BorderSize, middleLeft.Width);
        Assert.AreEqual(panelHeight - 2f * BorderSize, middleLeft.Height);
    }

    [TestMethod]
    public void ComputeSliceLayout_MiddleSlice_FillsRemainingInterior()
    {
        PanelSlice[] layout = NewLayoutBuffer();
        const float panelWidth = 400f;
        const float panelHeight = 250f;

        PanelGraphic.ComputeSliceLayout(layout, panelWidth, panelHeight,
            BorderSize, TextureSize, TextureSize);

        PanelSlice middle = layout[4];
        Assert.AreEqual(BorderSize, middle.X);
        Assert.AreEqual(BorderSize, middle.Y);
        Assert.AreEqual(panelWidth - 2f * BorderSize, middle.Width);
        Assert.AreEqual(panelHeight - 2f * BorderSize, middle.Height);
    }

    [TestMethod]
    public void ComputeSliceLayout_AdjacentSlicesShareBoundariesWithNoGap()
    {
        PanelSlice[] layout = NewLayoutBuffer();

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 320f, panelHeight: 240f,
            BorderSize, TextureSize, TextureSize);

        // Top row left+middle: left's right edge meets middle's left edge.
        Assert.AreEqual(layout[0].X + layout[0].Width, layout[1].X);
        // Top row middle+right.
        Assert.AreEqual(layout[1].X + layout[1].Width, layout[2].X);
        // Left column top+middle.
        Assert.AreEqual(layout[0].Y + layout[0].Height, layout[3].Y);
        // Left column middle+bottom.
        Assert.AreEqual(layout[3].Y + layout[3].Height, layout[6].Y);
    }

    [TestMethod]
    public void ComputeSliceLayout_PanelNarrowerThanTwoBorders_MiddleColumnIsZeroWidth()
    {
        PanelSlice[] layout = NewLayoutBuffer();
        // Panel width 80, two borders of 50 each would need 100 — middle column should clamp to 0.
        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 80f, panelHeight: 200f,
            BorderSize, TextureSize, TextureSize);

        Assert.AreEqual(0f, layout[1].Width);
        Assert.AreEqual(0f, layout[4].Width);
        Assert.AreEqual(0f, layout[7].Width);
    }

    [TestMethod]
    public void ComputeSliceLayout_PanelShorterThanTwoBorders_MiddleRowIsZeroHeight()
    {
        PanelSlice[] layout = NewLayoutBuffer();

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 200f, panelHeight: 80f,
            BorderSize, TextureSize, TextureSize);

        Assert.AreEqual(0f, layout[3].Height);
        Assert.AreEqual(0f, layout[4].Height);
        Assert.AreEqual(0f, layout[5].Height);
    }

    [TestMethod]
    public void ComputeSliceLayout_NonSquareTexture_ProducesCorrectUvBoundaries()
    {
        PanelSlice[] layout = NewLayoutBuffer();
        const int textureWidth = 300;
        const int textureHeight = 120;
        const float borderSize = 30f;

        PanelGraphic.ComputeSliceLayout(layout, panelWidth: 200f, panelHeight: 200f,
            borderSize, textureWidth, textureHeight);

        PanelSlice topLeft = layout[0];
        Assert.AreEqual(0f, topLeft.U1);
        Assert.AreEqual(borderSize / textureWidth, topLeft.U2, delta: 1e-6f);
        Assert.AreEqual(0f, topLeft.V1);
        Assert.AreEqual(borderSize / textureHeight, topLeft.V2, delta: 1e-6f);

        PanelSlice bottomRight = layout[8];
        Assert.AreEqual((textureWidth - borderSize) / textureWidth, bottomRight.U1, delta: 1e-6f);
        Assert.AreEqual(1f, bottomRight.U2);
        Assert.AreEqual((textureHeight - borderSize) / textureHeight, bottomRight.V1, delta: 1e-6f);
        Assert.AreEqual(1f, bottomRight.V2);
    }
}

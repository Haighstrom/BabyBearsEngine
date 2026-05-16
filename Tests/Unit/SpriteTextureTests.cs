using BabyBearsEngine.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class SpriteTextureTests
{
    private const double Delta = 1e-5;

    private sealed class StubTexture(int width, int height) : ITexture
    {
        public int Handle => 0;
        public int Width { get; } = width;
        public int Height { get; } = height;
        public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0) { }
        public void Dispose() { }
    }

    // With padding = 0 the behaviour matches simple 1/columns * 1/rows grid

    [TestMethod]
    public void GetFrameUVs_NoPadding_SingleFrame_CoversFullTexture()
    {
        SpriteTexture st = new(new StubTexture(100, 100), 1, 1, 0);

        var (u1, v1, u2, v2) = st.GetFrameUVs(0);

        Assert.AreEqual(0f, u1, Delta);
        Assert.AreEqual(0f, v1, Delta);
        Assert.AreEqual(1f, u2, Delta);
        Assert.AreEqual(1f, v2, Delta);
    }

    [TestMethod]
    public void GetFrameUVs_NoPadding_2x2_Frame0_IsTopLeft()
    {
        SpriteTexture st = new(new StubTexture(200, 200), 2, 2, 0);

        var (u1, v1, u2, v2) = st.GetFrameUVs(0);

        Assert.AreEqual(0f, u1, Delta);
        Assert.AreEqual(0f, v1, Delta);
        Assert.AreEqual(0.5f, u2, Delta);
        Assert.AreEqual(0.5f, v2, Delta);
    }

    [TestMethod]
    public void GetFrameUVs_NoPadding_2x2_Frame1_IsTopRight()
    {
        SpriteTexture st = new(new StubTexture(200, 200), 2, 2, 0);

        var (u1, v1, u2, v2) = st.GetFrameUVs(1);

        Assert.AreEqual(0.5f, u1, Delta);
        Assert.AreEqual(0f, v1, Delta);
        Assert.AreEqual(1f, u2, Delta);
        Assert.AreEqual(0.5f, v2, Delta);
    }

    [TestMethod]
    public void GetFrameUVs_NoPadding_2x2_Frame2_IsBottomLeft()
    {
        SpriteTexture st = new(new StubTexture(200, 200), 2, 2, 0);

        var (u1, v1, u2, v2) = st.GetFrameUVs(2);

        Assert.AreEqual(0f, u1, Delta);
        Assert.AreEqual(0.5f, v1, Delta);
        Assert.AreEqual(0.5f, u2, Delta);
        Assert.AreEqual(1f, v2, Delta);
    }

    // With padding, the UV calculations must account for the inset and reduced frame size

    [TestMethod]
    public void GetFrameUVs_WithPadding_SingleFrame_UVsAreInset()
    {
        // Texture: 104×104, 1 frame, 2px padding on each side → frame occupies [2..102, 2..102]
        SpriteTexture st = new(new StubTexture(104, 104), 1, 1, 2);

        var (u1, v1, u2, v2) = st.GetFrameUVs(0);

        Assert.AreEqual(2f / 104f, u1, Delta);
        Assert.AreEqual(2f / 104f, v1, Delta);
        Assert.AreEqual(102f / 104f, u2, Delta);
        Assert.AreEqual(102f / 104f, v2, Delta);
    }

    [TestMethod]
    public void GetFrameUVs_WithPadding_EachFrameHasCorrectWidth()
    {
        // 2 columns, each frame originally 50px wide, 2px padding → newW = 100 + 3*2 = 106
        // Frame UV width = 50/106
        SpriteTexture st = new(new StubTexture(106, 100), 2, 1, 2);

        float expectedFrameU = 50f / 106f;

        var (u1_0, _, u2_0, _) = st.GetFrameUVs(0);
        var (u1_1, _, u2_1, _) = st.GetFrameUVs(1);

        Assert.AreEqual(expectedFrameU, u2_0 - u1_0, Delta);
        Assert.AreEqual(expectedFrameU, u2_1 - u1_1, Delta);
    }

    [TestMethod]
    public void GetFrameUVs_WithPadding_AdjacentFramesDoNotOverlap()
    {
        SpriteTexture st = new(new StubTexture(106, 100), 2, 1, 2);

        var (_, _, u2_0, _) = st.GetFrameUVs(0);
        var (u1_1, _, _, _) = st.GetFrameUVs(1);

        Assert.IsTrue(u1_1 > u2_0, "Frame 1 UV start must come after frame 0 UV end.");
    }

    [TestMethod]
    public void GetFrameUVs_WithPadding_LastFrameEndDoesNotExceedOne()
    {
        SpriteTexture st = new(new StubTexture(106, 100), 2, 1, 2);

        var (_, _, u2, v2) = st.GetFrameUVs(1);

        Assert.IsTrue(u2 <= 1f + 1e-5f, "Last frame U2 must not exceed 1.");
        Assert.IsTrue(v2 <= 1f + 1e-5f, "Last frame V2 must not exceed 1.");
    }

    [TestMethod]
    public void FrameU_NoPadding_Equals1DividedByColumns()
    {
        SpriteTexture st = new(new StubTexture(200, 100), 4, 2, 0);

        Assert.AreEqual(1f / 4f, st.FrameU, Delta);
        Assert.AreEqual(1f / 2f, st.FrameV, Delta);
    }

    [TestMethod]
    public void FrameU_WithPadding_SmallerThan1DividedByColumns()
    {
        SpriteTexture st = new(new StubTexture(106, 100), 2, 1, 2);

        Assert.IsTrue(st.FrameU < 1f / 2f, "Padding must reduce FrameU below the unpadded value.");
    }
}

using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ISpriteSheetTests
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

    // A pure-data sprite sheet with no GPU surface — the whole point of the ISpriteSheet split.
    private sealed class FakeSpriteSheet(int columns, int rows) : ISpriteSheet
    {
        public int Columns { get; } = columns;
        public int Rows { get; } = rows;
        public int Frames => Columns * Rows;
        public float FrameU => 1f / Columns;
        public float FrameV => 1f / Rows;
        public (float U1, float V1, float U2, float V2) GetFrameUVs(int frame)
        {
            int col = frame % Columns;
            int row = frame / Columns;
            return (col * FrameU, row * FrameV, (col + 1) * FrameU, (row + 1) * FrameV);
        }
    }

    [TestMethod]
    public void ISpriteTexture_IsAssignableTo_ISpriteSheet()
    {
        // The split's central invariant: anything that's an ISpriteTexture is also an ISpriteSheet,
        // so consumers can treat textures as opaque grid handles.
        Assert.IsTrue(typeof(ISpriteSheet).IsAssignableFrom(typeof(ISpriteTexture)));
    }

    [TestMethod]
    public void SpriteTexture_UsedAsISpriteSheet_ExposesGridContract()
    {
        SpriteTexture concrete = new(new StubTexture(200, 200), columns: 2, rows: 2, padding: 0);

        ISpriteSheet sheet = concrete;

        Assert.AreEqual(2, sheet.Columns);
        Assert.AreEqual(2, sheet.Rows);
        Assert.AreEqual(4, sheet.Frames);
        Assert.AreEqual(0.5f, sheet.FrameU, Delta);
        Assert.AreEqual(0.5f, sheet.FrameV, Delta);

        var (u1, v1, u2, v2) = sheet.GetFrameUVs(0);
        Assert.AreEqual(0f, u1, Delta);
        Assert.AreEqual(0f, v1, Delta);
        Assert.AreEqual(0.5f, u2, Delta);
        Assert.AreEqual(0.5f, v2, Delta);
    }

    [TestMethod]
    public void ISpriteSheet_CanBeImplemented_WithNoGLDependency()
    {
        // If this class compiles and runs, ISpriteSheet has no ITexture members leaking in —
        // gameplay code can hold a sprite sheet without depending on the OpenGL namespace.
        ISpriteSheet sheet = new FakeSpriteSheet(columns: 3, rows: 2);

        Assert.AreEqual(6, sheet.Frames);

        var (u1, v1, u2, v2) = sheet.GetFrameUVs(5);

        Assert.AreEqual(2f / 3f, u1, Delta);
        Assert.AreEqual(0.5f, v1, Delta);
        Assert.AreEqual(1f, u2, Delta);
        Assert.AreEqual(1f, v2, Delta);
    }
}

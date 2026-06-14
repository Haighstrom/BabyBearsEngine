using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class IGraphicAlphaTests
{
    private sealed class FakeGraphic : IGraphic
    {
        public Colour Colour { get; set; } = Colour.White;
        public float Angle { get; set; } = 0f;
        public int Layer { get; set; } = 0;
        public bool Visible { get; set; } = true;
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Width { get; set; } = 0f;
        public float Height { get; set; } = 0f;
        public IContainer? Parent { get; set; } = null;
        public bool Exists => Parent is not null;

        public event EventHandler<LayerChangedEventArgs>? LayerChanged
        {
            add { }
            remove { }
        }

        public event EventHandler? Added
        {
            add { }
            remove { }
        }

        public event EventHandler? Removed
        {
            add { }
            remove { }
        }

        public void Remove() => throw new NotSupportedException();

        public void Render(ref Matrix3 projection, ref Matrix3 modelView) => throw new NotSupportedException();
    }

    [TestMethod]
    public void Alpha_Get_ReturnsColourAComponent()
    {
        IGraphic g = new FakeGraphic { Colour = new(10, 20, 30, 128) };

        Assert.AreEqual((byte)128, g.Alpha);
    }

    [TestMethod]
    public void Alpha_Get_OnDefaultWhite_Returns255()
    {
        IGraphic g = new FakeGraphic();

        Assert.AreEqual((byte)255, g.Alpha);
    }

    [TestMethod]
    public void Alpha_Set_To255_ProducesFullyOpaque()
    {
        IGraphic g = new FakeGraphic { Colour = new(10, 20, 30, 0) };

        g.Alpha = 255;

        Assert.AreEqual(new Colour(10, 20, 30, 255), g.Colour);
    }

    [TestMethod]
    public void Alpha_Set_To0_ProducesFullyTransparent()
    {
        IGraphic g = new FakeGraphic { Colour = new(10, 20, 30, 255) };

        g.Alpha = 0;

        Assert.AreEqual(new Colour(10, 20, 30, 0), g.Colour);
    }

    [TestMethod]
    public void Alpha_Set_PreservesRgbComponents()
    {
        IGraphic g = new FakeGraphic { Colour = new(11, 22, 33, 44) };

        g.Alpha = 128;

        Assert.AreEqual((byte)11, g.Colour.R);
        Assert.AreEqual((byte)22, g.Colour.G);
        Assert.AreEqual((byte)33, g.Colour.B);
    }

    [TestMethod]
    public void Alpha_Set_StoresByteVerbatim()
    {
        IGraphic g = new FakeGraphic();

        g.Alpha = 128;

        Assert.AreEqual((byte)128, g.Colour.A);
    }

    [TestMethod]
    public void Alpha_ReadModifyWrite_HalvesSymmetrically()
    {
        IGraphic g = new FakeGraphic { Colour = new(10, 20, 30, 200) };

        g.Alpha = (byte)(g.Alpha / 2);

        Assert.AreEqual((byte)100, g.Colour.A);
    }

    [TestMethod]
    public void Alpha_DimDispatch_WorksOnConcreteIGraphicImplementations()
    {
        FakeGraphic concrete = new() { Colour = new(50, 60, 70, 80) };
        IGraphic asInterface = concrete;

        asInterface.Alpha = 255;

        Assert.AreEqual((byte)255, concrete.Colour.A);
        Assert.AreEqual((byte)50, concrete.Colour.R);
    }
}

using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FadeOutGraphicTests
{
    private sealed class FakeGraphic : IGraphic
    {
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Width { get; set; } = 0f;
        public float Height { get; set; } = 0f;
        public Colour Colour { get; set; } = Colour.White;
        public bool Visible { get; set; } = true;
        public int Layer { get; set; } = 0;

        private IContainer? _parent = null;
        public IContainer? Parent
        {
            get => _parent;
            set
            {
                if (value is null)
                {
                    _parent = null;
                    Removed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _parent = value;
                    Added?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public bool Exists => _parent is not null;

        public event EventHandler<LayerChangedEventArgs>? LayerChanged;
        public event EventHandler? Added;
        public event EventHandler? Removed;

        public void Remove() => Parent?.Remove(this);
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private sealed class FakeContainer : IContainer
    {
        public List<IAddable> Added { get; } = [];
        public List<IAddable> Removed { get; } = [];

        public void Add(IAddable entity)
        {
            Added.Add(entity);
            entity.Parent = this;
        }

        public void Remove(IAddable entity)
        {
            Removed.Add(entity);
            entity.Parent = null;
        }

        public void RemoveAll() { }

        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    [TestMethod]
    public void Constructor_AdoptsGraphicPositionAsOwnPosition()
    {
        FakeGraphic graphic = new() { X = 100f, Y = 50f, Width = 30f, Height = 20f };

        FadeOutGraphic fade = new(graphic, velocityX: 0f, velocityY: -40f, duration: 1.0);

        Assert.AreEqual(100f, fade.X);
        Assert.AreEqual(50f, fade.Y);
    }

    [TestMethod]
    public void Constructor_AdoptsGraphicSizeAsOwnSize()
    {
        FakeGraphic graphic = new() { X = 0f, Y = 0f, Width = 30f, Height = 20f };

        FadeOutGraphic fade = new(graphic, 0f, 0f, 1.0);

        Assert.AreEqual(30f, fade.Width);
        Assert.AreEqual(20f, fade.Height);
    }

    [TestMethod]
    public void Constructor_ResetsGraphicLocalPositionToOrigin()
    {
        FakeGraphic graphic = new() { X = 100f, Y = 50f };

        _ = new FadeOutGraphic(graphic, 0f, 0f, 1.0);

        Assert.AreEqual(0f, graphic.X);
        Assert.AreEqual(0f, graphic.Y);
    }

    [TestMethod]
    public void Constructor_AddsGraphicAsChild()
    {
        FakeGraphic graphic = new() { Width = 10f, Height = 10f };

        FadeOutGraphic fade = new(graphic, 0f, 0f, 1.0);

        Assert.AreSame(fade, graphic.Parent);
    }

    [TestMethod]
    public void Constructor_AcceptsLayer()
    {
        FakeGraphic graphic = new() { Width = 10f, Height = 10f };

        FadeOutGraphic fade = new(graphic, 0f, 0f, 1.0, layer: 5);

        Assert.AreEqual(5, fade.Layer);
    }

    [TestMethod]
    public void Update_PropagatesDriftToGraphic()
    {
        FakeGraphic graphic = new() { X = 0f, Y = 0f, Width = 10f, Height = 10f };
        FadeOutGraphic fade = new(graphic, velocityX: 100f, velocityY: -50f, duration: 2.0);
        fade.Parent = new FakeContainer();

        fade.Update(0.1);

        Assert.AreEqual(10f, graphic.X, delta: 0.001f);
        Assert.AreEqual(-5f, graphic.Y, delta: 0.001f);
    }

    [TestMethod]
    public void Update_PropagatesFadeToGraphic()
    {
        FakeGraphic graphic = new() { Colour = Colour.White, Width = 10f, Height = 10f };
        FadeOutGraphic fade = new(graphic, 0f, 0f, duration: 1.0);
        fade.Parent = new FakeContainer();

        fade.Update(0.5);

        Assert.AreEqual(127, (int)graphic.Colour.A);
    }

    [TestMethod]
    public void Update_WhenDurationElapsed_RemovesSelfFromParent()
    {
        FakeGraphic graphic = new() { Width = 10f, Height = 10f };
        FadeOutGraphic fade = new(graphic, 0f, 0f, duration: 1.0);
        FakeContainer parent = new();
        parent.Add(fade);

        fade.Update(1.0);

        Assert.Contains(fade, parent.Removed);
        Assert.IsNull(fade.Parent);
    }

    [TestMethod]
    public void Update_BeforeDurationElapsed_DoesNotRemoveSelf()
    {
        FakeGraphic graphic = new() { Width = 10f, Height = 10f };
        FadeOutGraphic fade = new(graphic, 0f, 0f, duration: 1.0);
        FakeContainer parent = new();
        parent.Add(fade);

        fade.Update(0.5);

        Assert.IsEmpty(parent.Removed);
        Assert.AreSame(parent, fade.Parent);
    }
}

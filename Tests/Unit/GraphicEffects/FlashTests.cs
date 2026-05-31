using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.GraphicEffects;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit.GraphicEffects;

[TestClass]
public class FlashTests
{
    private sealed class FakeGraphic : IGraphic
    {
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Width { get; set; } = 0f;
        public float Height { get; set; } = 0f;
        public Colour Colour { get; set; } = Colour.White;
        public float Angle { get; set; } = 0f;
        public bool Visible { get; set; } = true;
        public int Layer { get; set; } = 0;
        public IContainer? Parent { get; set; } = new FakeContainer();
        public bool Exists => Parent is not null;

        public event EventHandler<LayerChangedEventArgs>? LayerChanged;

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

        public void Remove() => Parent = null;
        public void Render(ref Matrix3 projection, ref Matrix3 modelView) { }
    }

    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    [TestMethod]
    public void NewFlash_DoesNotChangeAlpha()
    {
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 0) };
        Flash flash = new(target);

        flash.Update(0.1);

        Assert.AreEqual(0, (int)target.Colour.A);
        Assert.IsFalse(flash.IsFlashing);
    }

    [TestMethod]
    public void Trigger_StartsRiseTowardPeakAlpha()
    {
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 0) };
        Flash flash = new(target)
        {
            RiseSeconds = 0.1f,
            FallSeconds = 1.0f,
            PeakAlpha = 200,
        };

        flash.Trigger();

        // Half-way through the rise — alpha should be ~half of peak (rest is 0).
        flash.Update(0.05);

        Assert.IsGreaterThan(70, (int)target.Colour.A);
        Assert.IsLessThan(130, (int)target.Colour.A);
        Assert.IsTrue(flash.IsFlashing);
    }

    [TestMethod]
    public void Trigger_AlphaPeaksAtRiseBoundary()
    {
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 0) };
        Flash flash = new(target)
        {
            RiseSeconds = 0.1f,
            FallSeconds = 1.0f,
            PeakAlpha = 200,
        };

        flash.Trigger();
        flash.Update(0.1); // exactly at the rise/fall boundary

        Assert.AreEqual(200, (int)target.Colour.A);
    }

    [TestMethod]
    public void Trigger_AlphaReturnsToRestAfterFullCycle()
    {
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 0) };
        Flash flash = new(target)
        {
            RiseSeconds = 0.05f,
            FallSeconds = 0.25f,
            PeakAlpha = 200,
        };

        flash.Trigger();
        flash.Update(0.05 + 0.25 + 0.01); // past the end of the fall

        Assert.AreEqual(0, (int)target.Colour.A);
        Assert.IsFalse(flash.IsFlashing);
    }

    [TestMethod]
    public void Trigger_PreservesRgbDuringFlash()
    {
        FakeGraphic target = new() { Colour = new Colour(180, 40, 60, 0) }; // red-ish damage tint
        Flash flash = new(target);

        flash.Trigger();
        flash.Update(0.05);

        Assert.AreEqual(180, (int)target.Colour.R);
        Assert.AreEqual(40, (int)target.Colour.G);
        Assert.AreEqual(60, (int)target.Colour.B);
    }

    [TestMethod]
    public void NonZeroRestAlpha_FlashRisesFromRestToPeak()
    {
        // Target starts visible at alpha 40 — a faint base glow that briefly intensifies.
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 40) };
        Flash flash = new(target)
        {
            RiseSeconds = 0.1f,
            FallSeconds = 1.0f,
            PeakAlpha = 200,
        };

        flash.Trigger();
        flash.Update(0.1); // at peak

        Assert.AreEqual(200, (int)target.Colour.A);

        // After full cycle, return to 40, not 0.
        flash.Update(1.0);

        Assert.AreEqual(40, (int)target.Colour.A);
    }

    [TestMethod]
    public void Trigger_DuringActiveFlash_RestartsFromBeginning()
    {
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 0) };
        Flash flash = new(target)
        {
            RiseSeconds = 0.1f,
            FallSeconds = 1.0f,
            PeakAlpha = 200,
        };

        flash.Trigger();
        flash.Update(0.05); // mid-rise

        flash.Trigger(); // restart
        flash.Update(0.0); // sample at t=0 of new flash

        // After restart, immediately on the next tick we're at the start of the rise — alpha 0.
        Assert.IsLessThan(20, (int)target.Colour.A);
    }

    [TestMethod]
    public void AutoFlash_FiresAtSpacedIntervals()
    {
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 0) };
        // Tight, deterministic interval window so this test doesn't gamble on RNG.
        Flash flash = new(target, new SystemRandom(seed: 42))
        {
            AutoFlashMinInterval = 1f,
            AutoFlashMaxInterval = 1f,
            RiseSeconds = 0.05f,
            FallSeconds = 0.05f,
            PeakAlpha = 200,
            AutoFlash = true,
        };

        // Just before the first auto-fire — should still be at rest.
        flash.Update(0.5);
        Assert.AreEqual(0, (int)target.Colour.A);

        // Past the interval — a flash should be in progress.
        flash.Update(0.55);
        Assert.IsTrue(flash.IsFlashing);
    }

    [TestMethod]
    public void AutoFlash_DisabledAfterEnable_StopsScheduling()
    {
        FakeGraphic target = new() { Colour = new Colour(255, 255, 255, 0) };
        Flash flash = new(target)
        {
            AutoFlashMinInterval = 0.1f,
            AutoFlashMaxInterval = 0.1f,
            AutoFlash = true,
        };

        flash.AutoFlash = false;
        flash.Update(5.0); // way past any auto-interval

        Assert.IsFalse(flash.IsFlashing);
    }
}

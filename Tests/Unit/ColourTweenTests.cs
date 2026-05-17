using System.Collections.Generic;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ColourTweenTests
{
    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { entity.Parent = null; }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private static ColourTween InContainer(ColourTween tween)
    {
        tween.Parent = new FakeContainer();
        return tween;
    }

    [TestMethod]
    public void InitialValue_IsFromColour()
    {
        Colour from = new(255, 0, 0);
        Colour to = new(0, 0, 255);
        var tween = new ColourTween(from, to, duration: 1.0);

        Assert.AreEqual(from, tween.Value);
    }

    [TestMethod]
    public void Update_AtCompletion_ValueIsToColour()
    {
        Colour from = new(255, 0, 0);
        Colour to = new(0, 0, 255);
        var tween = InContainer(new ColourTween(from, to, duration: 1.0));

        tween.Update(1.0);

        Assert.AreEqual(to, tween.Value);
    }

    [TestMethod]
    public void Update_Midway_ValueIsBlendedColour()
    {
        Colour from = new(0, 0, 0);
        Colour to = new(200, 100, 50);
        var tween = InContainer(new ColourTween(from, to, duration: 2.0));

        tween.Update(1.0); // halfway

        Assert.AreEqual(100, tween.Value.R);
        Assert.AreEqual(50, tween.Value.G);
        Assert.AreEqual(25, tween.Value.B);
    }

    [TestMethod]
    public void Update_WithEasing_ValueUsesEasedProgress()
    {
        // EaseInQuad at t=0.5 gives Progress=0.25; so colour should be 25% of the way.
        Colour from = new(0, 0, 0);
        Colour to = new(200, 0, 0);
        var tween = InContainer(new ColourTween(from, to, duration: 2.0, easing: Easings.EaseInQuad));

        tween.Update(1.0); // LinearProgress=0.5, Progress=0.25

        Assert.AreEqual(50, tween.Value.R); // 200 * 0.25
    }
}

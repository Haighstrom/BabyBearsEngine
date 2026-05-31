using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class LinearMotionTests
{
    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { entity.Parent = null; }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private static LinearMotion InContainer(LinearMotion motion)
    {
        motion.Parent = new FakeContainer();
        return motion;
    }

    [TestMethod]
    public void InitialValue_IsStartPoint()
    {
        Point start = new(10f, 20f);
        Point end = new(40f, 60f);
        LinearMotion motion = new(start, end, duration: 1.0);

        Assert.AreEqual(start, motion.Value);
    }

    [TestMethod]
    public void Update_Midway_ValueIsMidpoint()
    {
        LinearMotion motion = InContainer(new LinearMotion(new Point(0f, 0f), new Point(10f, 20f), duration: 2.0));

        motion.Update(1.0);

        Assert.AreEqual(5f, motion.Value.X, delta: 1e-5);
        Assert.AreEqual(10f, motion.Value.Y, delta: 1e-5);
    }

    [TestMethod]
    public void Update_AtCompletion_ValueIsEndPoint()
    {
        Point start = new(0f, 0f);
        Point end = new(10f, 20f);
        LinearMotion motion = InContainer(new LinearMotion(start, end, duration: 1.0));

        motion.Update(1.0);

        Assert.AreEqual(end, motion.Value);
    }

    [TestMethod]
    public void Update_NegativeRange_InterpolatesCorrectly()
    {
        LinearMotion motion = InContainer(new LinearMotion(new Point(10f, 20f), new Point(0f, 0f), duration: 2.0));

        motion.Update(1.0);

        Assert.AreEqual(5f, motion.Value.X, delta: 1e-5);
        Assert.AreEqual(10f, motion.Value.Y, delta: 1e-5);
    }

    [TestMethod]
    public void Update_WithEasing_ValueUsesEasedProgress()
    {
        // EaseInQuad: Progress = t² so at t=0.5, Progress = 0.25, Value.X = 0 + 100 * 0.25 = 25.
        LinearMotion motion = InContainer(
            new LinearMotion(new Point(0f, 0f), new Point(100f, 0f), duration: 2.0, easing: Easings.EaseInQuad));

        motion.Update(1.0);

        Assert.AreEqual(25f, motion.Value.X, delta: 1e-5);
        Assert.AreEqual(0f, motion.Value.Y, delta: 1e-5);
    }

    [TestMethod]
    public void Update_Loop_ValueResetsTowardsStartOnNextCycle()
    {
        LinearMotion motion = InContainer(
            new LinearMotion(new Point(0f, 0f), new Point(10f, 0f), duration: 1.0, loop: true));

        motion.Update(1.0);
        motion.Update(0.5);

        Assert.AreEqual(5f, motion.Value.X, delta: 1e-5);
    }
}

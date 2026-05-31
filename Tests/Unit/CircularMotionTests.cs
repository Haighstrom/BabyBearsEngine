using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CircularMotionTests
{
    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { entity.Parent = null; }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private static CircularMotion InContainer(CircularMotion motion)
    {
        motion.Parent = new FakeContainer();
        return motion;
    }

    [TestMethod]
    public void InitialValue_IsOnCircleAtStartAngle()
    {
        // Start at 0° on circle centred at (50, 50) with radius 10: position = (60, 50).
        CircularMotion motion = new(new Point(50f, 50f), radius: 10f, startAngleDegrees: 0.0, duration: 1.0);

        Assert.AreEqual(60f, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(50f, motion.Value.Y, delta: 1e-4);
    }

    [TestMethod]
    public void Update_QuarterRevolution_LandsAt90Degrees()
    {
        // A quarter of one revolution starting at 0° on a unit circle = (0, 1).
        CircularMotion motion = InContainer(
            new CircularMotion(new Point(0f, 0f), radius: 1f, startAngleDegrees: 0.0, duration: 4.0));

        motion.Update(1.0);

        Assert.AreEqual(0f, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(1f, motion.Value.Y, delta: 1e-4);
    }

    [TestMethod]
    public void Update_AtCompletion_ValueIsBackAtStart()
    {
        CircularMotion motion = InContainer(
            new CircularMotion(new Point(50f, 50f), radius: 10f, startAngleDegrees: 45.0, duration: 1.0));

        Point startValue = motion.Value;

        motion.Update(1.0);

        Assert.AreEqual(startValue.X, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(startValue.Y, motion.Value.Y, delta: 1e-4);
    }
}

using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Tweens;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class EllipticalMotionTests
{
    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { entity.Parent = null; }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    private static EllipticalMotion InContainer(EllipticalMotion motion)
    {
        motion.Parent = new FakeContainer();
        return motion;
    }

    [TestMethod]
    public void InitialValue_IsOnEllipseAtStartAngle()
    {
        // Start at 0° on an ellipse centred at (100, 50) with radii (20, 10):
        // X = 100 + cos(0) * 20 = 120, Y = 50 + sin(0) * 10 = 50.
        EllipticalMotion motion = new(new Point(100f, 50f), radiusX: 20f, radiusY: 10f, startAngleDegrees: 0.0, duration: 1.0);

        Assert.AreEqual(120f, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(50f, motion.Value.Y, delta: 1e-4);
    }

    [TestMethod]
    public void InitialAngleDegrees_IsStartAngle()
    {
        EllipticalMotion motion = new(new Point(0f, 0f), radiusX: 1f, radiusY: 1f, startAngleDegrees: 45.0, duration: 1.0);

        Assert.AreEqual(45.0, motion.AngleDegrees, delta: 1e-10);
    }

    [TestMethod]
    public void Update_Midway_ValueIsOppositeSideOfEllipse()
    {
        // Halfway through one revolution = 180° from start.
        // Starting at 0° with radii (20, 10), 180° lands at (-20, 0) relative to centre.
        EllipticalMotion motion = InContainer(
            new EllipticalMotion(new Point(100f, 50f), radiusX: 20f, radiusY: 10f, startAngleDegrees: 0.0, duration: 2.0));

        motion.Update(1.0);

        Assert.AreEqual(80f, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(50f, motion.Value.Y, delta: 1e-4);
    }

    [TestMethod]
    public void Update_Midway_AngleDegreesIs180BeyondStart()
    {
        EllipticalMotion motion = InContainer(
            new EllipticalMotion(new Point(0f, 0f), radiusX: 1f, radiusY: 1f, startAngleDegrees: 30.0, duration: 2.0));

        motion.Update(1.0);

        Assert.AreEqual(210.0, motion.AngleDegrees, delta: 1e-10);
    }

    [TestMethod]
    public void Update_AtCompletion_ValueIsBackAtStart()
    {
        // After one full revolution the position returns to where it started.
        EllipticalMotion motion = InContainer(
            new EllipticalMotion(new Point(100f, 50f), radiusX: 20f, radiusY: 10f, startAngleDegrees: 30.0, duration: 1.0));

        Point startValue = motion.Value;

        motion.Update(1.0);

        Assert.AreEqual(startValue.X, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(startValue.Y, motion.Value.Y, delta: 1e-4);
    }

    [TestMethod]
    public void Update_DifferentRadii_TraversesEllipseNotCircle()
    {
        // At 90° on an ellipse with radii (20, 10), position is (0, 10) relative to centre.
        EllipticalMotion motion = InContainer(
            new EllipticalMotion(new Point(0f, 0f), radiusX: 20f, radiusY: 10f, startAngleDegrees: 0.0, duration: 4.0));

        motion.Update(1.0); // quarter of one revolution = 90°

        Assert.AreEqual(0f, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(10f, motion.Value.Y, delta: 1e-4);
    }

    [TestMethod]
    public void Update_WithEasing_AdvancesByEasedProgress()
    {
        // EaseInQuad at t=0.5 gives Progress=0.25, so angle advances 0.25 * 360° = 90°.
        // Starting at 0° on a unit circle, 90° lands at (0, 1).
        EllipticalMotion motion = InContainer(
            new EllipticalMotion(new Point(0f, 0f), radiusX: 1f, radiusY: 1f, startAngleDegrees: 0.0, duration: 2.0, easing: Easings.EaseInQuad));

        motion.Update(1.0); // LinearProgress = 0.5, Progress = 0.25

        Assert.AreEqual(0f, motion.Value.X, delta: 1e-4);
        Assert.AreEqual(1f, motion.Value.Y, delta: 1e-4);
    }
}

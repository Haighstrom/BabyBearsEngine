using System;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MoveFadeRemoveControllerTests
{
    private sealed class FakeMoveFadeable : IMoveFadeable
    {
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public Colour Colour { get; set; } = Colour.White;
        public IContainer? Parent { get; set; } = new FakeContainer();
        public bool Exists => Parent is not null;
        public void Remove() => Parent = null;
    }

    private sealed class FakeContainer : IContainer
    {
        public void Add(IAddable entity) { }
        public void Remove(IAddable entity) { }
        public void RemoveAll() { }
        public (float x, float y) GetWindowCoordinates(float x, float y) => (x, y);
    }

    [TestMethod]
    public void Update_MovesTargetByVelocityTimesElapsed()
    {
        FakeMoveFadeable target = new() { X = 0f, Y = 0f };
        MoveFadeRemoveController controller = new(target, velocityX: 100f, velocityY: -50f, duration: 2.0);

        controller.Update(0.1);

        Assert.AreEqual(10f, target.X, delta: 0.001f);
        Assert.AreEqual(-5f, target.Y, delta: 0.001f);
    }

    [TestMethod]
    public void Update_FadesAlphaTowardZero()
    {
        FakeMoveFadeable target = new() { Colour = Colour.White }; // A = 255
        MoveFadeRemoveController controller = new(target, 0f, 0f, duration: 1.0);

        controller.Update(0.5); // halfway

        Assert.AreEqual(127, (int)target.Colour.A);
    }

    [TestMethod]
    public void Update_PreservesRgbWhileFading()
    {
        FakeMoveFadeable target = new() { Colour = new Colour(100, 150, 200, 255) };
        MoveFadeRemoveController controller = new(target, 0f, 0f, duration: 1.0);

        controller.Update(0.5);

        Assert.AreEqual(100, (int)target.Colour.R);
        Assert.AreEqual(150, (int)target.Colour.G);
        Assert.AreEqual(200, (int)target.Colour.B);
    }

    [TestMethod]
    public void Update_WhenDurationElapsed_RemovesTarget()
    {
        FakeMoveFadeable target = new();
        MoveFadeRemoveController controller = new(target, 0f, 0f, duration: 1.0);

        controller.Update(1.0);

        Assert.IsNull(target.Parent);
    }

    [TestMethod]
    public void Update_WhenDurationElapsed_RaisesCompleted()
    {
        FakeMoveFadeable target = new();
        bool raised = false;
        MoveFadeRemoveController controller = new(target, 0f, 0f, duration: 1.0);
        controller.Completed += (_, _) => raised = true;

        controller.Update(1.0);

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void Update_BeforeDurationElapsed_DoesNotRemoveTarget()
    {
        FakeMoveFadeable target = new();
        MoveFadeRemoveController controller = new(target, 0f, 0f, duration: 1.0);

        controller.Update(0.5);

        Assert.IsNotNull(target.Parent);
    }

    [TestMethod]
    public void Update_AfterCompletion_DoesNotMoveTargetFurther()
    {
        FakeMoveFadeable target = new() { X = 0f };
        MoveFadeRemoveController controller = new(target, velocityX: 100f, velocityY: 0f, duration: 1.0);

        controller.Update(1.0); // completes
        controller.Update(1.0); // should be a no-op

        Assert.AreEqual(100f, target.X, delta: 0.001f);
    }
}

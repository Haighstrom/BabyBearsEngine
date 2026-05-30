using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class DockingControllerTests
{
    private sealed class FakeRect : IRect
    {
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Width { get; set; } = 0f;
        public float Height { get; set; } = 0f;
    }

    private FakeRect _target = null!;
    private FakeRect _host = null!;

    [TestInitialize]
    public void Setup()
    {
        _target = new() { Width = 20f, Height = 10f };
        _host = new() { X = 100f, Y = 200f, Width = 100f, Height = 50f };
    }

    private DockingController MakeController(DockPosition position, Point shift = default) =>
        new(_target, () => _host, position, shift);

    // Snap on construct

    [TestMethod]
    public void Constructor_SnapsTargetToHostImmediately()
    {
        MakeController(DockPosition.TopLeft);

        Assert.AreEqual(100f, _target.X);
        Assert.AreEqual(200f, _target.Y);
    }

    // Anchors — exhaustive table over the 11 enum values

    [TestMethod]
    [DataRow(DockPosition.TopLeft,      100f, 200f)]
    [DataRow(DockPosition.TopCentre,    140f, 200f)]
    [DataRow(DockPosition.TopRight,     180f, 200f)]
    [DataRow(DockPosition.CentreLeft,   100f, 220f)]
    [DataRow(DockPosition.Centre,       140f, 220f)]
    [DataRow(DockPosition.CentreRight,  180f, 220f)]
    [DataRow(DockPosition.BottomLeft,   100f, 240f)]
    [DataRow(DockPosition.BottomCentre, 140f, 240f)]
    [DataRow(DockPosition.BottomRight,  180f, 240f)]
    [DataRow(DockPosition.HangBelow,    140f, 250f)]
    [DataRow(DockPosition.HangAbove,    140f, 190f)]
    public void Anchor_PlacesTargetCorrectly(DockPosition position, float expectedX, float expectedY)
    {
        // Host at (100,200) size 100x50 → centre (150, 225), right 200, bottom 250.
        // Target size 20x10 → halfW=10, halfH=5.
        DockingController controller = MakeController(position);

        controller.Update(0.0);

        Assert.AreEqual(expectedX, _target.X, 0.001f);
        Assert.AreEqual(expectedY, _target.Y, 0.001f);
    }

    // Shift

    [TestMethod]
    public void Shift_OffsetsTargetAfterAnchoring()
    {
        DockingController controller = MakeController(DockPosition.TopLeft, new Point(5f, -3f));

        controller.Update(0.0);

        Assert.AreEqual(105f, _target.X);
        Assert.AreEqual(197f, _target.Y);
    }

    [TestMethod]
    public void Shift_AssignedAfterConstruction_TakesEffectOnNextUpdate()
    {
        DockingController controller = MakeController(DockPosition.Centre);
        controller.Shift = new Point(10f, 10f);

        controller.Update(0.0);

        Assert.AreEqual(150f, _target.X);
        Assert.AreEqual(230f, _target.Y);
    }

    // Host follows when it moves

    [TestMethod]
    public void Update_WhenHostMoves_TargetFollows()
    {
        DockingController controller = MakeController(DockPosition.TopLeft);
        _host.X = 500f;
        _host.Y = 600f;

        controller.Update(0.0);

        Assert.AreEqual(500f, _target.X);
        Assert.AreEqual(600f, _target.Y);
    }

    // Host resolver is re-evaluated each frame

    [TestMethod]
    public void Update_ResolvesHostEachFrame()
    {
        FakeRect first = new() { X = 0f, Y = 0f, Width = 10f, Height = 10f };
        FakeRect second = new() { X = 500f, Y = 500f, Width = 10f, Height = 10f };
        IRect current = first;

        DockingController controller = new(_target, () => current, DockPosition.TopLeft);
        current = second;
        controller.Update(0.0);

        Assert.AreEqual(500f, _target.X);
        Assert.AreEqual(500f, _target.Y);
    }

    // DockPosition hot-swap

    [TestMethod]
    public void DockPosition_AssignedAfterConstruction_TakesEffectOnNextUpdate()
    {
        DockingController controller = MakeController(DockPosition.TopLeft);

        controller.DockPosition = DockPosition.BottomRight;
        controller.Update(0.0);

        Assert.AreEqual(180f, _target.X);
        Assert.AreEqual(240f, _target.Y);
    }
}

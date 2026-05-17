using System;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class YIndexedLayerControllerTests
{
    private sealed class FakeLayered : ILayered
    {
        private int _layer = 0;

        public int Layer
        {
            get => _layer;
            set
            {
                int old = _layer;
                _layer = value;
                if (old != value)
                {
                    LayerChanged?.Invoke(this, new LayerChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<LayerChangedEventArgs>? LayerChanged;
    }

    [TestMethod]
    public void Update_SetsLayerFromYPosition()
    {
        float y = 50f;
        FakeLayered target = new();
        YIndexedLayerController controller = new(() => y, target);

        controller.Update(0.016);

        Assert.AreEqual(50, target.Layer);
    }

    [TestMethod]
    public void Update_HigherY_ProducesLowerLayer()
    {
        FakeLayered shallowTarget = new();
        FakeLayered deepTarget = new();
        YIndexedLayerController shallowController = new(() => 30f, shallowTarget);
        YIndexedLayerController deepController = new(() => 60f, deepTarget);

        shallowController.Update(0.016);
        deepController.Update(0.016);

        Assert.IsGreaterThan(deepTarget.Layer, shallowTarget.Layer);
    }

    [TestMethod]
    public void Update_LayerClampsToZero_WhenYExceedsOffset()
    {
        float y = 150f;
        FakeLayered target = new();
        YIndexedLayerController controller = new(() => y, target);

        controller.Update(0.016);

        Assert.AreEqual(0, target.Layer);
    }

    [TestMethod]
    public void Update_RespectsCustomLayerOffset()
    {
        float y = 100f;
        FakeLayered target = new();
        YIndexedLayerController controller = new(() => y, target, layerOffset: 500);

        controller.Update(0.016);

        Assert.AreEqual(400, target.Layer);
    }

    [TestMethod]
    public void Update_TracksChangingY()
    {
        float y = 30f;
        FakeLayered target = new();
        YIndexedLayerController controller = new(() => y, target);

        controller.Update(0.016);
        y = 60f;
        controller.Update(0.016);

        Assert.AreEqual(40, target.Layer);
    }
}

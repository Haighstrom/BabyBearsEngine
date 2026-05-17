using BabyBearsEngine.Worlds.UI;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ScrollingListPanelTests
{
    // CalculateThumbProportion

    [TestMethod]
    public void CalculateThumbProportion_ContentEqualsPanel_ReturnsOne()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 300f);

        Assert.AreEqual(1f, result);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentTwicePanel_ReturnsHalf()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 600f);

        Assert.AreEqual(0.5f, result);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentThreeTimes_ReturnsThird()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(100f, 300f);

        Assert.AreEqual(1f / 3f, result, delta: 0.0001f);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentSmallerThanPanel_ReturnsOne()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 100f);

        Assert.AreEqual(1f, result);
    }

    [TestMethod]
    public void CalculateThumbProportion_ContentZero_ReturnsOne()
    {
        float result = ScrollingListPanel.CalculateThumbProportion(300f, 0f);

        Assert.AreEqual(1f, result);
    }

    // CalculateScrollOffset

    [TestMethod]
    public void CalculateScrollOffset_AtZero_ReturnsZero()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(0f, 300f, 600f);

        Assert.AreEqual(0f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_AtOne_ReturnsMaxOffset()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(1f, 300f, 600f);

        Assert.AreEqual(300f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_AtHalf_ReturnsHalfMaxOffset()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(0.5f, 300f, 600f);

        Assert.AreEqual(150f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_ContentSmallerThanPanel_ReturnsZero()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(1f, 300f, 100f);

        Assert.AreEqual(0f, result);
    }

    [TestMethod]
    public void CalculateScrollOffset_ContentEqualsPanel_ReturnsZero()
    {
        float result = ScrollingListPanel.CalculateScrollOffset(1f, 300f, 300f);

        Assert.AreEqual(0f, result);
    }
}

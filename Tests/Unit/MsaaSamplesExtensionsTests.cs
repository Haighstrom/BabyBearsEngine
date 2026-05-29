using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MsaaSamplesExtensionsTests
{
    [TestMethod]
    public void ClampToMax_RequestedBelowMax_ReturnsRequested()
    {
        Assert.AreEqual(MsaaSamples.X4, MsaaSamples.X4.ClampToMax(maxSamples: 8));
    }

    [TestMethod]
    public void ClampToMax_RequestedEqualsMax_ReturnsRequested()
    {
        Assert.AreEqual(MsaaSamples.X8, MsaaSamples.X8.ClampToMax(maxSamples: 8));
    }

    [TestMethod]
    public void ClampToMax_RequestedAboveMax_ReturnsHighestEnumValueAtOrBelowMax()
    {
        Assert.AreEqual(MsaaSamples.X4, MsaaSamples.X16.ClampToMax(maxSamples: 4));
    }

    [TestMethod]
    public void ClampToMax_RequestedAboveMaxAndMaxBetweenEnumValues_ReturnsHighestEnumValueAtOrBelowMax()
    {
        Assert.AreEqual(MsaaSamples.X2, MsaaSamples.X8.ClampToMax(maxSamples: 3));
    }

    [TestMethod]
    public void ClampToMax_MaxBelowMinimumEnabledValue_ReturnsDisabled()
    {
        Assert.AreEqual(MsaaSamples.Disabled, MsaaSamples.X8.ClampToMax(maxSamples: 1));
    }

    [TestMethod]
    public void ClampToMax_MaxZero_ReturnsDisabled()
    {
        Assert.AreEqual(MsaaSamples.Disabled, MsaaSamples.X8.ClampToMax(maxSamples: 0));
    }

    [TestMethod]
    public void ClampToMax_RequestedDisabled_ReturnsDisabled()
    {
        Assert.AreEqual(MsaaSamples.Disabled, MsaaSamples.Disabled.ClampToMax(maxSamples: 8));
    }
}

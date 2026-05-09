using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class LayerChangedEventArgsTests
{
    [TestMethod]
    public void Constructor_StoresOldAndNewLayer()
    {
        var args = new LayerChangedEventArgs(oldLayer: 3, newLayer: 7);
        Assert.AreEqual(3, args.OldLayer);
        Assert.AreEqual(7, args.NewLayer);
    }
}

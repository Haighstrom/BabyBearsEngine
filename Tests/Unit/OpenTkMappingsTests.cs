using BabyBearsEngine.Platform.OpenTK;
using OpenTKWindowBorder = OpenTK.Windowing.Common.WindowBorder;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class OpenTkMappingsTests
{
    // WindowBorder

    [TestMethod]
    public void WindowBorder_AllKnownValues_RoundTripThroughOpenTK()
    {
        foreach (WindowBorder border in Enum.GetValues<WindowBorder>())
        {
            OpenTKWindowBorder mapped = border.ToOpenTK();
            WindowBorder back = mapped.ToWindowBorder();
            Assert.AreEqual(border, back);
        }
    }

    [TestMethod]
    public void WindowBorder_UnknownBbeValue_Throws()
    {
        // 99 is not a defined WindowBorder. A silent (T) cast would hand OpenTK garbage; an
        // exhaustive switch should throw so the regression is caught at the boundary.
        WindowBorder unknown = (WindowBorder)99;

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => unknown.ToOpenTK());
    }

    [TestMethod]
    public void WindowBorder_UnknownOpenTkValue_Throws()
    {
        OpenTKWindowBorder unknown = (OpenTKWindowBorder)99;

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => unknown.ToWindowBorder());
    }
}

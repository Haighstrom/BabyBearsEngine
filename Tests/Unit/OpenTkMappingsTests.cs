using BabyBearsEngine.Platform.OpenTK;
using OpenTKImage = OpenTK.Windowing.Common.Input.Image;
using OpenTKWindowBorder = OpenTK.Windowing.Common.WindowBorder;
using OpenTKWindowIcon = OpenTK.Windowing.Common.Input.WindowIcon;

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

    // WindowIcon

    [TestMethod]
    public void WindowIcon_ToOpenTK_PreservesAllImageVariants()
    {
        byte[] large = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
        byte[] small = [20, 21, 22, 23];
        WindowIcon icon = new(new WindowIconImage(2, 2, large), new WindowIconImage(1, 1, small));

        OpenTKWindowIcon mapped = icon.ToOpenTK();

        Assert.HasCount(2, mapped.Images);
        Assert.AreEqual(2, mapped.Images[0].Width);
        Assert.AreEqual(1, mapped.Images[1].Width);
    }

    [TestMethod]
    public void WindowIcon_RoundTrip_PreservesAllImageVariants()
    {
        byte[] large = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
        byte[] small = [20, 21, 22, 23];
        WindowIcon icon = new(new WindowIconImage(2, 2, large), new WindowIconImage(1, 1, small));

        WindowIcon back = icon.ToOpenTK().ToWindowIcon();

        Assert.HasCount(2, back.Images);
        Assert.AreEqual(2, back.Images[0].Width);
        Assert.AreEqual(2, back.Images[0].Height);
        CollectionAssert.AreEqual(large, back.Images[0].Pixels);
        Assert.AreEqual(1, back.Images[1].Width);
        Assert.AreEqual(1, back.Images[1].Height);
        CollectionAssert.AreEqual(small, back.Images[1].Pixels);
    }

    [TestMethod]
    public void WindowIcon_Empty_RoundTrip_StaysEmpty()
    {
        WindowIcon back = new WindowIcon().ToOpenTK().ToWindowIcon();

        Assert.IsTrue(back.IsEmpty);
        Assert.IsEmpty(back.Images);
    }

    [TestMethod]
    public void WindowIcon_EmptyOpenTkIcon_MapsToEmpty()
    {
        WindowIcon mapped = new OpenTKWindowIcon().ToWindowIcon();

        Assert.IsTrue(mapped.IsEmpty);
    }
}

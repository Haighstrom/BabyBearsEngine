using System;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class OpenTKEnumMirrorTests
{
    [TestMethod]
    public void Keys_MirrorsOpenTKExactly()
    {
        AssertEnumsMirror<Input.Keys, OpenTK.Windowing.GraphicsLibraryFramework.Keys>();
    }

    [TestMethod]
    public void MouseButton_MirrorsOpenTKExactly()
    {
        AssertEnumsMirror<Input.MouseButton, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton>();
    }

    [TestMethod]
    public void WindowBorder_MirrorsOpenTKExactly()
    {
        AssertEnumsMirror<WindowBorder, OpenTK.Windowing.Common.WindowBorder>();
    }

    [TestMethod]
    public void WindowState_MirrorsOpenTKExactly()
    {
        AssertEnumsMirror<WindowState, OpenTK.Windowing.Common.WindowState>();
    }

    private static void AssertEnumsMirror<TBBE, TOpenTK>()
        where TBBE : struct, Enum
        where TOpenTK : struct, Enum
    {
        var bbeNames = Enum.GetNames<TBBE>();
        var openTKNames = Enum.GetNames<TOpenTK>();

        CollectionAssert.AreEquivalent(
            openTKNames,
            bbeNames,
            $"Name sets differ between {typeof(TBBE).FullName} and {typeof(TOpenTK).FullName}.");

        foreach (var name in bbeNames)
        {
            var bbeValue = Convert.ToInt32(Enum.Parse<TBBE>(name));
            var openTKValue = Convert.ToInt32(Enum.Parse<TOpenTK>(name));
            Assert.AreEqual(openTKValue, bbeValue, $"Value mismatch for '{name}' in {typeof(TBBE).Name}.");
        }
    }
}

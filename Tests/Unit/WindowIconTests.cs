namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class WindowIconTests
{
    [TestMethod]
    public void Default_IsEmpty()
    {
        WindowIcon icon = new();

        Assert.IsTrue(icon.IsEmpty);
        Assert.IsEmpty(icon.Images);
    }

    [TestMethod]
    public void SingleImageConstructor_CreatesOneImageWithGivenData()
    {
        byte[] pixels = [1, 2, 3, 4];
        WindowIcon icon = new(1, 1, pixels);

        Assert.IsFalse(icon.IsEmpty);
        Assert.HasCount(1, icon.Images);
        Assert.AreEqual(1, icon.Images[0].Width);
        Assert.AreEqual(1, icon.Images[0].Height);
        Assert.AreSame(pixels, icon.Images[0].Pixels);
    }

    [TestMethod]
    public void MultiImageConstructor_PreservesAllVariantsInOrder()
    {
        WindowIconImage large = new(2, 2, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
        WindowIconImage small = new(1, 1, [20, 21, 22, 23]);

        WindowIcon icon = new(large, small);

        Assert.HasCount(2, icon.Images);
        Assert.AreSame(large, icon.Images[0]);
        Assert.AreSame(small, icon.Images[1]);
    }

    [TestMethod]
    public void MultiImageConstructor_WithNoImages_IsEmpty()
    {
        WindowIcon icon = new(images: []);

        Assert.IsTrue(icon.IsEmpty);
    }

    [TestMethod]
    public void WindowIconImage_StoresWidthHeightAndPixels()
    {
        byte[] pixels = [10, 20, 30, 40];
        WindowIconImage image = new(3, 4, pixels);

        Assert.AreEqual(3, image.Width);
        Assert.AreEqual(4, image.Height);
        Assert.AreSame(pixels, image.Pixels);
    }
}

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CollectionExtensionsTests
{
    // ─── Rotate ───

    [TestMethod]
    public void Rotate_ByOne_MovesFirstElementToEnd()
    {
        List<int> list = [1, 2, 3, 4, 5];

        list.Rotate(1);

        CollectionAssert.AreEqual(new List<int> { 2, 3, 4, 5, 1 }, list);
    }

    [TestMethod]
    public void Rotate_ByTwo_MovesFirstTwoElementsToEnd()
    {
        List<int> list = [1, 2, 3, 4, 5];

        list.Rotate(2);

        CollectionAssert.AreEqual(new List<int> { 3, 4, 5, 1, 2 }, list);
    }

    [TestMethod]
    public void Rotate_ByCount_ReturnsSameOrder()
    {
        List<int> list = [1, 2, 3, 4, 5];
        List<int> original = [1, 2, 3, 4, 5];

        list.Rotate(5);

        CollectionAssert.AreEqual(original, list);
    }

    [TestMethod]
    public void Rotate_ByMoreThanCount_WrapsAround()
    {
        List<int> list = [1, 2, 3];

        list.Rotate(4);

        CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, list);
    }

    [TestMethod]
    public void Rotate_SingleElement_Unchanged()
    {
        List<int> list = [42];

        list.Rotate(1);

        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void Rotate_ZeroAmount_Throws()
    {
        List<int> list = [1, 2, 3];

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.Rotate(0));
    }

    [TestMethod]
    public void Rotate_NegativeAmount_Throws()
    {
        List<int> list = [1, 2, 3];

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.Rotate(-1));
    }
}

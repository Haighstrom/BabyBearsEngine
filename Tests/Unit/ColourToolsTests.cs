using System.Collections.Generic;
using System.Reflection;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class ColourToolsTests
{
    private static IReadOnlyList<Colour> NamedColours()
    {
        List<Colour> colours = [];
        foreach (PropertyInfo p in typeof(Colour).GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            if (p.GetValue(null) is Colour c)
            {
                colours.Add(c);
            }
        }
        return colours;
    }

    [TestMethod]
    public void RandSystemColour_ReturnsOneOfTheNamedColours()
    {
        IReadOnlyList<Colour> named = NamedColours();

        for (int i = 0; i < 50; i++)
        {
            Colour result = ColourTools.RandSystemColour();
            Assert.Contains(result, named);
        }
    }

    [TestMethod]
    public void RandSystemColour_NamedColoursAreNonEmpty()
    {
        // Sanity check: Colour must expose at least some named static colours.
        IReadOnlyList<Colour> named = NamedColours();
        Assert.IsGreaterThan(0, named.Count);
    }
}

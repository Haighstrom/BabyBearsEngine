using System.Reflection;

namespace BabyBearsEngine;

/// <summary>Utilities for selecting random named <see cref="Colour"/> values.</summary>
public static class ColourTools
{
    /// <summary>
    /// Returns a randomly selected colour from the named static <see cref="Colour"/> properties
    /// (e.g. <see cref="Colour.Red"/>, <see cref="Colour.Blue"/>).
    /// </summary>
    public static Colour RandSystemColour()
    {
        List<Colour> colours = [];

        foreach (var propertyInfo in typeof(Colour)
            .GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            var v = propertyInfo.GetValue(null);

            if (v is Colour colour)
            {
                colours.Add(colour);
            }
        }

        Random random = new();

        return colours[random.Next(colours.Count)];
    }
}

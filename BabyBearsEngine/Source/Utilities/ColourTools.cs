using System.Collections.Generic;
using System.Reflection;

namespace BabyBearsEngine;

public static class ColourTools
{
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

using System.Collections.Generic;
using System.Reflection;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Source.Tools;

public static class ColourTools
{
    public static Color4 RandSystemColour()
    {
        List<Color4> colours = [];

        foreach (var propertyInfo in typeof(Color4).GetProperties(
            BindingFlags.Static | BindingFlags.Public))
        {
            var v = propertyInfo.GetValue(null);

            if (v is Color4 colour)
            {
                colours.Add(colour);
            }
        }

        Random random = new();

        return colours[random.Next(colours.Count)];
    }
}

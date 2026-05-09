using System.Runtime.InteropServices;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine;

/// <summary>An RGBA colour with byte components (0–255). Premade named colours are available as static properties.</summary>
/// <param name="R">Red component, 0–255.</param>
/// <param name="G">Green component, 0–255.</param>
/// <param name="B">Blue component, 0–255.</param>
/// <param name="A">Alpha component, 0–255. Defaults to 255 (fully opaque).</param>
[StructLayout(LayoutKind.Sequential)]
public partial record struct Colour(byte R, byte G, byte B, byte A = 255) : IEquatable<Colour>
{
    private static byte FloatToByte(float value, string componentName)
    {
#if DEBUG
        if (value < 0f || value > 1f)
        {
            Logger.Log($"Colour component {componentName} was clamped from {value}");
        }
#endif
        return (byte)Math.Clamp((int)Math.Round(value * 255f), 0, 255);
    }

    private static void HslToRgb(float h, float s, float l, out float r, out float g, out float b)
    {
        if (s == 0f)
        {
            r = g = b = l;
            return;
        }

        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;
        r = HueToRgb(p, q, h + 1f / 3f);
        g = HueToRgb(p, q, h);
        b = HueToRgb(p, q, h - 1f / 3f);
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    private static void RgbToHsl(float r, float g, float b, out float h, out float s, out float l)
    {
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        l = (max + min) / 2f;

        if (max == min)
        {
            h = s = 0f;
            return;
        }

        float d = max - min;
        s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

        if (max == r)
        {
            h = (g - b) / d + (g < b ? 6f : 0f);
        }
        else if (max == g)
        {
            h = (b - r) / d + 2f;
        }
        else
        {
            h = (r - g) / d + 4f;
        }

        h /= 6f;
    }

    /// <summary>Creates a colour from normalised float components.</summary>
    /// <param name="r">Red component, 0.0–1.0.</param>
    /// <param name="g">Green component, 0.0–1.0.</param>
    /// <param name="b">Blue component, 0.0–1.0.</param>
    /// <param name="a">Alpha component, 0.0–1.0. Defaults to 1.0 (fully opaque).</param>
    public Colour(float r, float g, float b, float a = 1f)
        : this(FloatToByte(r, nameof(R)),
              FloatToByte(g, nameof(G)),
              FloatToByte(b, nameof(B)),
              FloatToByte(a, nameof(A)))
    {
    }

    /// <summary>Creates a colour by copying RGB from an existing colour and replacing the alpha.</summary>
    /// <param name="colour">Source colour to copy RGB from.</param>
    /// <param name="alpha">New alpha component, 0–255.</param>
    public Colour(Colour colour, byte alpha)
        : this(colour.R, colour.G, colour.B, alpha)
    {
    }

    /// <summary>Creates a colour by copying RGB from an existing colour and replacing the alpha.</summary>
    /// <param name="colour">Source colour to copy RGB from.</param>
    /// <param name="alpha">New alpha component, 0.0–1.0.</param>
    public Colour(Colour colour, float alpha)
        : this(colour.R, colour.G, colour.B, FloatToByte(alpha, nameof(A)))
    {
    }

    /// <summary>Gets the red component as a normalised value in [0, 1].</summary>
    public readonly float NormalisedR => R / 255f;
    /// <summary>Gets the green component as a normalised value in [0, 1].</summary>
    public readonly float NormalisedG => G / 255f;
    /// <summary>Gets the blue component as a normalised value in [0, 1].</summary>
    public readonly float NormalisedB => B / 255f;
    /// <summary>Gets the alpha component as a normalised value in [0, 1].</summary>
    public readonly float NormalisedA => A / 255f;

    /// <summary>
    /// Returns a darker version of this colour by reducing its HSL lightness.
    /// Alpha is preserved.
    /// </summary>
    /// <param name="amount">How much to darken, in the range 0.0–1.0. 0.0 leaves the colour unchanged; 1.0 produces black.</param>
    public readonly Colour Darkened(float amount)
    {
        RgbToHsl(NormalisedR, NormalisedG, NormalisedB, out float h, out float s, out float l);
        l = Math.Clamp(l - amount, 0f, 1f);
        HslToRgb(h, s, l, out float r, out float g, out float b);
        return new Colour(r, g, b, NormalisedA);
    }

    /// <summary>
    /// Returns a lighter version of this colour by increasing its HSL lightness.
    /// Alpha is preserved.
    /// </summary>
    /// <param name="amount">How much to lighten, in the range 0.0–1.0. 0.0 leaves the colour unchanged; 1.0 produces white.</param>
    public readonly Colour Lightened(float amount)
    {
        RgbToHsl(NormalisedR, NormalisedG, NormalisedB, out float h, out float s, out float l);
        l = Math.Clamp(l + amount, 0f, 1f);
        HslToRgb(h, s, l, out float r, out float g, out float b);
        return new Colour(r, g, b, NormalisedA);
    }

    /// <summary>Returns this colour packed as a 32-bit ARGB integer.</summary>
    public readonly int ToArgb() => (A << 24) | (R << 16) | (G << 8) | B;

    /// <summary>Returns this colour as a <see cref="System.Drawing.Color"/>.</summary>
    public readonly System.Drawing.Color ToColor => System.Drawing.Color.FromArgb(A, R, G, B);

    /// <summary>Returns this colour as an OpenTK <see cref="OpenTK.Mathematics.Color4"/>. Internal bridge for the platform layer.</summary>
    internal readonly OpenTK.Mathematics.Color4 ToOpenTK() => new(NormalisedR, NormalisedG, NormalisedB, NormalisedA);

    /// <summary>Returns this colour with the alpha replaced.</summary>
    /// <param name="alpha">New alpha component, 0–255.</param>
    public readonly Colour WithAlpha(byte alpha) => new(R, G, B, alpha);
}

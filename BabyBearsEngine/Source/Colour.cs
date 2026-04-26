using System.Runtime.InteropServices;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine;

/// <summary>An RGBA colour with byte components (0–255). Premade named colours are available as static properties.</summary>
/// <param name="R">Red component, 0–255.</param>
/// <param name="G">Green component, 0–255.</param>
/// <param name="B">Blue component, 0–255.</param>
/// <param name="A">Alpha component, 0–255. Defaults to 255 (fully opaque).</param>
[StructLayout(LayoutKind.Sequential)]
public record struct Colour(byte R, byte G, byte B, byte A = 255) : IEquatable<Colour>
{
    #region Premade Colours
    /// <summary>Gets the system colour with (R, G, B, A) = (255, 255, 255, 0).</summary>
    public static Colour Transparent => new(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);

    /// <summary>Gets the system colour with (R, G, B, A) = (240, 248, 255, 255).</summary>
    public static Colour AliceBlue => new(240, 248, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (250, 235, 215, 255).</summary>
    public static Colour AntiqueWhite => new(250, 235, 215, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 255, 255, 255).</summary>
    public static Colour Aqua => new(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (127, 255, 212, 255).</summary>
    public static Colour Aquamarine => new(127, byte.MaxValue, 212, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (240, 255, 255, 255).</summary>
    public static Colour Azure => new(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (245, 245, 220, 255).</summary>
    public static Colour Beige => new(245, 245, 220, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 228, 196, 255).</summary>
    public static Colour Bisque => new(byte.MaxValue, 228, 196, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 0, 0, 255).</summary>
    public static Colour Black => new(0, 0, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 235, 205, 255).</summary>
    public static Colour BlanchedAlmond => new(byte.MaxValue, 235, 205, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 0, 255, 255).</summary>
    public static Colour Blue => new(0, 0, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (138, 43, 226, 255).</summary>
    public static Colour BlueViolet => new(138, 43, 226, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (165, 42, 42, 255).</summary>
    public static Colour Brown => new(165, 42, 42, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (222, 184, 135, 255).</summary>
    public static Colour BurlyWood => new(222, 184, 135, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (95, 158, 160, 255).</summary>
    public static Colour CadetBlue => new(95, 158, 160, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (127, 255, 0, 255).</summary>
    public static Colour Chartreuse => new(127, byte.MaxValue, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (210, 105, 30, 255).</summary>
    public static Colour Chocolate => new(210, 105, 30, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 127, 80, 255).</summary>
    public static Colour Coral => new(byte.MaxValue, 127, 80, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (100, 149, 237, 255).</summary>
    public static Colour CornflowerBlue => new(100, 149, 237, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 248, 220, 255).</summary>
    public static Colour Cornsilk => new(byte.MaxValue, 248, 220, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (220, 20, 60, 255).</summary>
    public static Colour Crimson => new(220, 20, 60, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 255, 255, 255).</summary>
    public static Colour Cyan => new(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 0, 139, 255).</summary>
    public static Colour DarkBlue => new(0, 0, 139, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 139, 139, 255).</summary>
    public static Colour DarkCyan => new(0, 139, 139, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (184, 134, 11, 255).</summary>
    public static Colour DarkGoldenrod => new(184, 134, 11, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (169, 169, 169, 255).</summary>
    public static Colour DarkGray => new(169, 169, 169, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 100, 0, 255).</summary>
    public static Colour DarkGreen => new(0, 100, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (189, 183, 107, 255).</summary>
    public static Colour DarkKhaki => new(189, 183, 107, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (139, 0, 139, 255).</summary>
    public static Colour DarkMagenta => new(139, 0, 139, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (85, 107, 47, 255).</summary>
    public static Colour DarkOliveGreen => new(85, 107, 47, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 140, 0, 255).</summary>
    public static Colour DarkOrange => new(byte.MaxValue, 140, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (153, 50, 204, 255).</summary>
    public static Colour DarkOrchid => new(153, 50, 204, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (139, 0, 0, 255).</summary>
    public static Colour DarkRed => new(139, 0, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (233, 150, 122, 255).</summary>
    public static Colour DarkSalmon => new(233, 150, 122, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (143, 188, 139, 255).</summary>
    public static Colour DarkSeaGreen => new(143, 188, 139, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (72, 61, 139, 255).</summary>
    public static Colour DarkSlateBlue => new(72, 61, 139, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (47, 79, 79, 255).</summary>
    public static Colour DarkSlateGray => new(47, 79, 79, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 206, 209, 255).</summary>
    public static Colour DarkTurquoise => new(0, 206, 209, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (148, 0, 211, 255).</summary>
    public static Colour DarkViolet => new(148, 0, 211, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 20, 147, 255).</summary>
    public static Colour DeepPink => new(byte.MaxValue, 20, 147, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 191, 255, 255).</summary>
    public static Colour DeepSkyBlue => new(0, 191, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (105, 105, 105, 255).</summary>
    public static Colour DimGray => new(105, 105, 105, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (30, 144, 255, 255).</summary>
    public static Colour DodgerBlue => new(30, 144, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (178, 34, 34, 255).</summary>
    public static Colour Firebrick => new(178, 34, 34, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 250, 240, 255).</summary>
    public static Colour FloralWhite => new(byte.MaxValue, 250, 240, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (34, 139, 34, 255).</summary>
    public static Colour ForestGreen => new(34, 139, 34, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 0, 255, 255).</summary>
    public static Colour Fuchsia => new(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (220, 220, 220, 255).</summary>
    public static Colour Gainsboro => new(220, 220, 220, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (248, 248, 255, 255).</summary>
    public static Colour GhostWhite => new(248, 248, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 215, 0, 255).</summary>
    public static Colour Gold => new(byte.MaxValue, 215, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (218, 165, 32, 255).</summary>
    public static Colour Goldenrod => new(218, 165, 32, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (128, 128, 128, 255).</summary>
    public static Colour Gray => new(128, 128, 128, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 128, 0, 255).</summary>
    public static Colour Green => new(0, 128, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (173, 255, 47, 255).</summary>
    public static Colour GreenYellow => new(173, byte.MaxValue, 47, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (240, 255, 240, 255).</summary>
    public static Colour Honeydew => new(240, byte.MaxValue, 240, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 105, 180, 255).</summary>
    public static Colour HotPink => new(byte.MaxValue, 105, 180, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (205, 92, 92, 255).</summary>
    public static Colour IndianRed => new(205, 92, 92, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (75, 0, 130, 255).</summary>
    public static Colour Indigo => new(75, 0, 130, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 255, 240, 255).</summary>
    public static Colour Ivory => new(byte.MaxValue, byte.MaxValue, 240, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (240, 230, 140, 255).</summary>
    public static Colour Khaki => new(240, 230, 140, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (230, 230, 250, 255).</summary>
    public static Colour Lavender => new(230, 230, 250, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 240, 245, 255).</summary>
    public static Colour LavenderBlush => new(byte.MaxValue, 240, 245, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (124, 252, 0, 255).</summary>
    public static Colour LawnGreen => new(124, 252, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 250, 205, 255).</summary>
    public static Colour LemonChiffon => new(byte.MaxValue, 250, 205, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (173, 216, 230, 255).</summary>
    public static Colour LightBlue => new(173, 216, 230, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (240, 128, 128, 255).</summary>
    public static Colour LightCoral => new(240, 128, 128, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (224, 255, 255, 255).</summary>
    public static Colour LightCyan => new(224, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (250, 250, 210, 255).</summary>
    public static Colour LightGoldenrodYellow => new(250, 250, 210, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (144, 238, 144, 255).</summary>
    public static Colour LightGreen => new(144, 238, 144, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (211, 211, 211, 255).</summary>
    public static Colour LightGray => new(211, 211, 211, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 182, 193, 255).</summary>
    public static Colour LightPink => new(byte.MaxValue, 182, 193, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 160, 122, 255).</summary>
    public static Colour LightSalmon => new(byte.MaxValue, 160, 122, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (32, 178, 170, 255).</summary>
    public static Colour LightSeaGreen => new(32, 178, 170, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (135, 206, 250, 255).</summary>
    public static Colour LightSkyBlue => new(135, 206, 250, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (119, 136, 153, 255).</summary>
    public static Colour LightSlateGray => new(119, 136, 153, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (176, 196, 222, 255).</summary>
    public static Colour LightSteelBlue => new(176, 196, 222, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 255, 224, 255).</summary>
    public static Colour LightYellow => new(byte.MaxValue, byte.MaxValue, 224, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 255, 0, 255).</summary>
    public static Colour Lime => new(0, byte.MaxValue, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (50, 205, 50, 255).</summary>
    public static Colour LimeGreen => new(50, 205, 50, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (250, 240, 230, 255).</summary>
    public static Colour Linen => new(250, 240, 230, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 0, 255, 255).</summary>
    public static Colour Magenta => new(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (128, 0, 0, 255).</summary>
    public static Colour Maroon => new(128, 0, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (102, 205, 170, 255).</summary>
    public static Colour MediumAquamarine => new(102, 205, 170, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 0, 205, 255).</summary>
    public static Colour MediumBlue => new(0, 0, 205, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (186, 85, 211, 255).</summary>
    public static Colour MediumOrchid => new(186, 85, 211, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (147, 112, 219, 255).</summary>
    public static Colour MediumPurple => new(147, 112, 219, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (60, 179, 113, 255).</summary>
    public static Colour MediumSeaGreen => new(60, 179, 113, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (123, 104, 238, 255).</summary>
    public static Colour MediumSlateBlue => new(123, 104, 238, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 250, 154, 255).</summary>
    public static Colour MediumSpringGreen => new(0, 250, 154, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (72, 209, 204, 255).</summary>
    public static Colour MediumTurquoise => new(72, 209, 204, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (199, 21, 133, 255).</summary>
    public static Colour MediumVioletRed => new(199, 21, 133, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (25, 25, 112, 255).</summary>
    public static Colour MidnightBlue => new(25, 25, 112, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (245, 255, 250, 255).</summary>
    public static Colour MintCream => new(245, byte.MaxValue, 250, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 228, 225, 255).</summary>
    public static Colour MistyRose => new(byte.MaxValue, 228, 225, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 228, 181, 255).</summary>
    public static Colour Moccasin => new(byte.MaxValue, 228, 181, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 222, 173, 255).</summary>
    public static Colour NavajoWhite => new(byte.MaxValue, 222, 173, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 0, 128, 255).</summary>
    public static Colour Navy => new(0, 0, 128, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (253, 245, 230, 255).</summary>
    public static Colour OldLace => new(253, 245, 230, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (128, 128, 0, 255).</summary>
    public static Colour Olive => new(128, 128, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (107, 142, 35, 255).</summary>
    public static Colour OliveDrab => new(107, 142, 35, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 165, 0, 255).</summary>
    public static Colour Orange => new(byte.MaxValue, 165, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 69, 0, 255).</summary>
    public static Colour OrangeRed => new(byte.MaxValue, 69, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (218, 112, 214, 255).</summary>
    public static Colour Orchid => new(218, 112, 214, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (238, 232, 170, 255).</summary>
    public static Colour PaleGoldenrod => new(238, 232, 170, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (152, 251, 152, 255).</summary>
    public static Colour PaleGreen => new(152, 251, 152, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (175, 238, 238, 255).</summary>
    public static Colour PaleTurquoise => new(175, 238, 238, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (219, 112, 147, 255).</summary>
    public static Colour PaleVioletRed => new(219, 112, 147, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 239, 213, 255).</summary>
    public static Colour PapayaWhip => new(byte.MaxValue, 239, 213, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 218, 185, 255).</summary>
    public static Colour PeachPuff => new(byte.MaxValue, 218, 185, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (205, 133, 63, 255).</summary>
    public static Colour Peru => new(205, 133, 63, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 192, 203, 255).</summary>
    public static Colour Pink => new(byte.MaxValue, 192, 203, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (221, 160, 221, 255).</summary>
    public static Colour Plum => new(221, 160, 221, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (176, 224, 230, 255).</summary>
    public static Colour PowderBlue => new(176, 224, 230, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (128, 0, 128, 255).</summary>
    public static Colour Purple => new(128, 0, 128, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 0, 0, 255).</summary>
    public static Colour Red => new(byte.MaxValue, 0, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (188, 143, 143, 255).</summary>
    public static Colour RosyBrown => new(188, 143, 143, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (65, 105, 225, 255).</summary>
    public static Colour RoyalBlue => new(65, 105, 225, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (139, 69, 19, 255).</summary>
    public static Colour SaddleBrown => new(139, 69, 19, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (250, 128, 114, 255).</summary>
    public static Colour Salmon => new(250, 128, 114, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (244, 164, 96, 255).</summary>
    public static Colour SandyBrown => new(244, 164, 96, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (46, 139, 87, 255).</summary>
    public static Colour SeaGreen => new(46, 139, 87, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 245, 238, 255).</summary>
    public static Colour SeaShell => new(byte.MaxValue, 245, 238, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (160, 82, 45, 255).</summary>
    public static Colour Sienna => new(160, 82, 45, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (192, 192, 192, 255).</summary>
    public static Colour Silver => new(192, 192, 192, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (135, 206, 235, 255).</summary>
    public static Colour SkyBlue => new(135, 206, 235, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (106, 90, 205, 255).</summary>
    public static Colour SlateBlue => new(106, 90, 205, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (112, 128, 144, 255).</summary>
    public static Colour SlateGray => new(112, 128, 144, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 250, 250, 255).</summary>
    public static Colour Snow => new(byte.MaxValue, 250, 250, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 255, 127, 255).</summary>
    public static Colour SpringGreen => new(0, byte.MaxValue, 127, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (70, 130, 180, 255).</summary>
    public static Colour SteelBlue => new(70, 130, 180, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (210, 180, 140, 255).</summary>
    public static Colour Tan => new(210, 180, 140, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (0, 128, 128, 255).</summary>
    public static Colour Teal => new(0, 128, 128, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (216, 191, 216, 255).</summary>
    public static Colour Thistle => new(216, 191, 216, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 99, 71, 255).</summary>
    public static Colour Tomato => new(byte.MaxValue, 99, 71, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (64, 224, 208, 255).</summary>
    public static Colour Turquoise => new(64, 224, 208, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (238, 130, 238, 255).</summary>
    public static Colour Violet => new(238, 130, 238, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (245, 222, 179, 255).</summary>
    public static Colour Wheat => new(245, 222, 179, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 255, 255, 255).</summary>
    public static Colour White => new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (245, 245, 245, 255).</summary>
    public static Colour WhiteSmoke => new(245, 245, 245, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (255, 255, 0, 255).</summary>
    public static Colour Yellow => new(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);

    /// <summary>Gets the system colour with (R, G, B, A) = (154, 205, 50, 255).</summary>
    public static Colour YellowGreen => new(154, 205, 50, byte.MaxValue);
    #endregion

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

    /// <summary>Returns this colour as an OpenTK <see cref="OpenTK.Mathematics.Color4"/>.</summary>
    public readonly OpenTK.Mathematics.Color4 ToOpenTK() => new(NormalisedR, NormalisedG, NormalisedB, NormalisedA);

    /// <summary>Returns this colour with the alpha replaced.</summary>
    /// <param name="alpha">New alpha component, 0–255.</param>
    public readonly Colour WithAlpha(byte alpha) => new(R, G, B, alpha);
}

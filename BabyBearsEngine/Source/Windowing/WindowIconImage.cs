namespace BabyBearsEngine;

/// <summary>
/// A single bitmap within a <see cref="WindowIcon"/>, at one specific resolution. Pixels are RGBA
/// bytes in row-major order, so <see cref="Pixels"/> length must equal
/// <see cref="Width"/> × <see cref="Height"/> × 4.
/// </summary>
/// <param name="Width">Image width in pixels.</param>
/// <param name="Height">Image height in pixels.</param>
/// <param name="Pixels">RGBA pixel data, row-major, with one byte per channel.</param>
public sealed record WindowIconImage(int Width, int Height, byte[] Pixels);

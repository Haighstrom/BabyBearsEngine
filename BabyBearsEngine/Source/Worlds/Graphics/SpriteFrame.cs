namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Describes one frame of a <see cref="NonSquareAnimation"/>: the on-screen output size and
/// the normalised UV region (0–1) within the texture that supplies the pixels.
/// </summary>
public readonly record struct SpriteFrame(float OutputWidth, float OutputHeight, float U1, float V1, float U2, float V2);

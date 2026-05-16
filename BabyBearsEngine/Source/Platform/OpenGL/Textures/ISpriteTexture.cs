namespace BabyBearsEngine.OpenGL;

/// <summary>
/// An <see cref="ITexture"/> that describes an evenly-tiled sprite sheet — a grid of
/// <see cref="Columns"/> × <see cref="Rows"/> frames all the same pixel size.
/// </summary>
public interface ISpriteTexture : ITexture
{
    /// <summary>Number of frames across the sheet.</summary>
    int Columns { get; }

    /// <summary>Number of frames down the sheet.</summary>
    int Rows { get; }

    /// <summary>Total frames (<see cref="Columns"/> × <see cref="Rows"/>).</summary>
    int Frames { get; }

    /// <summary>Normalised UV width of one frame, accounting for any padding between frames.</summary>
    float FrameU { get; }

    /// <summary>Normalised UV height of one frame, accounting for any padding between frames.</summary>
    float FrameV { get; }

    /// <summary>
    /// Returns the normalised UV rectangle (u1, v1) → (u2, v2) for the given zero-based
    /// frame index. Frames are ordered left-to-right, top-to-bottom.
    /// </summary>
    (float U1, float V1, float U2, float V2) GetFrameUVs(int frame);
}

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>A renderable graphic that displays one frame of a sprite sheet at a time.</summary>
public interface ISprite : IGraphic
{
    /// <summary>Zero-based index of the currently displayed frame.</summary>
    int Frame { get; set; }

    /// <summary>Total number of frames in the sprite sheet.</summary>
    int Frames { get; }
}

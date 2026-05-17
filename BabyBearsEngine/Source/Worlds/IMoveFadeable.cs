namespace BabyBearsEngine.Worlds;

/// <summary>
/// Implemented by entities or graphics that can be moved, faded, and removed by a
/// <see cref="MoveFadeRemoveController"/>.
/// </summary>
public interface IMoveFadeable : IAddable
{
    /// <summary>X position in the parent's local space. The controller writes this each frame.</summary>
    float X { get; set; }

    /// <summary>Y position in the parent's local space. The controller writes this each frame.</summary>
    float Y { get; set; }

    /// <summary>Tint / fill colour. The controller fades the alpha channel to zero over the effect lifetime.</summary>
    Colour Colour { get; set; }
}

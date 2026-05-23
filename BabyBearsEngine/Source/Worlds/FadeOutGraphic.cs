using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// A short-lived overlay graphic that drifts in a chosen direction while fading out, then
/// disappears on its own. The standard shape for floating damage numbers, "+5" pickup hints,
/// drifting arrows, and similar "show it briefly, then forget it" effects — give it any graphic,
/// a velocity, and a lifetime, drop it into the world, and it cleans up after itself.
/// </summary>
/// <remarks>
/// Wraps the supplied <see cref="IGraphic"/> together with a <see cref="MoveFadeRemoveController"/>.
/// For richer behaviour (different easing, mid-flight changes, multiple graphics) build the
/// graphic + controller pair yourself instead.
/// </remarks>
public sealed class FadeOutGraphic : Entity
{
    /// <param name="graphic">The graphic to display. Its <see cref="IRectAddable.X"/> and <see cref="IRectAddable.Y"/> are taken as this container's initial position, then reset to (0, 0) so the graphic renders at the container's local origin.</param>
    /// <param name="velocityX">Horizontal drift speed in pixels per second.</param>
    /// <param name="velocityY">Vertical drift speed in pixels per second.</param>
    /// <param name="duration">Effect lifetime in seconds before this container removes itself.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public FadeOutGraphic(IGraphic graphic, float velocityX, float velocityY, double duration, int layer = 0)
        : base(graphic.X, graphic.Y, graphic.Width, graphic.Height, layer: layer)
    {
        graphic.X = 0f;
        graphic.Y = 0f;
        Add(graphic);

        MoveFadeRemoveController controller = new(graphic, velocityX, velocityY, duration);
        controller.Completed += OnEffectCompleted;
        Add(controller);
    }

    private void OnEffectCompleted(object? sender, EventArgs e)
    {
        Remove();
    }
}

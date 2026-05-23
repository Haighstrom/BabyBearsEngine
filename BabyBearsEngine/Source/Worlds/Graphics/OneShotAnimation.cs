using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// An <see cref="Animation"/> that plays every frame once on attachment and then removes itself
/// from its parent. Useful for one-off visual effects (puffs, sparkles, hit flashes) where the
/// caller does not want to manage playback or cleanup.
/// </summary>
/// <param name="texture">Sprite sheet texture.</param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="frameDuration">Seconds per frame. Defaults to <see cref="Animation.DefaultFrameDuration"/>.</param>
public sealed class OneShotAnimation : Animation
{
    public OneShotAnimation(ISpriteTexture texture, float x, float y, float width, float height, double frameDuration = DefaultFrameDuration)
        : base(texture, x, y, width, height, frameDuration)
    {
        AnimationComplete += OnAnimationCompleted;
    }

    protected override void OnAdded()
    {
        base.OnAdded();
        Play(looping: false);
    }

    private void OnAnimationCompleted(object? sender, EventArgs e)
    {
        Remove();
    }
}

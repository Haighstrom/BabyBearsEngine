using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A <see cref="Sprite"/> that advances through a sequence of frames over time.
/// Call one of the <c>Play</c> overloads to start; the animation is inactive by default.
/// </summary>
/// <param name="texture">Sprite sheet texture.</param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="frameDuration">Seconds per frame. Defaults to 0.2 s (5 fps).</param>
public class Animation(ISpriteTexture texture, float x, float y, float width, float height, double frameDuration = Animation.DefaultFrameDuration)
    : Sprite(texture, x, y, width, height), IUpdateable
{
    /// <summary>Default seconds per frame (0.2 s = 5 fps) when <c>frameDuration</c> is not specified.</summary>
    public const double DefaultFrameDuration = 0.2;

    private IList<int> _framesToPlay = [0];
    private int _currentFrameIndex = 0;
    private double _timeToNextFrame = 0.0;
    private bool _looping = true;

    /// <summary>
    /// Whether the animation is currently advancing. Set to <c>false</c> to pause without
    /// losing playback state. The scene graph skips <see cref="Update"/> when this is
    /// <c>false</c>.
    /// </summary>
    public bool Active { get; set; } = false;

    /// <summary>A convenience list of every frame index in the sheet, in order.</summary>
    public IList<int> AllFrames => Enumerable.Range(0, Frames).ToList();

    /// <summary>Seconds each frame is displayed.</summary>
    public double FrameDuration { get; set; } = frameDuration;

    /// <summary>Raised when a non-looping animation reaches its last frame.</summary>
    public event EventHandler? AnimationComplete;

    /// <summary>
    /// Starts playback with full control: which frames to play, which frame to start on,
    /// how much time is left for that first frame, and whether to loop.
    /// </summary>
    public void Play(IList<int> frames, int startFrameIndex, double firstFrameRemaining, bool looping)
    {
        Active = true;
        _framesToPlay = frames;
        _currentFrameIndex = startFrameIndex;
        _timeToNextFrame = firstFrameRemaining;
        Frame = frames[startFrameIndex];
        _looping = looping;
    }

    /// <summary>
    /// Starts playback through the given frames (or all frames if none specified).
    /// </summary>
    /// <param name="looping">Whether to loop after the last frame.</param>
    /// <param name="frames">Frames to play. Omit to play all frames.</param>
    public void Play(bool looping, params int[] frames)
    {
        IList<int> list = frames.Length == 0 ? AllFrames : frames;
        Play(list, 0, FrameDuration, looping);
    }

    /// <summary>Starts playback through the given frames, looping. Omit to loop all frames.</summary>
    public void Play(params int[] frames) => Play(true, frames);

    /// <summary>Syncs this animation to another — same frames, frame index, and remaining time.</summary>
    public void Play(Animation other) => Play(other._framesToPlay, other._currentFrameIndex, other._timeToNextFrame, other._looping);

    /// <inheritdoc/>
    public void Update(double elapsed)
    {
        if (!Active || _framesToPlay.Count <= 1)
        {
            return;
        }

        _timeToNextFrame -= elapsed;

        while (_timeToNextFrame <= 0)
        {
            if (_currentFrameIndex == _framesToPlay.Count - 1 && !_looping)
            {
                Active = false;
                _timeToNextFrame = 0.0;
                AnimationComplete?.Invoke(this, EventArgs.Empty);
                break;
            }
            else
            {
                _currentFrameIndex = ((_currentFrameIndex + 1) % _framesToPlay.Count);
                Frame = _framesToPlay[_currentFrameIndex];
                _timeToNextFrame += FrameDuration;
            }
        }
    }
}

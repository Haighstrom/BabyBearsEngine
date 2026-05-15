using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Rendering;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// An animated graphic where each frame can have a different output size and UV region.
/// Useful for attack animations and other sequences whose frames don't fit a uniform grid.
/// Construction allocates GL resources — must be created on the engine thread.
/// </summary>
public class NonSquareAnimation : GraphicBase, IGraphic, IUpdateable, IDisposable
{
    private const double DefaultFrameDuration = 0.2;

    private readonly GraphicRenderer _renderer;
    private readonly IList<SpriteFrame> _frames;
    private int[] _framesToPlay = [0];
    private int _currentFrameIndex = 0;
    private double _timeToNextFrame = 0.0;
    private bool _looping = true;
    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <param name="texture">Source texture (not owned; not disposed with this object).</param>
    /// <param name="x">X position in the parent's local space.</param>
    /// <param name="y">Y position in the parent's local space.</param>
    /// <param name="frames">Frame definitions in display order.</param>
    /// <param name="frameDuration">Seconds per frame.</param>
    /// <param name="initialFrame">Zero-based index of the first frame to display.</param>
    public NonSquareAnimation(ITexture texture, float x, float y, IList<SpriteFrame> frames, double frameDuration = DefaultFrameDuration, int initialFrame = 0)
        : base(x, y, frames[initialFrame].OutputWidth, frames[initialFrame].OutputHeight)
    {
        _frames = frames;
        _renderer = new GraphicRenderer(texture);
        FrameDuration = frameDuration;
        SetFrame(initialFrame);
    }

    /// <summary>Whether the animation is advancing. The scene graph skips <see cref="Update"/> when false.</summary>
    public bool Active { get; set; } = false;

    /// <summary>A convenience list of every frame index, in order.</summary>
    public IList<int> AllFrames => Enumerable.Range(0, _frames.Count).ToList();

    /// <inheritdoc cref="IGraphic.Colour"/>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Zero-based index of the currently displayed frame.</summary>
    public int CurrentFrame => _framesToPlay[_currentFrameIndex];

    /// <summary>Seconds each frame is displayed.</summary>
    public double FrameDuration { get; set; }

    /// <summary>Total number of frame definitions.</summary>
    public int TotalFrames => _frames.Count;

    /// <summary>Raised when a non-looping animation reaches its last frame.</summary>
    public event EventHandler? AnimationComplete;

    /// <summary>Syncs this animation to another — same frames, index, and remaining time.</summary>
    public void PlayMatching(NonSquareAnimation other) => PlayFrom(other._looping, other._currentFrameIndex, other._timeToNextFrame, other._framesToPlay);

    /// <summary>Plays all frames once, then raises <see cref="AnimationComplete"/>.</summary>
    public void PlayAllOnce() => Play(false, AllFrames.ToArray());

    /// <summary>Plays all frames looping.</summary>
    public void PlayAllLooping() => Play(true, AllFrames.ToArray());

    /// <summary>Shows a single static frame.</summary>
    public void Play(int frame) => Play(false, frame);

    /// <summary>Plays the given frames, looping or not. Omit frames to play all.</summary>
    public void Play(bool looping, params int[] frames)
    {
        int[] list = frames.Length == 0 ? AllFrames.ToArray() : frames;
        PlayFrom(looping, 0, FrameDuration, list);
    }

    /// <summary>Starts playback with full state control.</summary>
    public void PlayFrom(bool looping, int startIndex, double firstFrameRemaining, params int[] frames)
    {
        Active = true;
        _framesToPlay = frames;
        _currentFrameIndex = startIndex;
        _timeToNextFrame = firstFrameRemaining;
        _looping = looping;
        SetFrame(frames[startIndex]);
    }

    /// <inheritdoc/>
    public void Update(double elapsed)
    {
        if (!Active || _framesToPlay.Length <= 1)
        {
            return;
        }

        _timeToNextFrame -= elapsed;

        while (_timeToNextFrame <= 0)
        {
            if (_currentFrameIndex == _framesToPlay.Length - 1 && !_looping)
            {
                Active = false;
                _timeToNextFrame = 0.0;
                AnimationComplete?.Invoke(this, EventArgs.Empty);
                break;
            }
            else
            {
                _currentFrameIndex = (_currentFrameIndex + 1) % _framesToPlay.Length;
                SetFrame(_framesToPlay[_currentFrameIndex]);
                _timeToNextFrame += FrameDuration;
            }
        }
    }

    /// <inheritdoc/>
    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (Width == 0 || Height == 0)
        {
            return;
        }

        if (_verticesChanged)
        {
            SpriteFrame f = _frames[_framesToPlay[_currentFrameIndex]];
            _renderer.UpdateVertices(Width, Height, _colour, f.U1, f.V1, f.U2, f.V2);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);
        _renderer.Render(ref projection, ref mv);
    }

    private void SetFrame(int frameIndex)
    {
        SpriteFrame f = _frames[frameIndex];
        Width = f.OutputWidth;
        Height = f.OutputHeight;
        _verticesChanged = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _renderer.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}

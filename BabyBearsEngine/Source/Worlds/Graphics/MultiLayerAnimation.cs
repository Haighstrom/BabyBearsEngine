using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Rendering;
using OpenTK.Mathematics;
using Matrix3 = BabyBearsEngine.Geometry.Matrix3;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// An animated graphic that renders multiple sprite-sheet textures in sync, composited as
/// layers on a single quad. All textures must share the same frame count and UV grid. Layers
/// with a higher <c>subLayer</c> value are drawn further behind (matching the higher-is-behind
/// convention used by the camera and <see cref="ContainerEntity"/>).
/// Construction allocates GL resources — must be created on the engine thread.
/// </summary>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="frameDuration">Seconds per frame.</param>
/// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
public class MultiLayerAnimation(float x, float y, float width, float height, double frameDuration = MultiLayerAnimation.DefaultFrameDuration, int layer = 0) : GraphicBase(x, y, width, height, layer), IGraphic, IUpdateable, IDisposable
{
    private const double DefaultFrameDuration = 0.2;

    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private readonly StandardMatrixShaderProgram _shader = new();
    private readonly List<(ISpriteTexture Texture, int SubLayer)> _layers = [];
    private IList<int> _framesToPlay = [0];
    private int _currentFrameIndex = 0;
    private int _frame = 0;
    private double _timeToNextFrame = 0.0;
    private bool _looping = true;
    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    private ISpriteTexture ReferenceTexture
    {
        get
        {
            if (_layers.Count == 0)
            {
                throw new InvalidOperationException("Cannot play a MultiLayerAnimation until at least one texture has been added via AddTexture.");
            }
            return _layers[0].Texture;
        }
    }

    /// <summary>Whether the animation is advancing. The scene graph skips <see cref="Update"/> when false.</summary>
    public bool Active { get; set; } = false;

    /// <summary>A convenience list of every frame index, in order.</summary>
    public IList<int> AllFrames => Enumerable.Range(0, ReferenceTexture.Frames).ToList();

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

    /// <summary>Seconds each frame is displayed.</summary>
    public double FrameDuration { get; set; } = frameDuration;

    /// <summary>Total frames, derived from the first added texture.</summary>
    public int Frames => ReferenceTexture.Frames;

    /// <summary>Raised when a non-looping animation reaches its last frame.</summary>
    public event EventHandler? AnimationComplete;

    /// <summary>
    /// Adds a sprite-sheet texture at the given sub-layer. Higher sub-layers render further behind.
    /// All textures must have the same frame count and UV layout as the first one added.
    /// </summary>
    public void AddTexture(ISpriteTexture texture, int subLayer = 0)
    {
        if (_layers.Count > 0)
        {
            if (texture.Frames != ReferenceTexture.Frames)
            {
                throw new ArgumentException($"Texture has {texture.Frames} frames but the reference texture has {ReferenceTexture.Frames}.");
            }
        }

        _layers.Add((texture, subLayer));
        // Render order is the list order: highest sub-layer first (drawn behind), lowest last (on top).
        _layers.Sort((a, b) => b.SubLayer.CompareTo(a.SubLayer));
    }

    /// <summary>Removes a previously added texture.</summary>
    public void RemoveTexture(ISpriteTexture texture)
    {
        int index = _layers.FindIndex(t => t.Texture == texture);

        if (index >= 0)
        {
            _layers.RemoveAt(index);
        }
    }

    /// <summary>
    /// Starts playback with full state control.
    /// </summary>
    public void Play(IList<int> frames, int startFrameIndex, double firstFrameRemaining, bool looping)
    {
        Active = true;
        _framesToPlay = frames;
        _currentFrameIndex = startFrameIndex;
        _timeToNextFrame = firstFrameRemaining;
        _frame = frames[startFrameIndex];
        _looping = looping;
        _verticesChanged = true;
    }

    /// <summary>Plays the given frames, looping or not. Omit frames to play all.</summary>
    public void Play(bool looping, params int[] frames)
    {
        IList<int> list = frames.Length == 0 ? AllFrames : frames;
        Play(list, 0, FrameDuration, looping);
    }

    /// <summary>Starts looping playback through the given frames. Omit to loop all frames.</summary>
    public void Play(params int[] frames) => Play(true, frames);

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
                _currentFrameIndex = (_currentFrameIndex + 1) % _framesToPlay.Count;
                _frame = _framesToPlay[_currentFrameIndex];
                _timeToNextFrame += FrameDuration;
                _verticesChanged = true;
            }
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _verticesChanged = true;
    }

    /// <inheritdoc/>
    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (Width == 0 || Height == 0 || _layers.Count == 0)
        {
            return;
        }

        if (_verticesChanged)
        {
            var (u1, v1, u2, v2) = ReferenceTexture.GetFrameUVs(_frame);
            Color4 c = _colour.ToOpenTK();
            _vertexDataBuffer.SetNewVertices([
                new Vertex(0,     0,      c, u1, v1),
                new Vertex(Width, 0,      c, u2, v1),
                new Vertex(0,     Height, c, u1, v2),
                new Vertex(Width, Height, c, u2, v2),
            ]);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);
        _shader.Bind();
        _vertexDataBuffer.Bind();
        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv);

        foreach (var (texture, _) in _layers)
        {
            texture.Bind();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _vertexDataBuffer.Dispose();
            _shader.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}

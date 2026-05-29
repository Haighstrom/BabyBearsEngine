using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Worlds.Particles;

/// <summary>
/// A GPU-rendered 2D particle system. Adds to a world/entity like any other graphic; one
/// instance owns its emitter shape, particle storage, and GL pipeline (vertex buffer + shader).
/// Construction allocates GL resources — must be created on the engine thread after the GL
/// context exists. Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
/// <remarks>
/// <para>The system advances during <see cref="Update"/>: each frame it integrates particle
/// positions, removes any whose lifetime has elapsed, and (when <see cref="Emitting"/> is true)
/// spawns new particles at <see cref="EmissionRate"/> per second up to <see cref="MaxParticles"/>.
/// During <see cref="Render"/> it uploads the current particles to a VBO and draws them as
/// camera-facing quads via the billboard geometry-shader pipeline.</para>
///
/// <para>Per-particle visual evolution is configured via <see cref="ColourOverLife"/> and
/// <see cref="SizeOverLife"/>, each receiving a normalised lifetime <c>t</c> in <c>[0, 1]</c>
/// (0 = just spawned, 1 = about to die) and the particle's starting colour/size. Defaults are
/// identity functions (no fade or shrink).</para>
/// </remarks>
public sealed class ParticleSystem : AddableRectBase, IUpdateable, IRenderable, ILayered, IDisposable
{
    private readonly List<Particle> _particles = [];
    private readonly Random _random;
    private ParticleShaderProgram? _shader = null;
    private TexturedParticleShaderProgram? _texturedShader = null;
    private VertexDataBuffer<ParticleVertex>? _vertexDataBuffer = null;
    private IEmitterShape _emitterShape;
    private double _emitCounter = 0;
    private int _layer = int.MaxValue;
    private bool _disposed = false;

    /// <param name="emitterShape">Strategy for sampling each particle's spawn position and velocity.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    /// <param name="random">Optional random source. When null, a fresh <see cref="System.Random"/> is created. Pass a seeded instance for deterministic spawn patterns (unit tests, replays).</param>
    public ParticleSystem(IEmitterShape emitterShape, int layer = int.MaxValue, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(emitterShape);
        ArgumentOutOfRangeException.ThrowIfNegative(layer);
        _emitterShape = emitterShape;
        _layer = layer;
        _random = random ?? new Random();
    }

    /// <inheritdoc/>
    public bool Active { get; set; } = true;

    /// <summary>Colour applied to every emitted particle as its <see cref="Particle.StartColour"/>. Defaults to <see cref="Colour.White"/>.</summary>
    public Colour Colour { get; set; } = Colour.White;

    /// <summary>
    /// Function that returns the rendered colour for a particle based on its normalised
    /// lifetime <c>t</c> (0 → 1) and its <see cref="Particle.StartColour"/>. Default returns
    /// the start colour unchanged. Use <see cref="Colour.Lerp"/> for the common "fade to
    /// transparent" or "shift hue over life" patterns.
    /// </summary>
    public Func<float, Colour, Colour> ColourOverLife { get; set; } = static (t, startColour) => startColour;

    /// <summary>Particles emitted per second while <see cref="Emitting"/> is true. Default 30. Must be ≥ 0.</summary>
    public float EmissionRate { get; set; } = 30f;

    /// <summary>Whether the system spawns new particles each frame. Existing particles continue to age and render regardless. Default true.</summary>
    public bool Emitting { get; set; } = true;

    /// <summary>Strategy used to sample each new particle's spawn position and velocity. Not null; assigning null throws <see cref="ArgumentNullException"/>.</summary>
    public IEmitterShape EmitterShape
    {
        get => _emitterShape;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _emitterShape = value;
        }
    }

    /// <inheritdoc/>
    public int Layer
    {
        get => _layer;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            int old = _layer;
            _layer = value;
            if (old != value)
            {
                LayerChanged?.Invoke(this, new LayerChangedEventArgs(old, value));
            }
        }
    }

    /// <summary>Lifetime in seconds applied to every emitted particle as its <see cref="Particle.TotalLifetime"/>. Default 2.0 seconds. Must be > 0.</summary>
    public float Lifetime { get; set; } = 2f;

    /// <summary>Hard cap on active particles. New emissions stop once this many are alive; existing particles continue to age. Default 5000. Must be ≥ 0.</summary>
    public int MaxParticles { get; set; } = 5000;

    /// <summary>Number of live particles currently in flight. Useful for diagnostics and tests.</summary>
    public int ParticleCount => _particles.Count;

    /// <summary>
    /// The shader program used to render this system, lazily constructed on first <see cref="Render"/>.
    /// Returns null until rendering has occurred; this lets <see cref="Update"/>-only test
    /// harnesses exercise the system without a live GL context.
    /// </summary>
    public ParticleShaderProgram? Shader => _shader;

    /// <summary>
    /// Per-axis size in pixels applied to every emitted particle as its
    /// <see cref="Particle.StartSize"/>. X is quad width, Y is quad height. Pass equal
    /// components for a square sprite (the common case); unequal components for stretched
    /// billboards — rain streaks (small X, large Y), wide shockwaves (large X, small Y).
    /// Default <c>(16, 16)</c>. Components must be > 0.
    /// </summary>
    public Point StartSize { get; set; } = new(16f, 16f);

    /// <summary>
    /// Optional texture sampled across each particle's quad, multiplied by the per-particle
    /// colour. Null (the default) draws particles as solid coloured quads via
    /// <see cref="ParticleShaderProgram"/>; non-null switches to
    /// <see cref="TexturedParticleShaderProgram"/> and binds this texture before every draw.
    /// The texture is not owned by the system — disposing the system does not dispose the
    /// texture. Assigning a different texture is free; the textured shader program is lazily
    /// constructed on the first render that needs it.
    /// </summary>
    public ITexture? Texture { get; set; } = null;

    /// <summary>
    /// Function that returns the rendered per-axis size for a particle based on its
    /// normalised lifetime <c>t</c> (0 → 1) and its <see cref="Particle.StartSize"/>. Default
    /// returns the start size unchanged. Use e.g.
    /// <c>(t, s) => new Point(s.X * (1 - t), s.Y * (1 - t))</c> for shrinking-to-nothing on
    /// both axes, or scale axes independently to e.g. lengthen rain streaks as they fall.
    /// </summary>
    public Func<float, Point, Point> SizeOverLife { get; set; } = static (t, startSize) => startSize;

    /// <inheritdoc/>
    public bool Visible { get; set; } = true;

    /// <inheritdoc/>
    public event EventHandler<LayerChangedEventArgs>? LayerChanged;

    /// <summary>
    /// Spawns <paramref name="count"/> particles immediately. Useful for one-shot effects
    /// (explosions, pickup bursts) — does not interact with <see cref="EmissionRate"/> /
    /// <see cref="Emitting"/>. Respects <see cref="MaxParticles"/>.
    /// </summary>
    public void EmitBurst(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        for (int i = 0; i < count && _particles.Count < MaxParticles; i++)
        {
            _particles.Add(SpawnParticle());
        }
    }

    /// <summary>Removes all live particles. Does not change <see cref="Emitting"/> — call <c>Emitting = false</c> first if you want a clean stop.</summary>
    public void Clear()
    {
        _particles.Clear();
        _emitCounter = 0;
    }

    /// <inheritdoc/>
    public void Update(double elapsed)
    {
        AgeAndIntegrate(elapsed);

        if (Emitting && _particles.Count < MaxParticles)
        {
            _emitCounter += elapsed * EmissionRate;
            while (_emitCounter >= 1 && _particles.Count < MaxParticles)
            {
                _particles.Add(SpawnParticle());
                _emitCounter -= 1;
            }
        }
        else
        {
            // While emission is off (or capped), don't let the counter accrue beyond one tick's
            // worth — otherwise re-enabling later would dump a backlog of particles all at once.
            _emitCounter = Math.Min(_emitCounter, 1);
        }
    }

    /// <inheritdoc/>
    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (_particles.Count == 0)
        {
            return;
        }

        // Lazy-init so Update-only test paths don't need a GL context. The first render call
        // happens on the engine thread with the context live, matching the construction
        // requirements of every other GL-backed graphic in the engine. Each shader program
        // type is constructed on first use only — a system that stays untextured never
        // allocates the textured program, and vice versa.
        _vertexDataBuffer ??= new VertexDataBuffer<ParticleVertex>();

        IMatrixShaderProgram activeShader;
        if (Texture is not null)
        {
            _texturedShader ??= new TexturedParticleShaderProgram();
            activeShader = _texturedShader;
        }
        else
        {
            _shader ??= new ParticleShaderProgram();
            activeShader = _shader;
        }

        activeShader.Bind();
        _vertexDataBuffer.Bind();
        Texture?.Bind();

        ParticleVertex[] vertices = BuildVertices();
        _vertexDataBuffer.SetNewVertices(vertices);

        Matrix3 mv = Matrix3.Translate(ref modelView, X, Y);
        activeShader.SetProjectionMatrix(ref projection);
        activeShader.SetModelViewMatrix(ref mv);

        GL.DrawArrays(PrimitiveType.Points, 0, vertices.Length);
    }

    private void AgeAndIntegrate(double elapsed)
    {
        // Iterate in reverse so RemoveAt(i) doesn't shift indices we still need to visit.
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            Particle particle = _particles[i];
            particle.RemainingLifetime -= (float)elapsed;
            if (particle.RemainingLifetime <= 0)
            {
                _particles.RemoveAt(i);
                continue;
            }
            particle.Position = new Point(
                particle.Position.X + particle.Velocity.X * (float)elapsed,
                particle.Position.Y + particle.Velocity.Y * (float)elapsed);
            _particles[i] = particle;
        }
    }

    private Particle SpawnParticle()
    {
        ParticleSpawn spawn = _emitterShape.Sample(_random);
        return new Particle
        {
            Position = spawn.Position,
            Velocity = spawn.Velocity,
            StartColour = Colour,
            StartSize = StartSize,
            TotalLifetime = Lifetime,
            RemainingLifetime = Lifetime,
        };
    }

    private ParticleVertex[] BuildVertices()
    {
        ParticleVertex[] vertices = new ParticleVertex[_particles.Count];
        for (int i = 0; i < _particles.Count; i++)
        {
            Particle particle = _particles[i];
            float t = particle.TotalLifetime > 0
                ? Math.Clamp(1f - particle.RemainingLifetime / particle.TotalLifetime, 0f, 1f)
                : 1f;
            Colour currentColour = ColourOverLife(t, particle.StartColour);
            Point currentSize = SizeOverLife(t, particle.StartSize);
            vertices[i] = new ParticleVertex(
                particle.Position.X,
                particle.Position.Y,
                currentColour.ToOpenTK(),
                currentSize.X,
                currentSize.Y);
        }
        return vertices;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _vertexDataBuffer?.Dispose();
                _shader?.Dispose();
                _texturedShader?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

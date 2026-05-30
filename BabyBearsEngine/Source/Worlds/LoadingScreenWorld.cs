using System.Threading;
using System.Threading.Tasks;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Lifecycle state of a <see cref="LoadingScreenWorld"/>.
/// </summary>
public enum LoadingScreenState
{
    /// <summary>The world has been constructed but <see cref="LoadingScreenWorld.Load"/> has not yet started the background task.</summary>
    Pending = 0,

    /// <summary>The background load task is in flight.</summary>
    Loading = 1,

    /// <summary>The load task finished successfully. The world will switch to the next world on the next update tick.</summary>
    Completed = 2,

    /// <summary>The load task threw — see <see cref="LoadingScreenWorld.LoadError"/>. The world does not auto-switch in this case.</summary>
    Failed = 3,
}

/// <summary>
/// Payload for <see cref="LoadingScreenWorld.LoadFailed"/>.
/// </summary>
public sealed class LoadingScreenFailedEventArgs(Exception error)
{
    /// <summary>The exception thrown by the load delegate (with <see cref="AggregateException"/> unwrapped where possible).</summary>
    public Exception Error { get; } = error;
}

/// <summary>
/// A <see cref="World"/> shown while assets (or any other work) are loaded asynchronously, then
/// automatically hands off to a target world once the load completes.
/// <para>
/// The load delegate runs on a background <see cref="Task"/> and reports progress (0..1) via an
/// <see cref="IProgress{T}"/>. The default visual is a centred <see cref="ProgressBar"/> tracking
/// the reported progress; the bar is exposed via <see cref="Bar"/> for repositioning or
/// restyling, and additional children (background image, text label, animated cog, etc.) can be
/// added with <see cref="World.Add(IAddable)"/>.
/// </para>
/// <para>
/// Threading rule: the load delegate runs off the main thread, so it must not touch OpenGL.
/// Loading work that needs the GL context (texture upload, shader compilation, font atlas
/// generation) belongs in the constructor of the next world, or in the
/// <see cref="LoadCompleted"/> handler before the world switch is applied.
/// </para>
/// </summary>
public class LoadingScreenWorld : World
{
    private const float DefaultBarHeight = 30f;
    private const float DefaultBarWidth = 400f;
    // Y position as a fraction of window height — slightly below centre, like a typical splash screen.
    private const float DefaultBarYFraction = 0.7f;

    private readonly Func<IProgress<float>, CancellationToken, Task> _loadWork;
    private readonly Func<IWorld> _nextWorldFactory;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly AtomicFloatProgress _progress = new();
    private readonly IWorldSwitcher? _worldSwitcherOverride;

    private ProgressBar _bar;
    private Task? _loadTask;
    private LoadingScreenState _state = LoadingScreenState.Pending;
    private Exception? _loadError = null;
    private bool _handoffDone = false;

    /// <summary>
    /// Creates a loading screen world that runs <paramref name="loadWork"/> on a background
    /// thread and switches to the world produced by <paramref name="nextWorldFactory"/> when it
    /// completes successfully. The default progress bar visual is used.
    /// </summary>
    /// <param name="loadWork">The async work to perform. Receives an <see cref="IProgress{T}"/> for reporting progress in [0, 1] and a <see cref="CancellationToken"/> that fires when this world is unloaded.</param>
    /// <param name="nextWorldFactory">Factory for the world to switch to once loading completes. Invoked on the main thread.</param>
    public LoadingScreenWorld(
        Func<IProgress<float>, CancellationToken, Task> loadWork,
        Func<IWorld> nextWorldFactory)
        : this(loadWork, nextWorldFactory, ProgressBarTheme.Default)
    {
    }

    /// <summary>
    /// Creates a loading screen world with a custom <see cref="ProgressBarTheme"/> for the
    /// default centred progress bar.
    /// </summary>
    /// <param name="loadWork">The async work to perform. Receives an <see cref="IProgress{T}"/> for reporting progress in [0, 1] and a <see cref="CancellationToken"/> that fires when this world is unloaded.</param>
    /// <param name="nextWorldFactory">Factory for the world to switch to once loading completes. Invoked on the main thread.</param>
    /// <param name="barTheme">Visual styling for the default progress bar.</param>
    public LoadingScreenWorld(
        Func<IProgress<float>, CancellationToken, Task> loadWork,
        Func<IWorld> nextWorldFactory,
        ProgressBarTheme barTheme)
        : this(loadWork, nextWorldFactory, barTheme, worldSwitcherOverride: null)
    {
    }

    // Internal ctor used by tests to inject a fake world switcher without going through Engine /
    // EngineConfiguration. The public ctors funnel into this one.
    internal LoadingScreenWorld(
        Func<IProgress<float>, CancellationToken, Task> loadWork,
        Func<IWorld> nextWorldFactory,
        ProgressBarTheme barTheme,
        IWorldSwitcher? worldSwitcherOverride)
    {
        ArgumentNullException.ThrowIfNull(loadWork);
        ArgumentNullException.ThrowIfNull(nextWorldFactory);
        ArgumentNullException.ThrowIfNull(barTheme);

        _loadWork = loadWork;
        _nextWorldFactory = nextWorldFactory;
        _worldSwitcherOverride = worldSwitcherOverride;

        float windowWidth = Window.Width;
        float windowHeight = Window.Height;
        Rect barRect = new(
            (windowWidth - DefaultBarWidth) / 2f,
            windowHeight * DefaultBarYFraction,
            DefaultBarWidth,
            DefaultBarHeight);

        _bar = new ProgressBar(barRect, barTheme);
        Add(_bar);
    }

    /// <summary>
    /// The default progress bar that tracks reported progress. Exposed so callers can reposition,
    /// resize, or restyle it after construction, or replace it via <see cref="World.Remove"/> +
    /// <see cref="World.Add(IAddable)"/> if they want a fully custom visual.
    /// </summary>
    public ProgressBar Bar => _bar;

    /// <summary>The most recent progress value reported by the load delegate, in [0, 1].</summary>
    public float Progress => _progress.Value;

    /// <summary>The world's lifecycle state — see <see cref="LoadingScreenState"/>.</summary>
    public LoadingScreenState State => _state;

    /// <summary>The exception thrown by the load delegate if <see cref="State"/> is <see cref="LoadingScreenState.Failed"/>; <c>null</c> otherwise.</summary>
    public Exception? LoadError => _loadError;

    /// <summary>
    /// Raised on the main thread when the load delegate completes successfully, immediately
    /// before the world is switched. Use this hook for any main-thread, post-load work that must
    /// happen before the next world's constructor runs (e.g. promoting CPU-side image data to GL
    /// textures, finalising state derived from the loaded assets).
    /// </summary>
    public event Action? LoadCompleted;

    /// <summary>
    /// Raised on the main thread if the load delegate throws. The world does <b>not</b>
    /// auto-switch when this fires; the handler is expected to drive the next step (e.g. show an
    /// error screen, retry, or fall back to a degraded asset set).
    /// </summary>
    public event Action<LoadingScreenFailedEventArgs>? LoadFailed;

    /// <inheritdoc/>
    /// <remarks>Kicks off the background load task. Idempotent — calling <see cref="Load"/> a second time on the same instance has no effect.</remarks>
    public override void Load()
    {
        base.Load();

        if (_state != LoadingScreenState.Pending)
        {
            return;
        }

        _state = LoadingScreenState.Loading;
        CancellationToken token = _cancellation.Token;
        _loadTask = Task.Run(() => _loadWork(_progress, token), token);
    }

    /// <inheritdoc/>
    /// <remarks>Cancels the background load task (the token passed to the load delegate fires here) before tearing the world down.</remarks>
    public override void Unload()
    {
        if (!_cancellation.IsCancellationRequested)
        {
            _cancellation.Cancel();
        }

        base.Unload();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Mirrors the latest reported progress onto <see cref="Bar"/>, then — if the background task
    /// has finished — raises <see cref="LoadCompleted"/> / <see cref="LoadFailed"/> and (on
    /// success) triggers the world switch via <see cref="Engine.ChangeWorld(Func{IWorld})"/>.
    /// </remarks>
    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        // Progress is reported from the load thread; mirror the latest value onto the bar each
        // frame on the main thread so the visual stays in sync without needing a sync context.
        _bar.AmountFilled = _progress.Value;

        if (_handoffDone || _loadTask is null || !_loadTask.IsCompleted)
        {
            return;
        }

        if (_loadTask.IsFaulted)
        {
            _loadError = UnwrapAggregate(_loadTask.Exception);
            _state = LoadingScreenState.Failed;
            _handoffDone = true;
            Logger.Warning($"LoadingScreenWorld load delegate threw: {_loadError?.GetType().Name}: {_loadError?.Message}");
            LoadFailed?.Invoke(new LoadingScreenFailedEventArgs(_loadError!));
            return;
        }

        if (_loadTask.IsCanceled)
        {
            // The world was unloaded while loading — nothing more to do. State stays Loading
            // because the world is going away anyway; we don't fire any event here.
            _handoffDone = true;
            return;
        }

        // Successful completion. Snap the bar to full so a one-shot load with no intermediate
        // progress reports still looks "finished" for the frame before the world switches.
        _progress.Report(1f);
        _bar.AmountFilled = 1f;
        _state = LoadingScreenState.Completed;
        _handoffDone = true;

        LoadCompleted?.Invoke();

        IWorldSwitcher switcher = _worldSwitcherOverride ?? EngineConfiguration.WorldSwitcher;
        switcher.RequestWorldChange(_nextWorldFactory);
    }

    private static Exception? UnwrapAggregate(AggregateException? aggregate)
    {
        if (aggregate is null)
        {
            return null;
        }

        return aggregate.InnerExceptions.Count == 1 ? aggregate.InnerException : aggregate;
    }

    /// <summary>
    /// Thread-safe <see cref="IProgress{T}"/> sink for a single <c>float</c> in [0, 1]. The load
    /// thread writes via <see cref="Report"/>; the main thread reads via <see cref="Value"/>.
    /// Implemented with <see cref="Interlocked"/> on the bit pattern so no synchronisation
    /// context or lock is required.
    /// </summary>
    private sealed class AtomicFloatProgress : IProgress<float>
    {
        private int _bits = 0;

        public float Value => BitConverter.Int32BitsToSingle(Volatile.Read(ref _bits));

        public void Report(float value) => Volatile.Write(ref _bits, BitConverter.SingleToInt32Bits(value));
    }
}

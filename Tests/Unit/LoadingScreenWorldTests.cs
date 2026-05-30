using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;
using Colour = BabyBearsEngine.Colour;
using Point = System.Drawing.Point;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class LoadingScreenWorldTests
{
    private sealed class FakeWindow : IWindow
    {
        public WindowBorder Border { get; set; }
        public bool CursorLockedToWindow { get; set; }
        public CursorShape Cursor { get; set; }
        public bool CursorVisible { get; set; }
        public bool CloseOnXButton { get; set; }
        public int Height { get; set; } = 600;
        public WindowIcon Icon { get; set; } = new WindowIcon();
        public Point MaxClientSize { get; set; }
        public Point MinClientSize { get; set; }
        public WindowState State { get; set; }
        public string Title { get; set; } = "";
        public bool VSync { get; set; }
        public int Width { get; set; } = 800;
        public int X { get; set; }
        public int Y { get; set; }

        public event Action<WindowResizeEventArgs>? Resize;

        public void Centre() { }
        public void Close() { }
        public void RaiseResize(int width, int height) => Resize?.Invoke(new WindowResizeEventArgs(width, height));
    }

    private sealed class FakeWorldSwitcher : IWorldSwitcher
    {
        public int RequestCount { get; private set; } = 0;
        public IWorld? LastRequestedWorld { get; private set; } = null;
        public Func<IWorld>? LastRequestedFactory { get; private set; } = null;

        public void RequestWorldChange(IWorld world)
        {
            LastRequestedWorld = world;
            RequestCount++;
        }

        public void RequestWorldChange(Func<IWorld> createWorld)
        {
            LastRequestedFactory = createWorld;
            // Resolve the factory eagerly so the test can assert on the produced world.
            LastRequestedWorld = createWorld();
            RequestCount++;
        }
    }

    private sealed class StubGraphic : GraphicBase, IGraphic
    {
        public Colour Colour { get; set; }

        public float Angle { get; set; } = 0f;

        public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
        {
        }
    }

    private static ProgressBarTheme StubTheme() => new()
    {
        BackgroundFactory = _ => new StubGraphic(),
        FillFactory = _ => new StubGraphic(),
    };

    private FakeWindow _window = null!;
    private FakeWorldSwitcher _switcher = null!;

    [TestInitialize]
    public void Setup()
    {
        _window = new FakeWindow();
        EngineConfiguration.WindowService = _window;
        _switcher = new FakeWorldSwitcher();
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    private LoadingScreenWorld Make(
        Func<IProgress<float>, CancellationToken, Task>? loadWork = null,
        Func<IWorld>? nextWorldFactory = null)
    {
        loadWork ??= (_, _) => Task.CompletedTask;
        nextWorldFactory ??= () => new World();
        return new LoadingScreenWorld(loadWork, nextWorldFactory, StubTheme(), _switcher);
    }

    // Spins Update() until either the predicate is true or the timeout elapses. Used because
    // Task.Run hops to the thread pool and we need to give the background task a chance to land.
    private static void TickUntil(LoadingScreenWorld world, Func<bool> predicate, int timeoutMs = 2000)
    {
        var deadline = Environment.TickCount + timeoutMs;
        while (Environment.TickCount < deadline)
        {
            world.Update(0.016);
            if (predicate())
            {
                return;
            }
            Thread.Sleep(1);
        }
        Assert.Fail($"Predicate did not become true within {timeoutMs} ms.");
    }

    // Construction

    [TestMethod]
    public void Constructor_NullLoadWork_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new LoadingScreenWorld(null!, () => new World()));
    }

    [TestMethod]
    public void Constructor_NullNextWorldFactory_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new LoadingScreenWorld((_, _) => Task.CompletedTask, null!));
    }

    [TestMethod]
    public void Constructor_NullTheme_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new LoadingScreenWorld((_, _) => Task.CompletedTask, () => new World(), null!));
    }

    [TestMethod]
    public void Constructor_InitialStateIsPending()
    {
        LoadingScreenWorld world = Make();

        Assert.AreEqual(LoadingScreenState.Pending, world.State);
    }

    [TestMethod]
    public void Constructor_InitialProgressIsZero()
    {
        LoadingScreenWorld world = Make();

        Assert.AreEqual(0f, world.Progress);
    }

    [TestMethod]
    public void Constructor_BarIsCentredHorizontally()
    {
        _window.Width = 1000;
        _window.Height = 500;

        LoadingScreenWorld world = Make();

        // Bar default width is 400; centred horizontally at (1000-400)/2 = 300.
        Assert.AreEqual(300f, world.Bar.X);
    }

    // Load lifecycle

    [TestMethod]
    public void Load_TransitionsToLoading()
    {
        var gate = new ManualResetEventSlim();
        LoadingScreenWorld world = Make(loadWork: (_, _) =>
        {
            gate.Wait();
            return Task.CompletedTask;
        });

        world.Load();

        Assert.AreEqual(LoadingScreenState.Loading, world.State);
        gate.Set();
    }

    [TestMethod]
    public void Load_CalledTwice_DoesNotKickOffTwoTasks()
    {
        int invocationCount = 0;
        var gate = new ManualResetEventSlim();
        LoadingScreenWorld world = Make(loadWork: (_, _) =>
        {
            Interlocked.Increment(ref invocationCount);
            gate.Wait();
            return Task.CompletedTask;
        });

        world.Load();
        world.Load();

        // Give the (single) background task a moment to actually start incrementing.
        Thread.Sleep(50);
        Assert.AreEqual(1, invocationCount);
        gate.Set();
    }

    // Successful hand-off

    [TestMethod]
    public void Update_AfterLoadCompletes_RequestsWorldChange()
    {
        var nextWorld = new World();
        LoadingScreenWorld world = Make(
            loadWork: (_, _) => Task.CompletedTask,
            nextWorldFactory: () => nextWorld);
        world.Load();

        TickUntil(world, () => _switcher.RequestCount > 0);

        Assert.AreSame(nextWorld, _switcher.LastRequestedWorld);
    }

    [TestMethod]
    public void Update_AfterLoadCompletes_StateIsCompleted()
    {
        LoadingScreenWorld world = Make();
        world.Load();

        TickUntil(world, () => world.State == LoadingScreenState.Completed);

        Assert.AreEqual(LoadingScreenState.Completed, world.State);
    }

    [TestMethod]
    public void Update_AfterLoadCompletes_BarIsFull()
    {
        LoadingScreenWorld world = Make();
        world.Load();

        TickUntil(world, () => world.State == LoadingScreenState.Completed);

        Assert.AreEqual(1f, world.Bar.AmountFilled);
    }

    [TestMethod]
    public void Update_AfterLoadCompletes_RaisesLoadCompleted()
    {
        int completedCount = 0;
        LoadingScreenWorld world = Make();
        world.LoadCompleted += () => completedCount++;
        world.Load();

        TickUntil(world, () => completedCount > 0);

        Assert.AreEqual(1, completedCount);
    }

    [TestMethod]
    public void Update_LoadCompletedRaisedBeforeWorldChangeRequested()
    {
        int? completedRequestCountSnapshot = null;
        LoadingScreenWorld world = Make();
        world.LoadCompleted += () => completedRequestCountSnapshot = _switcher.RequestCount;
        world.Load();

        TickUntil(world, () => _switcher.RequestCount > 0);

        Assert.AreEqual(0, completedRequestCountSnapshot);
    }

    [TestMethod]
    public void Update_AfterLoadCompletes_DoesNotRequestWorldChangeTwice()
    {
        LoadingScreenWorld world = Make();
        world.Load();
        TickUntil(world, () => _switcher.RequestCount > 0);

        // Extra ticks should be no-ops.
        for (int tick = 0; tick < 5; tick++)
        {
            world.Update(0.016);
        }

        Assert.AreEqual(1, _switcher.RequestCount);
    }

    // Progress reporting

    [TestMethod]
    public void Update_MirrorsReportedProgressOntoBar()
    {
        var reported = new ManualResetEventSlim();
        var release = new ManualResetEventSlim();
        LoadingScreenWorld world = Make(loadWork: (progress, _) =>
        {
            progress.Report(0.42f);
            reported.Set();
            release.Wait();
            return Task.CompletedTask;
        });
        world.Load();

        Assert.IsTrue(reported.Wait(2000));
        // The load thread has reported but the task is still gated; advance one Update tick.
        world.Update(0.016);

        Assert.AreEqual(0.42f, world.Bar.AmountFilled, 1e-4f);
        Assert.AreEqual(0.42f, world.Progress, 1e-4f);

        release.Set();
    }

    // Failure path — the framework deliberately doesn't try to recover from load failures.
    // It re-throws on the main thread so the exception lands in GameLauncher.Run's fatal
    // handler. Game code that wants graceful recovery must try/catch inside its load delegate.

    [TestMethod]
    public void Update_AfterLoadThrows_RethrowsExceptionOnMainThread()
    {
        var thrown = new InvalidOperationException("boom");
        LoadingScreenWorld world = Make(loadWork: (_, _) => throw thrown);
        world.Load();

        // Wait for the worker task to fault, then assert the next Update throws it back at us.
        InvalidOperationException? rethrown = null;
        var deadline = Environment.TickCount + 2000;
        while (Environment.TickCount < deadline && rethrown is null)
        {
            try
            {
                world.Update(0.016);
            }
            catch (InvalidOperationException ex)
            {
                rethrown = ex;
            }
            Thread.Sleep(1);
        }

        Assert.IsNotNull(rethrown, "Update did not rethrow the worker exception within 2 seconds.");
        Assert.AreSame(thrown, rethrown);
    }

    [TestMethod]
    public void Update_AfterLoadThrows_DoesNotRequestWorldChange()
    {
        LoadingScreenWorld world = Make(loadWork: (_, _) => throw new InvalidOperationException("boom"));
        world.Load();

        var deadline = Environment.TickCount + 2000;
        bool threw = false;
        while (Environment.TickCount < deadline && !threw)
        {
            try { world.Update(0.016); }
            catch (InvalidOperationException) { threw = true; }
            Thread.Sleep(1);
        }

        Assert.IsTrue(threw, "Update did not rethrow the worker exception.");
        Assert.AreEqual(0, _switcher.RequestCount);
    }

    [TestMethod]
    public void Update_AfterLoadThrows_DoesNotRaiseLoadCompleted()
    {
        int completedCount = 0;
        LoadingScreenWorld world = Make(loadWork: (_, _) => throw new InvalidOperationException("boom"));
        world.LoadCompleted += () => completedCount++;
        world.Load();

        var deadline = Environment.TickCount + 2000;
        bool threw = false;
        while (Environment.TickCount < deadline && !threw)
        {
            try { world.Update(0.016); }
            catch (InvalidOperationException) { threw = true; }
            Thread.Sleep(1);
        }

        Assert.IsTrue(threw, "Update did not rethrow the worker exception.");
        Assert.AreEqual(0, completedCount);
    }

    // Unload + cancellation

    [TestMethod]
    public void Unload_CancelsLoadToken()
    {
        bool wasCancelled = false;
        var started = new ManualResetEventSlim();
        var stopped = new ManualResetEventSlim();
        LoadingScreenWorld world = Make(loadWork: (_, token) =>
        {
            started.Set();
            try
            {
                token.WaitHandle.WaitOne();
                wasCancelled = token.IsCancellationRequested;
            }
            finally
            {
                stopped.Set();
            }
            return Task.CompletedTask;
        });
        world.Load();
        Assert.IsTrue(started.Wait(2000));

        world.Unload();

        Assert.IsTrue(stopped.Wait(2000));
        Assert.IsTrue(wasCancelled);
    }
}

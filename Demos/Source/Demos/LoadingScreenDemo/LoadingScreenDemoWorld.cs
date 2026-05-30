using System;
using System.Threading;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.LoadingScreenDemo;

/// <summary>
/// Entry point for the LoadingScreen demo. Shows a button that kicks off
/// <see cref="GameAssetsLoadingWorld"/>, which loads the actual game assets on a background
/// thread and then switches to <see cref="LoadingResultWorld"/> to display them.
/// </summary>
internal class LoadingScreenDemoWorld : DemoWorld
{
    private const int ButtonHeight = 50;
    private const int ButtonWidth = 280;
    private const int InstructionWidth = 600;

    private readonly Func<World> _menuWorldFactory;

    public override string Name => "Loading Screen";

    public LoadingScreenDemoWorld(Func<World> menuWorldFactory)
        : base(menuWorldFactory)
    {
        _menuWorldFactory = menuWorldFactory;

        int instructionX = (Window.Width - InstructionWidth) / 2;
        Add(MakeCentredLabel(
            instructionX,
            120,
            InstructionWidth,
            40,
            "LoadingScreenWorld loads assets on a background thread with a shared GL context."));
        Add(MakeCentredLabel(
            instructionX,
            160,
            InstructionWidth,
            40,
            "Click below to run the load; the progress bar fills as each step completes."));

        int buttonX = (Window.Width - ButtonWidth) / 2;
        Button startButton = new(
            buttonX,
            240,
            ButtonWidth,
            ButtonHeight,
            ButtonTheme.FromColour(new Colour(120, 200, 140)),
            "Start load");
        startButton.LeftClicked += (_, _) => Engine.ChangeWorld(() => new GameAssetsLoadingWorld(_menuWorldFactory));
        Add(startButton);
    }

    internal static TextGraphic MakeCentredLabel(int x, int y, int width, int height, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 18), text, Colour.Black, x, y, width, height)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
    }
}

/// <summary>
/// Example <see cref="LoadingScreenWorld"/> subclass. The base class only provides the worker
/// thread + shared GL context; everything visible (progress bar, current-step label, world
/// handoff after loading) is owned here. That's deliberate — different games want very
/// different loading-screen visuals, so the base takes no opinion.
/// </summary>
internal sealed class GameAssetsLoadingWorld : LoadingScreenWorld
{
    private const float BarHeight = 30f;
    private const float BarWidth = 400f;
    private const float BarYFraction = 0.7f;

    private static readonly (string Name, Action<GameAssetsLoadingWorld> Work)[] s_steps =
    [
        ("Connecting to the cloud...",        _ => Thread.Sleep(500)),
        ("Downloading critical updates...",   _ => Thread.Sleep(700)),
        ("Decompressing audio bundles...",    _ => Thread.Sleep(600)),
        ("Loading bear texture",              w => w._loadedBear = Textures.CreateFromFile("Assets/SpinnableBear.png")),
        ("Compiling shaders...",              _ => Thread.Sleep(400)),
        ("Warming up GPU caches...",          _ => Thread.Sleep(300)),
    ];

    private static readonly ProgressBarTheme s_loadingBarTheme = ProgressBarTheme.FromBorderedColours(
        background: new Colour(40, 40, 60),
        fill: new Colour(110, 180, 255),
        border: new Colour(220, 230, 240),
        borderThickness: 2f);

    private readonly Func<World> _menuWorldFactory;
    private readonly ProgressBar _bar;
    private readonly TextGraphic _stepLabel;
    // _currentStepName is written from the worker thread (inside LoadAssets) and read from the
    // main thread (inside Update), so use Volatile to publish/observe across threads. _stepsCompleted
    // is integer so Interlocked.Increment/Read handles it.
    private volatile string _currentStepName = "";
    private int _stepsCompleted = 0;
    private ITexture _loadedBear = null!;

    public GameAssetsLoadingWorld(Func<World> menuWorldFactory)
    {
        _menuWorldFactory = menuWorldFactory;

        Rect barRect = new(
            (Window.Width - BarWidth) / 2f,
            Window.Height * BarYFraction,
            BarWidth,
            BarHeight);
        _bar = new ProgressBar(barRect, s_loadingBarTheme);
        Add(_bar);

        _stepLabel = LoadingScreenDemoWorld.MakeCentredLabel(
            0, (int)(barRect.Y + barRect.H + 16), Window.Width, 28,
            "");
        Add(_stepLabel);
    }

    /// <summary>Runs on the worker thread with a shared GL context current.</summary>
    protected override void LoadAssets()
    {
        foreach ((string name, Action<GameAssetsLoadingWorld> work) in s_steps)
        {
            if (LoadingCancelled)
            {
                return;
            }

            _currentStepName = name;
            work(this);
            Interlocked.Increment(ref _stepsCompleted);
        }
    }

    /// <summary>Runs on the main thread once loading is finished.</summary>
    protected override void OnLoadCompleted()
    {
        Engine.ChangeWorld(() => new LoadingResultWorld(_menuWorldFactory, _loadedBear));
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        _bar.AmountFilled = Volatile.Read(ref _stepsCompleted) / (float)s_steps.Length;
        _stepLabel.Text = _currentStepName;
    }
}

/// <summary>
/// Trivial world shown after the loading screen completes. Displays the texture that was loaded
/// on the worker thread and a "Back" button to return to the demo menu.
/// </summary>
internal sealed class LoadingResultWorld : DemoWorld
{
    private const float BearSize = 256f;

    public override string Name => "Loading Screen — Loaded!";

    public LoadingResultWorld(Func<World> menuWorldFactory, ITexture loadedBear)
        : base(menuWorldFactory)
    {
        Add(LoadingScreenDemoWorld.MakeCentredLabel(
            (Window.Width - 600) / 2,
            120,
            600,
            40,
            "Loaded on a background thread, drawn from the main thread:"));

        Add(new TextureGraphic(
            loadedBear,
            (Window.Width - BearSize) / 2f,
            180f,
            BearSize,
            BearSize));
    }
}

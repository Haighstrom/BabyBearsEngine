using System;
using System.Collections.Generic;
using System.Threading;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;
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
/// Example <see cref="LoadingScreenWorld"/> subclass. The two things a game author has to write
/// are <see cref="AssetsToLoad"/> (the list of work) and <see cref="NextWorld"/> (what to
/// switch to when loading finishes). Everything else — threading, progress reporting, GL
/// context setup — is handled by the base class.
/// </summary>
internal sealed class GameAssetsLoadingWorld : LoadingScreenWorld
{
    private static readonly ProgressBarTheme s_loadingBarTheme = ProgressBarTheme.FromBorderedColours(
        background: new Colour(40, 40, 60),
        fill: new Colour(110, 180, 255),
        border: new Colour(220, 230, 240),
        borderThickness: 2f);

    private readonly Func<World> _menuWorldFactory;
    private readonly TextGraphic _stepLabel;
    private ITexture _loadedBear = null!;

    public GameAssetsLoadingWorld(Func<World> menuWorldFactory)
        : base(barTheme: s_loadingBarTheme)
    {
        _menuWorldFactory = menuWorldFactory;

        _stepLabel = LoadingScreenDemoWorld.MakeCentredLabel(
            0, (int)Bar.Y + (int)Bar.Height + 16, Window.Width, 28,
            "");
        Add(_stepLabel);
    }

    /// <summary>
    /// The list of loading steps. Each Work delegate runs on a background thread with a shared
    /// OpenGL context current — <c>Textures.CreateFromFile</c> and other GL constructors work
    /// directly here. Progress is reported automatically: the bar advances by 1/N after each
    /// step completes.
    /// </summary>
    protected override IReadOnlyList<LoadStep> AssetsToLoad => [
        new("Connecting to the cloud...",        () => Thread.Sleep(500)),
        new("Downloading critical updates...",   () => Thread.Sleep(700)),
        new("Decompressing audio bundles...",    () => Thread.Sleep(600)),
        new("Loading bear texture",              () => _loadedBear = Textures.CreateFromFile("Assets/SpinnableBear.png")),
        new("Compiling shaders...",              () => Thread.Sleep(400)),
        new("Warming up GPU caches...",          () => Thread.Sleep(300)),
    ];

    protected override IWorld NextWorld() => new LoadingResultWorld(_menuWorldFactory, _loadedBear);

    public override void Update(double elapsed)
    {
        base.Update(elapsed);
        _stepLabel.Text = CurrentStepName ?? "";
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

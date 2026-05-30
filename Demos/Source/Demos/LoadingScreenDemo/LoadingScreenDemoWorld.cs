using System;
using System.Threading;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.LoadingScreenDemo;

/// <summary>
/// Launches a <see cref="LoadingScreenWorld"/> that actually loads a texture from disk on a
/// background thread with a shared OpenGL context. The progress bar fills in steps as the load
/// reports progress; on completion the demo switches to a world that displays the loaded
/// texture, proving the cross-thread upload worked.
/// </summary>
internal class LoadingScreenDemoWorld : DemoWorld
{
    private const int ButtonHeight = 50;
    private const int ButtonWidth = 280;
    private const int InstructionWidth = 600;
    private const int LoadStepCount = 20;
    private const int LoadStepDelayMilliseconds = 150;
    private const string LoadedTexturePath = "Assets/SpinnableBear.png";

    private static readonly ProgressBarTheme s_loadingBarTheme = ProgressBarTheme.FromBorderedColours(
        background: new Colour(40, 40, 60),
        fill: new Colour(110, 180, 255),
        border: new Colour(220, 230, 240),
        borderThickness: 2f);

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
            "LoadingScreenWorld runs the load work on a background thread with a shared GL context."));
        Add(MakeCentredLabel(
            instructionX,
            160,
            InstructionWidth,
            40,
            "Click below to load a texture off-thread, then switch to a world that displays it."));

        int buttonX = (Window.Width - ButtonWidth) / 2;
        Button startButton = new(
            buttonX,
            240,
            ButtonWidth,
            ButtonHeight,
            ButtonTheme.FromColour(new Colour(120, 200, 140)),
            "Start load");
        startButton.LeftClicked += (_, _) => Engine.ChangeWorld(BuildLoadingScreen);
        Add(startButton);
    }

    private static TextGraphic MakeCentredLabel(int x, int y, int width, int height, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 18), text, Colour.Black, x, y, width, height)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
    }

    private LoadingScreenWorld BuildLoadingScreen()
    {
        ITexture? loadedBear = null;

        return new LoadingScreenWorld(
            loadAssets: (progress, token) =>
            {
                // This runs on a background thread with a shared GL context. We can call texture
                // constructors directly here — no main-thread hop required.
                for (int step = 1; step <= LoadStepCount; step++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(LoadStepDelayMilliseconds);
                    progress.Report((float)step / LoadStepCount);
                }

                loadedBear = Textures.CreateFromFile(LoadedTexturePath);
            },
            nextWorldFactory: () => new LoadingResultWorld(_menuWorldFactory, loadedBear!),
            barTheme: s_loadingBarTheme);
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
        Add(MakeCentredLabel(
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

    private static TextGraphic MakeCentredLabel(int x, int y, int width, int height, string text)
    {
        return new TextGraphic(new FontDefinition("Times New Roman", 18), text, Colour.Black, x, y, width, height)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        };
    }
}

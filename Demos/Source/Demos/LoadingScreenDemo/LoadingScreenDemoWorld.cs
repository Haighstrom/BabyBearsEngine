using System;
using System.Threading.Tasks;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.LoadingScreenDemo;

/// <summary>
/// Launches a <see cref="LoadingScreenWorld"/> with a simulated 3-second async load. The
/// progress bar fills in five evenly-spaced steps as the background task reports progress; on
/// completion the demo switches to a "Loaded!" target world that hosts the standard Back
/// button so the user can return to the menu.
/// </summary>
internal class LoadingScreenDemoWorld : DemoWorld
{
    private const int ButtonHeight = 50;
    private const int ButtonWidth = 280;
    private const int InstructionWidth = 600;
    private const int LoadStepCount = 20;
    private const double LoadStepDelaySeconds = 0.15;

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
            "LoadingScreenWorld runs async work off the main thread and reports progress."));
        Add(MakeCentredLabel(
            instructionX,
            160,
            InstructionWidth,
            40,
            "Click below to simulate a 3-second load and watch the progress bar fill."));

        int buttonX = (Window.Width - ButtonWidth) / 2;
        Button startButton = new(
            buttonX,
            240,
            ButtonWidth,
            ButtonHeight,
            ButtonTheme.FromColour(new Colour(120, 200, 140)),
            "Start fake load");
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
        return new LoadingScreenWorld(
            loadWork: FakeLoadWorkAsync,
            nextWorldFactory: () => new LoadingScreenDemoWorld(_menuWorldFactory),
            barTheme: s_loadingBarTheme);
    }

    private static async Task FakeLoadWorkAsync(IProgress<float> progress, System.Threading.CancellationToken token)
    {
        // Simulated work: LoadStepCount small chunks with a brief delay between each. Reports
        // progress after every chunk so the bar visibly fills rather than jumping from 0 to 1.
        for (int step = 1; step <= LoadStepCount; step++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromSeconds(LoadStepDelaySeconds), token).ConfigureAwait(false);
            progress.Report((float)step / LoadStepCount);
        }
    }
}

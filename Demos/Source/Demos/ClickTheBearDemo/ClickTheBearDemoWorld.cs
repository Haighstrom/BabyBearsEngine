using System;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;

internal class ClickTheBearDemoWorld : DemoWorld
{
    private const double SpawnInterval = 1.5;
    private const int BearSize = 60;
    private const int ScoreIconSize = 24;
    private const int ScoreIconGap = 4;
    private const int ScoreMarginTop = 8;
    private const int ScoreMarginLeft = 95; // start after the 80px-wide Return button at x=5

    private double _spawnTimer = 0;
    private int _scoreCount = 0;

    public override string Name => "Click The Bear";

    public ClickTheBearDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(34, 139, 34);
    }

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        _spawnTimer -= elapsed;
        if (_spawnTimer <= 0)
        {
            SpawnBear();
            _spawnTimer = SpawnInterval;
        }
    }

    private void SpawnBear()
    {
        int scoreBarBottom = ScoreMarginTop + ScoreIconSize + ScoreMarginTop;
        int margin = 20;
        int x = Randomisation.Rand(margin, Window.Width - BearSize - margin);
        int y = Randomisation.Rand(scoreBarBottom + margin, Window.Height - BearSize - margin);

        var bear = new BearTarget(x, y);
        bear.BearClicked += (_, _) => AddScoreIcon();
        Add(bear);
    }

    private void AddScoreIcon()
    {
        int iconsPerRow = (Window.Width - ScoreMarginLeft - ScoreMarginTop) / (ScoreIconSize + ScoreIconGap);
        int col = _scoreCount % iconsPerRow;
        int row = _scoreCount / iconsPerRow;
        float x = ScoreMarginLeft + col * (ScoreIconSize + ScoreIconGap);
        float y = ScoreMarginTop + row * (ScoreIconSize + ScoreIconGap);

        Add(new Image(Textures.CreateFromFile("Assets/SpinnableBear.png"), x, y, ScoreIconSize, ScoreIconSize));
        _scoreCount++;
    }
}

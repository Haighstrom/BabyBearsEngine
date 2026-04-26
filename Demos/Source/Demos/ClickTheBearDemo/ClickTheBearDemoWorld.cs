using System;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;

internal class ClickTheBearDemoWorld : DemoWorld
{
    private const double SpawnInterval = 1.5;
    private const int BearSize = 60;
    private const int ScoreIconSize = 24;
    private const int ScoreIconGap = 4;
    private const int ScoreMargin = 8;

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
        int scoreBarBottom = ScoreMargin + ScoreIconSize + ScoreMargin;
        int margin = 20;
        int x = Randomisation.Rand(margin, Window.Width - BearSize - margin);
        int y = Randomisation.Rand(scoreBarBottom + margin, Window.Height - BearSize - margin);

        var bear = new BearTarget(x, y);
        bear.BearClicked += (_, _) => AddScoreIcon();
        Add(bear);
    }

    private void AddScoreIcon()
    {
        int iconsPerRow = (Window.Width - ScoreMargin * 2) / (ScoreIconSize + ScoreIconGap);
        int col = _scoreCount % iconsPerRow;
        int row = _scoreCount / iconsPerRow;
        float x = ScoreMargin + col * (ScoreIconSize + ScoreIconGap);
        float y = ScoreMargin + row * (ScoreIconSize + ScoreIconGap);

        Add(new Image(Textures.CreateFromFile("Assets/SpinnableBear.png"), x, y, ScoreIconSize, ScoreIconSize));
        _scoreCount++;
    }
}

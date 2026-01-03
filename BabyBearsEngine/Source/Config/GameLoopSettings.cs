namespace BabyBearsEngine.Source.Config;

public sealed record GameLoopSettings()
{
    public static GameLoopSettings Default => new();

    public int TargetFramesPerSecond { get; set; } = 60;

    public bool PeriodicallyLogGameLoopInfoInConsole { get; set; } = false;
}

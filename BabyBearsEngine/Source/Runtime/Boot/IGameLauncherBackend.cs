using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Source.Runtime.Boot;

public interface IGameLauncherBackend
{
    void Run(ApplicationSettings applicationSettings, Func<IWorld> createWorld);
}

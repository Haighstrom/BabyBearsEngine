using BabyBearsEngine.Source.Config;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Source.Boot;

public interface IGameLauncherBackend
{
    void Run(ApplicationSettings applicationSettings, Func<IWorld> createWorld);
}

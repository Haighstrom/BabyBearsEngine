using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.Runtime.Boot;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public interface IGameLauncherBackend
{
    IGameWindowContext CreateWindowContext(ApplicationSettings applicationSettings, Func<IWorld> createWorld);

    void RunGameLoop(IGameWindowContext gameWindowContext);
}

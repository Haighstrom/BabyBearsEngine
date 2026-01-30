using BabyBearsEngine.Runtime;

namespace BabyBearsEngine.PowerUsers;

//Platform Abstraction Layer
public interface IGamePlatform
{
    IPlatformContext Context { get; }

    void CreateWindow(ApplicationSettings appSettings);

    IPlatformContext CreateContext();

    void Run(IGameLoop gameLoop);
}

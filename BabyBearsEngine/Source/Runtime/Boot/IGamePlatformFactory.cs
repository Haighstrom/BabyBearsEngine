using BabyBearsEngine.Runtime;
using BabyBearsEngine.GameEngine;

namespace BabyBearsEngine.Platform.OpenTK;

public interface IGamePlatformFactory
{
    IPlatformContext CreatePlatformContext(IGameEngine gameEngine);
    IGameEngine CreateGameEngine(ApplicationSettings appSettings);
}

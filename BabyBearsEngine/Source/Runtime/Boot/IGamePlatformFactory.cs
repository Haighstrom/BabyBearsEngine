using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.GameEngine;

namespace BabyBearsEngine.Platform.OpenTK;

public interface IGamePlatformFactory
{
    IPlatformContext CreatePlatformContext(IGameEngine gameEngine);
    IGameEngine CreateGameEngine(ApplicationSettings appSettings);
}

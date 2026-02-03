using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.GameEngine;
using BabyBearsEngine.Source.OpenTK;

namespace BabyBearsEngine.Platform.OpenTK;

internal class OpenTKPlatformFactory : IGamePlatformFactory
{
    public IGameEngine CreateGameEngine(ApplicationSettings appSettings)
    {
        return new OpenTKGameEngine(appSettings);
    }

    public IPlatformContext CreatePlatformContext(IGameEngine gameEngine)
    {
        return new OpenTKContext((OpenTKGameEngine)gameEngine);
    }

}

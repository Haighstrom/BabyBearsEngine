using BabyBearsEngine.Runtime;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class Engine
{
    public static void ChangeWorld(IWorld newWorld) => EngineConfiguration.WorldSwitcher.RequestWorldChange(newWorld);
    public static void ChangeWorld(Func<IWorld> createNewWorld) => EngineConfiguration.WorldSwitcher.RequestWorldChange(createNewWorld);
}

using BabyBearsEngine.PowerUsers;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class Engine
{
    private static IWorldGameLoop? s_worldSwitcher;

    public static void Initialise(IWorldGameLoop worldSwitcher)
    {
        ArgumentNullException.ThrowIfNull(worldSwitcher);

        if (s_worldSwitcher is not null)
        {
            throw new InvalidOperationException("Engine has already been initialised.");
        }

        s_worldSwitcher = worldSwitcher;
    }

    public static void ChangeWorld(IWorld newWorld)
    {
        ArgumentNullException.ThrowIfNull(newWorld);

        throw new NotImplementedException();
    }
}

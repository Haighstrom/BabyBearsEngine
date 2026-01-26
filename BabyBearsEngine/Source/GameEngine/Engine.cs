using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class Engine
{
    public static void ChangeWorld(IWorld newWorld)
    {
        ArgumentNullException.ThrowIfNull(newWorld);

        throw new NotImplementedException();
    }
}

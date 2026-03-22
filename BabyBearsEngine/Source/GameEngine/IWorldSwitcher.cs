using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public interface IWorldSwitcher
{
    void RequestWorldChange(IWorld world);

    void RequestWorldChange(Func<IWorld> createWorld);
}

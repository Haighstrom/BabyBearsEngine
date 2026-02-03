using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.PowerUsers;

public interface IWorldSwitcher
{
    void RequestWorldChange(IWorld world);

    void RequestWorldChange(Func<IWorld> createWorld);
}

using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.PowerUsers;

public interface IWorldGameLoop : IGameLoop
{
    void RequestWorldChange(IWorld world);

    void RequestWorldChange(Func<IWorld> createWorld);
}

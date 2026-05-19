using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class Engine
{
    /// <summary>
    /// The engine's current frames-per-second, sampled once per second. Reads 0 until the
    /// first sample window completes after startup. Updates and renders run in lockstep, so
    /// this value reflects both the update and render rate.
    /// </summary>
    public static double Fps => EngineConfiguration.EngineInfo.Fps;

    /// <summary>
    /// Queues a switch to <paramref name="newWorld"/>. The change is applied at the start of
    /// the next update frame: the current world is unloaded, <paramref name="newWorld"/>
    /// becomes the active world, and its <see cref="IWorld.Load"/> hook runs before its first
    /// update. Safe to call from inside an update or event handler.
    /// </summary>
    /// <param name="newWorld">The already-constructed world to switch to.</param>
    public static void ChangeWorld(IWorld newWorld) => EngineConfiguration.WorldSwitcher.RequestWorldChange(newWorld);

    /// <summary>
    /// Queues a switch to a world produced by <paramref name="createNewWorld"/>. The factory is
    /// invoked at the start of the next update frame — not when this method is called — so
    /// constructor-time work (loading assets, allocating GL resources) is deferred until the
    /// previous world has been unloaded. Use this overload when world construction is expensive
    /// or has side effects that should not run until the switch actually takes place.
    /// </summary>
    /// <param name="createNewWorld">Factory that produces the new world.</param>
    public static void ChangeWorld(Func<IWorld> createNewWorld) => EngineConfiguration.WorldSwitcher.RequestWorldChange(createNewWorld);
}

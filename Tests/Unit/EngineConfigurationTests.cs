using System;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class EngineConfigurationTests
{
    private sealed class FakeWorldSwitcher : IWorldSwitcher
    {
        public IWorld? LastRequestedWorld { get; private set; }

        public void RequestWorldChange(IWorld world) => LastRequestedWorld = world;
        public void RequestWorldChange(Func<IWorld> createWorld) => LastRequestedWorld = createWorld();
    }

    private sealed class FakeEngineInfo : IEngineInfo
    {
        public double Fps { get; set; }
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    [TestMethod]
    public void Engine_ChangeWorld_DelegatesToInstalledWorldSwitcher()
    {
        var fake = new FakeWorldSwitcher();
        EngineConfiguration.WorldSwitcher = fake;
        var world = new World();

        Engine.ChangeWorld(world);

        Assert.AreSame(world, fake.LastRequestedWorld);
    }

    [TestMethod]
    public void Engine_Fps_DelegatesToInstalledEngineInfo()
    {
        EngineConfiguration.EngineInfo = new FakeEngineInfo { Fps = 42.5 };

        Assert.AreEqual(42.5, Engine.Fps);
    }
}

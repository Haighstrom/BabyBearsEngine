using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Platform;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Source.GameEngine;

public sealed class WorldGameLoop(
    Func<IWorld> createInitialWorld,
    IRenderHost renderHost)
    : IGameLoop
{
    private readonly Func<IWorld> _createInitialWorld = createInitialWorld;
    private readonly IRenderHost _renderHost = renderHost;

    private IWorld? _world;
    private Func<IWorld>? _pendingWorldFactory;

    public void RequestWorldChange(Func<IWorld> createWorld)
    {
        ArgumentNullException.ThrowIfNull(createWorld);
        _pendingWorldFactory = createWorld;
    }

    public void Load()
    {
        _renderHost.Initialise();

        _world = _createInitialWorld();
        _world.Load();
    }

    public void Unload()
    {
        _world?.Unload();
        _world = null;
    }

    public void Update(double deltaSeconds)
    {
        ApplyPendingWorldChangeIfAny();

        _world?.UpdateThings(deltaSeconds);
    }

    public void Render(double deltaSeconds)
    {
        _renderHost.BeginFrame();

        _world?.DrawGraphics();

        GPUResourceDeletion.ProcessDeletes();

        _renderHost.EndFrame();
    }

    public void HandleScreenResize(int width, int height)
    {
        _renderHost.HandleScreenResize(width, height);
    }

    private void ApplyPendingWorldChangeIfAny()
    {
        var pending = _pendingWorldFactory;

        if (pending is null)
        {
            return;
        }

        _pendingWorldFactory = null;

        _world?.Unload();
        _world = pending();
        _world.Load();
    }
}

using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Platform;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.PowerUsers;

public sealed class WorldGameLoop(IWorld firstWorld, IRenderHost renderHost)
    : IGameLoop, IWorldGameLoop, IDisposable
{
    private readonly IRenderHost _renderHost = renderHost;

    private IWorld? _world;
    private Func<IWorld>? _pendingWorldFactory;
    private bool _disposedValue;

    public void RequestWorldChange(IWorld world) => RequestWorldChange(() => world);

    public void RequestWorldChange(Func<IWorld> createWorld)
    {
        ArgumentNullException.ThrowIfNull(createWorld);
        _pendingWorldFactory = createWorld;
    }

    public void Load()
    {
        _renderHost.Initialise();

        _world = firstWorld;
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

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~WorldGameLoop()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

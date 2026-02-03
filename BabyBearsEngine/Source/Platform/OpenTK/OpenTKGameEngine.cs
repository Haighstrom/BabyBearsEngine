using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.PowerUsers;
using BabyBearsEngine.Source.GameEngine;
using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine;

internal sealed class OpenTKGameEngine(ApplicationSettings appSettings)
    : GameWindow(appSettings.GameLoopSettings.ToOpenTK(), appSettings.WindowSettings.ToOpenTK()), IWorldSwitcher, IGameEngine
{
    private IWorld _world = new World();
    private Func<IWorld>? _pendingWorldFactory;

    private void ApplyPendingWorldChangeIfAny()
    {
        var pending = _pendingWorldFactory;

        if (pending is null)
        {
            return;
        }

        _pendingWorldFactory = null;

        _world.Unload();
        _world = pending();
        _world.Load();
    }

    public void Run(IWorld world)
    {
        _world = world;

        Run();
    }

    public void RequestWorldChange(IWorld world) => RequestWorldChange(() => world);

    public void RequestWorldChange(Func<IWorld> createWorld)
    {
        ArgumentNullException.ThrowIfNull(createWorld);
        _pendingWorldFactory = createWorld;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        _world.Load(); //does nothing currently - world is swapped
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        ApplyPendingWorldChangeIfAny();

        _world.UpdateThings(args.Time);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _world.DrawGraphics();

        GPUResourceDeletion.ProcessDeletes();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }
}

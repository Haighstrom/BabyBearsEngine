using System.ComponentModel;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Source.GameEngine;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine;

internal sealed class OpenTKGameEngine(ApplicationSettings appSettings)
    : GameWindow(appSettings.GameLoopSettings.ToOpenTK(), appSettings.WindowSettings.ToOpenTK()), IWorldSwitcher, IGameEngine
{
    private Func<IWorld>? _pendingWorldFactory;
    private IWorld _world = new World();

    public bool ExitOnClose { get; set; } = appSettings.WindowSettings.ExitOnClose;

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

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!ExitOnClose)
        {
            e.Cancel = true;
        }

        base.OnClosing(e);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        var ws = appSettings.WindowSettings;

        if (ws.Centre)
        {
            CenterWindow();
        }

        CursorState = ws.ToCursorState();
        Cursor = ws.Cursor.ToOpenTK();

        _world.Load(); //does nothing currently - world is swapped
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _world.Draw();

        GPUResourceDeletion.ProcessDeletes();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        ApplyPendingWorldChangeIfAny();

        _world.Update(args.Time);

        MouseSolver.Update();
    }

    public void RequestWorldChange(IWorld world) => RequestWorldChange(() => world);

    public void RequestWorldChange(Func<IWorld> createWorld)
    {
        ArgumentNullException.ThrowIfNull(createWorld);
        _pendingWorldFactory = createWorld;
    }

    public void Run(IWorld world)
    {
        _world = world;

        Run();
    }
}

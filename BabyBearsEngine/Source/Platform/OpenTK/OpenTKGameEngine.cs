using System.ComponentModel;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.GameEngine;
using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine;

internal sealed class OpenTKGameEngine(ApplicationSettings appSettings)
    : GameWindow(appSettings.GameLoopSettings.ToOpenTK(), appSettings.WindowSettings.ToOpenTK()), IWorldSwitcher, IGameEngine
{
    private readonly EngineInfo _engineInfo = new();
    private Func<IWorld>? _pendingWorldFactory;
    internal bool _programmaticClose;
    private IWorld _world = new World();

    public bool CloseOnXButton { get; set; } = appSettings.WindowSettings.CloseOnXButton;

    internal EngineInfo EngineInfo => _engineInfo;

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
        if (!CloseOnXButton && !_programmaticClose)
        {
            e.Cancel = true;
        }

        _programmaticClose = false;

        base.OnClosing(e);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // The bind cache is static and survives between consecutive GameLauncher.Run invocations
        // (e.g. across system tests). Reset it now that a fresh GL context is current so stale
        // handles from a previous context can't suppress real Bind calls.
        OpenGLHelper.ResetCache();

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

        // Capability snapshot must precede LogStartupContext (which reads it) and the engine
        // version gate (which throws on insufficient GL). Both run before any world loads or
        // shaders compile so a refusal produces a clean diagnostic rather than a shader-compile
        // crash or a black screen.
        GpuCapabilities.PopulateFromGL();
        EngineDiagnostics.LogStartupContext(appSettings.LogSettings);

        Version grantedVersion = GpuCapabilities.Current.OpenGLVersion;
        Version requestedVersion = new(ws.OpenGLVersion.major, ws.OpenGLVersion.minor);
        string? versionWarning = GpuCapabilities.GetGrantedBelowRequestedWarning(requestedVersion, grantedVersion);
        if (versionWarning is not null)
        {
            Logger.Warning(versionWarning);
        }

        GpuCapabilities.EnforceEngineMinimum(grantedVersion, GpuCapabilities.EngineMinimumOpenGL);

        _world.Load(); //does nothing currently - world is swapped

        EngineDiagnostics.LogInitialisationComplete();

        IsVisible = true;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        _engineInfo.Tick(args.Time);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _world.Draw();

        GPUResourceDeletion.ProcessDeletes();

        ErrorCode glError = GL.GetError();
        if (glError != ErrorCode.NoError)
        {
            throw new InvalidOperationException($"OpenGL error after frame render: {glError}");
        }

        if (appSettings.DiagnosticsSettings.CaptureFrames)
        {
            EngineConfiguration.ScreenCaptureService.CaptureCurrentBackbuffer();
        }

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

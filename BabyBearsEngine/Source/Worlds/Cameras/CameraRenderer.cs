using System.Collections.Generic;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Buffers;

namespace BabyBearsEngine.Worlds.Cameras;

internal sealed class CameraRenderer : IDisposable
{
    private float _lastHeight = 0f;
    private float _lastWidth = 0f;
    private MsaaFBO? _msaaFBO;
    private readonly CameraMSAAShader? _mSAAShader;
    private FBO _outputFBO;
    private readonly MsaaSamples _samples;
    private readonly StandardMatrixShaderProgram _shader;
    private readonly VertexDataBuffer<Vertex> _vertexBuffer = new();
    private Vertex[] _vertices = [];

    public CameraRenderer(float width, float height, MsaaSamples samples)
    {
        _samples = samples;
        _shader = new();

        if (samples != MsaaSamples.Disabled)
        {
            _msaaFBO = new((int)width, (int)height, (int)samples);
            _mSAAShader = new CameraMSAAShader(samples);
        }

        _outputFBO = new((int)width, (int)height);

        RebuildVertices(width, height);

        var err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Logger.Log($"OpenGL error! (CameraRenderer) {err}");
        }
    }

    public void Dispose()
    {
        _outputFBO.Dispose();
        _msaaFBO?.Dispose();
        _vertexBuffer.Dispose();
    }

    public void Render(Camera camera, IList<IRenderable> renderables, ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (camera.Width != _lastWidth || camera.Height != _lastHeight)
        {
            Resize(camera.Width, camera.Height);
        }

        bool msaaEnabled = _samples != MsaaSamples.Disabled;
        int previousFBO = OpenGLHelper.LastBoundFBO;
        var prevVP = OpenGLHelper.GetViewport();

        var (fboOrtho, identity) = RenderChildrenToFBO(camera, renderables, msaaEnabled);

        if (msaaEnabled)
        {
            ResolveMSAA(ref fboOrtho, ref identity);
        }

        CompositeOntoParent(camera, ref projection, ref modelView, prevVP, previousFBO);
    }

    private void Resize(float width, float height)
    {
        _outputFBO.Dispose();
        _outputFBO = new((int)width, (int)height);

        if (_msaaFBO != null)
        {
            _msaaFBO.Dispose();
            _msaaFBO = new((int)width, (int)height, (int)_samples);
        }

        RebuildVertices(width, height);
    }

    private void ResolveMSAA(ref Matrix3 fboOrtho, ref Matrix3 identity)
    {
        _outputFBO.Bind();
        GL.ClearColor(0, 0, 0, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        _msaaFBO!.BindTexture();

        _mSAAShader!.Bind();
        _mSAAShader.SetProjectionMatrix(ref fboOrtho);
        _mSAAShader.SetModelViewMatrix(ref identity);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, _vertices.Length);

        OpenGLHelper.UnbindTexture(TextureTarget.Texture2DMultisample);
    }

    private void CompositeOntoParent(Camera camera, ref Matrix3 projection, ref Matrix3 modelView, (int X, int Y, int Width, int Height) prevVP, int previousFBO)
    {
        OpenGLHelper.BindFBO(previousFBO);

        _outputFBO.Texture.Bind();
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
        GL.Viewport(prevVP.X, prevVP.Y, prevVP.Width, prevVP.Height);

        var mv2 = modelView;
        mv2 = Matrix3.Translate(ref mv2, camera.X, camera.Y);

        _shader.Bind();
        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv2);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, _vertices.Length);

        var err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Logger.Log($"OpenGL error! (CameraRenderer.Render) {err}");
        }
    }

    private void RebuildVertices(float width, float height)
    {
        var c = Colour.White.ToOpenTK();
        _vertices = [
            new (0,     0,      c, 0, 0),
            new (width, 0,      c, 1, 0),
            new (0,     height, c, 0, 1),
            new (width, height, c, 1, 1),
        ];
        _vertexBuffer.SetNewVertices(_vertices);
        _lastWidth = width;
        _lastHeight = height;
    }

    private (Matrix3 fboOrtho, Matrix3 identity) RenderChildrenToFBO(Camera camera, IList<IRenderable> renderables, bool msaaEnabled)
    {
        _vertexBuffer.Bind();

        if (msaaEnabled)
        {
            _msaaFBO!.Bind();
        }
        else
        {
            _outputFBO.Bind();
        }

        var bg = camera.BackgroundColour;
        GL.ClearColor(bg.R / 255f, bg.G / 255f, bg.B / 255f, bg.A / 255f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        GL.Viewport(0, 0, (int)camera.Width, (int)camera.Height);

        var fboOrtho = Matrix3.CreateFBOOrtho(camera.Width, camera.Height);
        var identity = Matrix3.Identity;
        var mv = Matrix3.ScaleAroundOrigin(ref identity, camera.View.TileWidth, camera.View.TileHeight);
        mv = Matrix3.Translate(ref mv, -camera.View.X, -camera.View.Y);

        foreach (var graphic in renderables)
        {
            if (!graphic.Visible)
            {
                continue;
            }

            graphic.Render(ref fboOrtho, ref mv);
        }

        _vertexBuffer.Bind();

        return (fboOrtho, identity);
    }
}

using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Diagnostics;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Platform.OpenGL.Buffers;
using BabyBearsEngine.Source.Platform.OpenGL.Shaders;
using BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;
using BabyBearsEngine.Source.Worlds;
using BabyBearsEngine.Source.Worlds.Cameras;

namespace BabyBearsEngine.Worlds;

public class Camera : IEntity, IContainer
{
    private readonly VertexDataBuffer<Vertex> _vertexBuffer = new();
    private readonly FBO _shaderPassFBO;
    private readonly MsaaFBO? _msaaFBO;
    private readonly StandardMatrixShaderProgram _shader;
    private readonly CameraMSAAShader _mSAAShader;
    private float _tileWidth, _tileHeight;
    private float _viewX, _viewY, _viewW, _viewH;
    private readonly List<IRenderable> _graphics = [];
    private readonly List<IUpdateable> _updateables = [];

    private Camera(float layer, float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;

        _shader = new();
        _mSAAShader = new CameraMSAAShader((int)width, (int)height, MSAASamples);

        var colour = Colour.White.ToOpenTK();

        Vertices = [
            new (x, y, colour, 0, 0), // bottom left
            new (x + width, y, colour, 1, 0), // bottom right
            new (x, y + height, colour, 0, 1), // top left
            new (x + width, y + height, colour, 1, 1), // top right
        ];

        _vertexBuffer.SetNewVertices(Vertices);

        if (width <= 0 || height <= 0)
        {
            throw new Exception();
        }

        if (MSAASamples != MsaaSamples.Disabled)
        {
            _msaaFBO = new((int)width, (int)height, (int)MSAASamples);
        }

        _shaderPassFBO = new((int)width, (int)height);

        //Check for OpenGL errors
        var err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Logger.Log($"OpenGL error! (Camera.SetUpFBOTex) {err}");
        }
    }

    public Camera(float layer, float x, float y, float width, float height, float tileW, float tileH)
        : this(layer, x, y, width, height)
    {
        FixedTileSize = true;

        //View set automatically - except it isnt?
        TileWidth = tileW;
        TileHeight = tileH;

    }

    public Camera(float layer, float x, float y, float width, float height, float viewX, float viewY, float viewW, float viewH)
        : this(layer, x, y, width, height)
    {
        FixedTileSize = false;

        //TileWidth/TileHeight set automatically
        _viewX = viewX;
        _viewY = viewY;
        _viewW = viewW;
        _viewH = viewH;
    }

    private float X { get; set; }
    private float Y { get; set; }
    private float Width { get; set; }
    private float Height { get; set; }

    private bool MSAAEnabled => MSAASamples != MsaaSamples.Disabled;

    private Vertex[] Vertices { get; set; }

    public float GameSpeed { get; set; } = 1;

    /// <summary>
    /// Angle in Degrees
    /// </summary>
    public float Angle { get; set; }

    public Colour BackgroundColour { get; set; } = Colour.White;

    public bool FixedTileSize { get; set; }

    public float MaxX { get; set; }

    public float MaxY { get; set; }

    public float MinX { get; set; }

    public float MinY { get; set; }

    public MsaaSamples MSAASamples { get; set; } = MsaaSamples.Disabled; //todo: trigger resize if this changes?

    public float TileHeight
    {
        get => _tileHeight;
        set
        {
            _tileHeight = value;

            if (FixedTileSize)
                _viewH = Height / TileHeight;
        }
    }

    public float TileWidth
    {
        get => _tileWidth;
        set
        {
            _tileWidth = value;

            if (FixedTileSize)
                _viewW = Width / value;
        }
    }

    public virtual (float X, float Y, float Width, float Height) View
    {
        get => (_viewX, _viewY, _viewW, _viewH);
        set
        {
            if (!FixedTileSize)
            {
                TileWidth = Width / value.Width;
                TileHeight = Height / value.Height;
            }

            _viewX = value.X;
            _viewY = value.Y;
            _viewW = value.Width;
            _viewH = value.Height;

            ViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ViewChanged;

    public void Update(double elapsed)
    {
        foreach (var updateable in _updateables.ToList())
        {
            updateable.Update(elapsed);
        }
    }

    public void Render(Matrix3 projection)
    {
        _vertexBuffer.Bind();

        //Locally save the current render target, we will then set this camera as the current render target for child cameras, then put it back
        int tempFBID = OpenGLHelper.LastBoundFBO;

        if (MSAAEnabled)
        {
            //Bind MSAA FBO to be the draw destination and clear it
            _msaaFBO!.Bind();
        }
        else
        {
            //Bind Shader Pass FBO to be the draw destination and clear it
            _shaderPassFBO.Bind();
        }

        //Clear the FBO
        GL.ClearColor(BackgroundColour.R / 255f, BackgroundColour.G / 255f, BackgroundColour.B / 255f, BackgroundColour.A / 255f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        //Set normal blend function for within the layers
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        //Save the previous viewport and set the viewport to match the size of the texture we are now drawing to - the FBO
        var prevVP = OpenGLHelper.GetViewport();
        GL.Viewport(0, 0, (int)Width, (int)Height);
        OpenGLHelper.LastBoundFBO = MSAAEnabled ? _msaaFBO!.Handle : _shaderPassFBO.Handle;

        var ortho = Matrix3.CreateFBOOrtho(Width, Height);

        //draw stuff here 
        foreach (var graphic in _graphics.ToList())
        {
            graphic.Render(ortho);
        }

        _vertexBuffer.Bind();

        //Revert the render target 
        OpenGLHelper.LastBoundFBO = tempFBID;
        //Bind the render target back to either the screen, or a camera higher up the heirachy, depending on what called this
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, OpenGLHelper.LastBoundFBO);

        if (MSAAEnabled)
        {
            //Bind the 2nd pass FBO and draw from the first to do MSAA sampling
            _shaderPassFBO.Bind();
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Bind the FBO to be drawn
            _msaaFBO!.BindTexture();

            //Do the MSAA render pass, drawing to the MSAATexture FBO
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);

            //Unbind the FBO 
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
        }

        //Bind the FBO to be drawn
        _shaderPassFBO.Texture.Bind();

        //Set some other blend fucntion when render the FBO texture which apparantly lets the layer alpha blend with the one beneath?
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        //reset viewport
        GL.Viewport(prevVP.X, prevVP.Y, prevVP.Width, prevVP.Height);

        if (MSAAEnabled)
        {
            _mSAAShader.Bind();
        }
        else
        {
            _shader.Bind();
        }

        _shader.SetProjectionMatrix(projection);

        //Render with assigned shader
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);
    }

    public void Resize(float newW, float newH)
    {
        throw new NotImplementedException();
        //W = newW;
        //H = newH;

        ////if (MSAASamples != MSAA_Samples.Disabled)
        ////    HF.Graphics.ResizeMSAAFramebuffer(_frameBufferMSAAID, ref _frameBufferMSAATexture, W, H, MSAASamples);

        ////HF.Graphics.ResizeFramebuffer(_frameBufferShaderPassID, ref _frameBufferShaderPassTexture, W, H);

        //var _colour = Colour.White.ToOpenTK();
        //Vertices =
        //[
        //    new Vertex(0, 0, _colour, 0f, 0f),
        //    new Vertex(Width, 0, _colour, 1f,  0f),
        //    new Vertex(0, Height, _colour, 0f,  1f),
        //    new Vertex(Width, Height, _colour, 1f,  1f)
        //];

        //SetUpFBOTex((int)Width, (int)Height);

        //_ortho = Matrix3.CreateFBOOrtho(Width, Height);

        //if (FixedTileSize)
        //{
        //    View.W = Width / TileWidth;
        //    View.H = Height / TileHeight;
        //}
        //else
        //{
        //    TileWidth = W / View.W;
        //    TileHeight = H / View.H;
        //}
    }

    public void Clear()
    {
        //dispose?
        _graphics.Clear();
        _updateables.Clear();
    }

    public void Add(IRenderable graphic)
    {
        _graphics.Add(graphic);
    }

    public void Add(IUpdateable updateable)
    {
        _updateables.Add(updateable);
    }

    public void Add(IEntity entity)
    {
        _graphics.Add(entity);
        _updateables.Add(entity);
    }

    public void Remove(IRenderable graphic)
    {
        _graphics.Remove(graphic);
    }

    public void Remove(IUpdateable updateable)
    {
        _updateables.Remove(updateable);
    }

    public void Remove(IEntity entity)
    {
        _graphics.Remove(entity);
        _updateables.Remove(entity);
    }

    public void Dispose()
    {

    }
}

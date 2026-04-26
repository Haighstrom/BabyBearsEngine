using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Buffers;
using BabyBearsEngine.Worlds.Cameras;

namespace BabyBearsEngine.Worlds;

public sealed class Camera : ContainerEntity
{
    private readonly VertexDataBuffer<Vertex> _vertexBuffer = new();
    private readonly CameraMSAAShader _mSAAShader;
    private readonly FBO _shaderPassFBO;
    private readonly MsaaFBO? _msaaFBO;
    private readonly StandardMatrixShaderProgram _shader;
    private float _tileWidth, _tileHeight;
    private float _viewX, _viewY, _viewW, _viewH;
    private float _width, _height;
    private bool _verticesChanged;

    private Camera(float x, float y, float width, float height, MsaaSamples samples = MsaaSamples.Disabled)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        MSAASamples = samples;

        _shader = new();
        _mSAAShader = new CameraMSAAShader(samples);

        var colour = Colour.White.ToOpenTK();

        Vertices = [
            new (0, 0, colour, 0, 0), // bottom left
            new (0 + width, 0, colour, 1, 0), // bottom right
            new (0, 0 + height, colour, 0, 1), // top left
            new (0 + width, 0 + height, colour, 1, 1), // top right
        ];

        _vertexBuffer.SetNewVertices(Vertices);
        _verticesChanged = false;

        if (width <= 0 || height <= 0)
        {
            throw new Exception();
        }

        if (samples != MsaaSamples.Disabled)
        {
            _msaaFBO = new((int)width, (int)height, (int)samples);
        }

        _shaderPassFBO = new((int)width, (int)height);

        //Check for OpenGL errors
        var err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Logger.Log($"OpenGL error! (Camera.SetUpFBOTex) {err}");
        }
    }

    public Camera(float x, float y, float width, float height, float tileW, float tileH, MsaaSamples samples = MsaaSamples.Disabled)
        : this(x, y, width, height, samples)
    {
        FixedTileSize = true;

        //View set automatically - except it isnt?
        TileWidth = tileW;
        TileHeight = tileH;
    }

    public Camera(float x, float y, float width, float height, float viewX, float viewY, float viewW, float viewH, MsaaSamples samples = MsaaSamples.Disabled)
        : this(x, y, width, height, samples)
    {
        FixedTileSize = false;

        //TileWidth/TileHeight set automatically (are they?)
        _viewX = viewX;
        _viewY = viewY;
        _viewW = viewW;
        _viewH = viewH;
    }

    public float X { get; set; }
    public float Y { get; set; }

    public float Width
    {
        get => _width;
        set { _width = value; _verticesChanged = true; }
    }

    public float Height
    {
        get => _height;
        set { _height = value; _verticesChanged = true; }
    }

    private Vertex[] Vertices { get; set; }

    public float GameSpeed { get; set; } = 1;

    public Colour BackgroundColour { get; set; } = Colour.White;

    public bool FixedTileSize { get; set; }

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

    public (float X, float Y, float Width, float Height) View
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

    public override (float x, float y) GetWindowCoordinates(float x, float y)
    {
        float sx = X + (x - View.X) * TileWidth;
        float sy = Y + (y - View.Y) * TileHeight;
        return Parent?.GetWindowCoordinates(sx, sy) ?? (sx, sy);
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (_verticesChanged)
        {
            var c = Colour.White.ToOpenTK();
            Vertices = [
                new (0,     0,       c, 0, 0),
                new (Width, 0,       c, 1, 0),
                new (0,     Height,  c, 0, 1),
                new (Width, Height,  c, 1, 1),
            ];
            _vertexBuffer.SetNewVertices(Vertices);
            _verticesChanged = false;
        }

        bool msaaEnabled = MSAASamples != MsaaSamples.Disabled;

        _vertexBuffer.Bind();

        //Locally save the current render target, we will then set this camera as the current render target for child cameras, then put it back
        int previousFBO = OpenGLHelper.LastBoundFBO;

        if (msaaEnabled)
        {
            //Bind MSAA FBO to be the draw destination
            _msaaFBO!.Bind();
            //_tempSecondFBO.Bind();
        }
        else
        {
            //Bind Shader Pass FBO to be the draw destination
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

        var fboOrtho = Matrix3.CreateFBOOrtho(Width, Height);
        var identity = Matrix3.Identity;
        var mv = Matrix3.ScaleAroundOrigin(ref identity, TileWidth, TileHeight);
        mv = Matrix3.Translate(ref mv, -View.X, -View.Y);

        //draw stuff here 
        foreach (var graphic in GetRenderables())
        {
            if (!graphic.Visible)
            {
                continue;
            }

            graphic.Render(ref fboOrtho, ref mv);
        }

        _vertexBuffer.Bind();

        if (msaaEnabled)
        {
            //Bind the 2nd pass FBO and draw from the first to do MSAA sampling
            _shaderPassFBO.Bind();
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Bind the FBO to be drawn
            _msaaFBO!.BindTexture();
            //_tempSecondFBO.Texture.Bind();

            //Do the MSAA render pass, drawing to the MSAATexture FBO
            _mSAAShader.Bind();
            _mSAAShader.SetProjectionMatrix(ref fboOrtho);
            _mSAAShader.SetModelViewMatrix(ref identity);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);

            OpenGLHelper.UnbindTexture(TextureTarget.Texture2DMultisample);
        }

        //Revert the render target 
        OpenGLHelper.BindFBO(previousFBO);

        //Bind the FBO to be drawn
        _shaderPassFBO.Texture.Bind();

        //Set some other blend fucntion when render the FBO texture which apparantly lets the layer alpha blend with the one beneath?
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        //reset viewport
        GL.Viewport(prevVP.X, prevVP.Y, prevVP.Width, prevVP.Height);

        var mv2 = modelView;
        mv2 = Matrix3.Translate(ref mv2, X, Y);

        _shader.Bind();
        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref mv2);

        //var texture = new TextureFactory().CreateTextureFromImageFile("Assets/fish.jpg");
        //texture.Bind();
        //Render with assigned shader
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);

        //Check for OpenGL errors
        var err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Logger.Log($"OpenGL error! (Camera.SetUpFBOTex) {err}");
        }
    }

}

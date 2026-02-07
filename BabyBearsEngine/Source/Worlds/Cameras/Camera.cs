//using BabyBearsEngine.OpenGL;
//using BabyBearsEngine.Source.Diagnostics;
//using BabyBearsEngine.Source.Platform.OpenGL.Buffers;
//using BabyBearsEngine.Source.Worlds;
//using BabyBearsEngine.Source.Worlds.Cameras;

//namespace BabyBearsEngine.Worlds;

//public class Camera : IEntity
//{
//    private int _frameBufferMSAAID;
//    private Texture _frameBufferMSAATexture;
//    private FBO _shaderPassFBO;
//    private readonly StandardMatrixShaderProgram _shader;
//    private readonly CameraMSAAShader _mSAAShader;
//    private Matrix3 _ortho;
//    private float _tileWidth, _tileHeight;
//    private float _viewX, _viewY, _viewW, _viewH;

//    private Camera(float layer, float x, float y, float width, float height)
//    {
//        _shader = new();
//        _mSAAShader = new CameraMSAAShader() { Samples = MSAASamples };

//        VertexBuffer = GL.GenBuffer();

//        var colour = Colour.White.ToOpenTK();
//        Vertices =
//        [
//            new Vertex(0, 0, colour, 0f, 0f),
//            new Vertex(width, 0, colour, 1f,  0f),
//            new Vertex(0, height, colour, 0f,  1f),
//            new Vertex(width, height, colour, 1f,  1f)
//        ];

//        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
//        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Vertex.Stride, Vertices, BufferUsageHint.DynamicDraw);
//        //OpenGLHelper.LastBoundVertexBuffer = VertexBuffer;

//        SetUpFBOTex((int)width, (int)height);

//        _ortho = Matrix3.CreateFBOOrtho(width, height);
//    }

//    public Camera(float layer, float x, float y, float width, float height, float tileW, float tileH)
//        : this(layer, x, y, width, height)
//    {
//        FixedTileSize = true;

//        //View set automatically - except it isnt?
//        TileWidth = tileW;
//        TileHeight = tileH;

//    }

//    public Camera(float layer, float x, float y, float width, float height, float viewX, float viewY, float viewW, float viewH)
//        : this(layer, x, y, width, height)
//    {
//        FixedTileSize = false;

//        //TileWidth/TileHeight set automatically
//        _viewX = viewX;
//        _viewY = viewY;
//        _viewW = viewW;
//        _viewH = viewH;
//    }

//    private float X { get; set; }
//    private float Y { get; set; }
//    private float Width { get; set; }
//    private float Height { get; set; }

//    private bool MSAAEnabled { get => MSAASamples != MsaaSamples.Disabled; }

//    private int VertexBuffer { get; set; }

//    private Vertex[] Vertices { get; set; }

//    public float GameSpeed { get; set; } = 1;

//    /// <summary>
//    /// Angle in Degrees
//    /// </summary>
//    public float Angle { get; set; }

//    public Colour BackgroundColour { get; set; } = Colour.White;

//    public bool FixedTileSize { get; set; }

//    public float MaxX { get; set; }

//    public float MaxY { get; set; }

//    public float MinX { get; set; }

//    public float MinY { get; set; }

//    public MsaaSamples MSAASamples { get; set; } = MsaaSamples.Disabled; //todo: trigger resize if this changes?

//    public float TileHeight
//    {
//        get => _tileHeight;
//        set
//        {
//            _tileHeight = value;

//            if (FixedTileSize)
//                _viewH = Height / TileHeight;
//        }
//    }

//    public float TileWidth
//    {
//        get => _tileWidth;
//        set
//        {
//            _tileWidth = value;

//            if (FixedTileSize)
//                _viewW = Width / value;
//        }
//    }

//    public virtual (float X, float Y, float Width, float Height) View
//    {
//        get => (_viewX, _viewY, _viewW, _viewH);
//        set
//        {
//            if (!FixedTileSize)
//            {
//                TileWidth = Width / value.Width;
//                TileHeight = Height / value.Height;
//            }

//            _viewX = value.X; 
//            _viewY = value.Y;
//            _viewW = value.Width; 
//            _viewH = value.Height;

//            ViewChanged?.Invoke(this, EventArgs.Empty);
//        }
//    }

//    public event EventHandler? ViewChanged;

//    /// <summary>
//    /// Initialise the textures that the Frame Buffet Object render target the camera draws to uses and the intermediate Multisample texture used in addition if MultiSample AntiAliasing is enabled.
//    /// </summary>
//    /// <param name="width"></param>
//    /// <param name="height"></param>
//    private void SetUpFBOTex(int width, int height)
//    {
//        if (width <= 0 || height <= 0)
//            return;

//        if (MSAASamples != MsaaSamples.Disabled)
//        {
//            OpenGLHelper.CreateMSAAFramebuffer(width, height, MSAASamples, out _frameBufferMSAAID, out _frameBufferMSAATexture);
//        }

//        _shaderPassFBO = new(width, height);

//        //Check for OpenGL errors
//        var err = GL.GetError();
//        if (err != ErrorCode.NoError)
//        {
//            Logger.Log($"OpenGL error! (Camera.SetUpFBOTex) {err}");
//        }
//    }

//    public void Update(double elapsed)
//    {
//    }

//    public void Render()
//    {
//        //Bind vertex buffer - optimise this later            
//        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
//        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Vertex.Stride, Vertices, BufferUsageHint.DynamicDraw);
//        OpenGLHelper.LastBoundVertexBuffer = VertexBuffer;

//        if (MSAAEnabled)
//        {
//            //Bind MSAA FBO to be the draw destination and clear it
//            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferMSAAID);
//            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, _frameBufferMSAATexture.Handle, 0);
//        }
//        else
//        {
//            //Bind Shader Pass FBO to be the draw destination and clear it
//            _shaderPassFBO.Bind();
//        }

//        //Clear the FBO
//        GL.ClearColor(BackgroundColour.R / 255f, BackgroundColour.G / 255f, BackgroundColour.B / 255f, BackgroundColour.A / 255f);
//        GL.Clear(ClearBufferMask.ColorBufferBit);

//        //Set normal blend function for within the layers
//        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

//        //Save the previous viewport and set the viewport to match the size of the texture we are now drawing to - the FBO
//        Rect prevVP = OpenGLHelper.GetViewport();
//        GL.Viewport(0, 0, (int)base.W, (int)base.H);

//        //Locally save the current render target, we will then set this camera as the current render target for child cameras, then put it back
//        int tempFBID = OpenGLHelper.LastBoundFBO;
//        OpenGLHelper.LastBoundFBO = MSAAEnabled ? _frameBufferMSAAID : _frameBufferShaderPassID;

//        Matrix3 identity = Matrix3.Identity;
//        Matrix3 MV = Matrix3.ScaleAroundOrigin(ref identity, TileWidth, TileHeight);
//        MV = Matrix3.Translate(ref MV, -View.X, -View.Y);

//        //draw stuff here 

//        base.Render(ref _ortho, ref MV);

//        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);//this MUST be called after base render

//        //Revert the render target 
//        OpenGLHelper.LastBoundFBO = tempFBID;

//        if (MSAAEnabled)
//        {
//            //Bind the 2nd pass FBO and draw from the first to do MSAA sampling
//            _shaderPassFBO.Bind();
//            GL.ClearColor(0, 0, 0, 0);
//            GL.Clear(ClearBufferMask.ColorBufferBit);

//            //Bind the FBO to be drawn
//            GL.BindTexture(TextureTarget.Texture2DMultisample, _frameBufferMSAATexture.Handle);

//            //Do the MSAA render pass, drawing to the MSAATexture FBO
//            _mSAAShader.Render(ref _ortho, ref identity, Vertices.Length, PrimitiveType.TriangleStrip);

//            //Unbind the FBO 
//            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
//        }

//        //Bind the render target back to either the screen, or a camera higher up the heirachy, depending on what called this
//        GL.BindFramebuffer(FramebufferTarget.Framebuffer, OpenGLHelper.LastBoundFBO);

//        //Bind the FBO to be drawn
//        _shaderPassFBO.Texture.Bind();

//        //Set some other blend fucntion when render the FBO texture which apparantly lets the layer alpha blend with the one beneath?
//        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

//        //reset viewport
//        OpenGLHelper.Viewport(prevVP);

//        Matrix3 mv = modelView;
//        if (Angle != 0)
//            mv = Matrix3.RotateAroundPoint(ref mv, Angle, R.Centre.X, R.Centre.Y);
//        mv = Matrix3.Translate(ref mv, X, Y);

//        //Render with assigned shader
//        _shader.Render(ref projection, ref mv, Vertices.Length, PRIMITIVE_TYPE.GL_TRIANGLE_STRIP);

//        OpenGLHelper.UnbindTexture();
//    }

//    public void Resize(float newW, float newH)
//    {
//        throw new NotImplementedException();
//        //W = newW;
//        //H = newH;

//        ////if (MSAASamples != MSAA_Samples.Disabled)
//        ////    HF.Graphics.ResizeMSAAFramebuffer(_frameBufferMSAAID, ref _frameBufferMSAATexture, W, H, MSAASamples);

//        ////HF.Graphics.ResizeFramebuffer(_frameBufferShaderPassID, ref _frameBufferShaderPassTexture, W, H);

//        //var _colour = Colour.White.ToOpenTK();
//        //Vertices =
//        //[
//        //    new Vertex(0, 0, _colour, 0f, 0f),
//        //    new Vertex(Width, 0, _colour, 1f,  0f),
//        //    new Vertex(0, Height, _colour, 0f,  1f),
//        //    new Vertex(Width, Height, _colour, 1f,  1f)
//        //];

//        //SetUpFBOTex((int)Width, (int)Height);

//        //_ortho = Matrix3.CreateFBOOrtho(Width, Height);

//        //if (FixedTileSize)
//        //{
//        //    View.W = Width / TileWidth;
//        //    View.H = Height / TileHeight;
//        //}
//        //else
//        //{
//        //    TileWidth = W / View.W;
//        //    TileHeight = H / View.H;
//        //}
//    }

//    public void Dispose()
//    {

//    }
//}

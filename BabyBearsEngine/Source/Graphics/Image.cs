using System.IO;
using System.Reflection.Metadata;
using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics;

public class Image : IGraphic, IDisposable
{
    private readonly StandardMatrixShaderProgram _shaderProgram;
    private readonly VAO _vAO;
    private readonly VBO _vBO;
    private readonly Texture _texture;

    private float _x, _y, _width, _height;
    private Color4 _colour = Color4.White;
    private bool _verticesChanged = true;

    public Image(GameWindow window, string texturePath, float x, float y, float width, float height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;

        _shaderProgram = new StandardMatrixShaderProgram(window);

        //Create and bind the VAO (which will store the VBO binding)
        _vAO = new VAO();
        _vAO.Bind();

        //Create and bind the VBO (which contains the objects vertices)
        _vBO = new VBO(BufferUsageHint.StaticDraw);
        _vBO.Bind();

        // Define attributes (this links the VBO to the VAO)
        _vAO.SetVertexFormats();

        _vAO.Unbind();

        _texture = new Texture(texturePath);
    }

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            _verticesChanged = true;
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            _verticesChanged = true;
        }
    }

    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            _verticesChanged = true;
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            _height = value; 
            _verticesChanged = true;
        }
    }

    public Color4 Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    private void UpdateVertices()
    {
        _vBO.Bind();

        Vertex[] vertices =
        [
            new(_x + _width, _y + _height, Colour, 1, 1), // top right
            new(_x + _width, _y, Colour, 1, 0), // bottom right
            new(_x, _y + _height, Colour, 0, 1), // top left
            new(_x, _y, Colour, 0, 0), // bottom left
        ];

        _vBO.BufferData(vertices);
    }

    public void Draw()
    {
        _shaderProgram.Bind();
        _vAO.Bind();
        _texture.Bind();

        if (_verticesChanged)
        {
            UpdateVertices();

            _verticesChanged = false;
        }

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    #region IDisposable
    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _shaderProgram.Dispose(); //todo: don't dispose shader here - it's a shared resource
                _vAO.Dispose();
                _vBO.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Image()
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
    #endregion
}

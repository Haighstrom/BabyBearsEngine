using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Graphics.Components;

namespace BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Chapter1;
internal class Triangle : IRenderable
{
    private bool _disposed;
    private int _vao;

    public Triangle()
    {
        BasicShaderProgram shader = new();
        GL.UseProgram(shader.Handle);

        float[] positionData =
        [
            -0.8f, -0.8f, 0,
            0.8f, -0.8f, 0,
            0f, 0.8f, 0
        ];
        float[] colourData =
        [
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        ];

        var positionVBO = GL.GenBuffer();
        var colourVBO = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, 9 * sizeof(float), positionData, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ArrayBuffer, colourVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, 9 * sizeof(float), colourData, BufferUsageHint.StaticDraw);

        _vao = GL.GenVertexArray();

        GL.BindVertexArray(_vao);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, colourVBO);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
    }

    public void Draw()
    {
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(_vao);

            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Triangle()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

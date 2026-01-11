using System.Collections.Generic;
using BabyBearsEngine.Source.Graphics.Components;
using BabyBearsEngine.Source.Graphics.Shaders.ShaderPrograms;
using BabyBearsEngine.Source.Graphics.Textures;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BabyBearsEngine.Source.Graphics.MemoryManagement;

internal class DefaultGPUMemoryService : IGPUMemoryService
{
    private readonly object _gate = new();

    private HashSet<int> _shaderProgramHandlesToDelete = [];
    private HashSet<int> _textureHandlesToDelete = [];
    private HashSet<int> _vaoHandlesToDelete = [];
    private HashSet<int> _vboHandlesToDelete = [];
    private HashSet<int> _eboHandlesToDelete = [];

    public void RequestDeleteShader(IShaderProgram shader)
    {
        ArgumentNullException.ThrowIfNull(shader);

        var handle = shader.Handle;
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _shaderProgramHandlesToDelete.Add(handle);
        }
    }

    public void RequestDeleteTexture(ITexture texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var handle = texture.Handle;

        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _textureHandlesToDelete.Add(handle);
        }
    }

    public void RequestDeleteVAO(VAO vao)
    {
        ArgumentNullException.ThrowIfNull(vao);

        var handle = vao.Handle;
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _vaoHandlesToDelete.Add(handle);
        }
    }

    public void RequestDeleteVBO(VBO vbo)
    {
        ArgumentNullException.ThrowIfNull(vbo);

        var handle = vbo.Handle;
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _vboHandlesToDelete.Add(handle);
        }
    }

    public void RequestDeleteEBO(EBO ebo)
    {
        ArgumentNullException.ThrowIfNull(ebo);

        var handle = ebo.Handle;
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _eboHandlesToDelete.Add(handle);
        }
    }

    public void ProcessDeletes()
    {
        // Swap work out under lock, then delete outside the lock.
        HashSet<int> shaders;
        HashSet<int> textures;
        HashSet<int> vaos;
        HashSet<int> vbos;
        HashSet<int> ebos;

        lock (_gate)
        {
            shaders = _shaderProgramHandlesToDelete; 
            _shaderProgramHandlesToDelete = [];

            textures = _textureHandlesToDelete; 
            _textureHandlesToDelete = [];

            vaos = _vaoHandlesToDelete; 
            _vaoHandlesToDelete = [];

            vbos = _vboHandlesToDelete; 
            _vboHandlesToDelete = [];

            ebos = _eboHandlesToDelete; 
            _eboHandlesToDelete = [];
        }

        if (shaders.Count != 0)
        {
            OpenGLHelper.UnbindShader();
            foreach (var handle in shaders)
            {
                GL.DeleteProgram(handle);
            }
        }

        if (textures.Count != 0)
        {
            OpenGLHelper.UnbindTexture();
            foreach (var handle in textures)
            {
                GL.DeleteTexture(handle);
            }
        }

        if (vaos.Count != 0)
        {
            OpenGLHelper.UnbindVAO();
            foreach (var handle in vaos)
            {
                GL.DeleteVertexArray(handle);
            }
        }

        if (vbos.Count != 0)
        {
            OpenGLHelper.UnbindVBO();
            foreach (var handle in vbos)
            {
                GL.DeleteBuffer(handle);
            }
        }

        if (ebos.Count != 0)
        {
            OpenGLHelper.UnbindEBO();
            foreach (var handle in ebos)
            {
                GL.DeleteBuffer(handle);
            }
        }
    }
}

using System.Collections.Generic;
using System.Threading;

namespace BabyBearsEngine.OpenGL;

internal class DefaultGPUResourceDeletionService : IGPUResourceDeletionService
{
    private readonly Lock _gate = new();

    private HashSet<int> _shaderProgramsToDelete = [];
    private HashSet<int> _texturesToDelete = [];
    private HashSet<int> _vertexArraysToDelete = [];
    private HashSet<int> _vBOsToDelete = [];
    private HashSet<int> _eBOsToDelete = [];

    public void QueueShaderDelete(int handle)
    {
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _shaderProgramsToDelete.Add(handle);
        }
    }

    public void QueueTextureDelete(int handle)
    {
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _texturesToDelete.Add(handle);
        }
    }

    public void QueueVertexArrayDelete(int handle)
    {
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _vertexArraysToDelete.Add(handle);
        }
    }

    public void QueueVBODelete(int handle)
    {
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _vBOsToDelete.Add(handle);
        }
    }

    public void QueueEBODelete(int handle)
    {
        if (handle == 0)
        {
            return;
        }

        lock (_gate)
        {
            _eBOsToDelete.Add(handle);
        }
    }

    public void ProcessDeletes()
    {
        // Swap work out under lock, then delete outside the lock.
        HashSet<int> shaders;
        HashSet<int> textures;
        HashSet<int> vertexArrays;
        HashSet<int> vbos;
        HashSet<int> ebos;

        lock (_gate)
        {
            shaders = _shaderProgramsToDelete; 
            _shaderProgramsToDelete = [];

            textures = _texturesToDelete;
            _texturesToDelete = [];

            vertexArrays = _vertexArraysToDelete;
            _vertexArraysToDelete = [];

            vbos = _vBOsToDelete;
            _vBOsToDelete = [];

            ebos = _eBOsToDelete;
            _eBOsToDelete = [];
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

        if (vertexArrays.Count != 0)
        {
            OpenGLHelper.UnbindVertexArray();
            foreach (var handle in vertexArrays)
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

namespace BabyBearsEngine.OpenGL;

public interface IGPUResourceDeletionService
{
    void QueueShaderDelete(int handle);
    void QueueTextureDelete(int handle);
    void QueueVertexArrayDelete(int handle);
    void QueueVBODelete(int handle);
    void QueueEBODelete(int handle);
    void ProcessDeletes();
}

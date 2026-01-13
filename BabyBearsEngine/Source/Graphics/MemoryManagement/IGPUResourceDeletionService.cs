namespace BabyBearsEngine.Source.Graphics.MemoryManagement;

internal interface IGPUResourceDeletionService
{
    void QueueShaderDelete(int handle);
    void QueueTextureDelete(int handle);
    void QueueVertexArrayDelete(int handle);
    void QueueVBODelete(int handle);
    void QueueEBODelete(int handle);
    void ProcessDeletes();
}

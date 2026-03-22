using BabyBearsEngine.Source.Runtime;

namespace BabyBearsEngine.OpenGL;

internal static class GPUResourceDeletion
{
    private static IGPUResourceDeletionService Implementation => EngineConfiguration.GPUResourceDeletionService;

    private static bool TryRequestDelete(Action<IGPUResourceDeletionService, int> enqueueDelete, int handle)
    {
        if (handle == 0)
        {
            return true;
        }

        enqueueDelete(Implementation, handle);
        return true;
    }

    public static bool TryRequestDeleteShader(int handle) => TryRequestDelete(static (service, h) => service.QueueShaderDelete(h), handle);

    public static bool TryRequestDeleteTexture(int handle) => TryRequestDelete(static (service, h) => service.QueueTextureDelete(h), handle);

    public static bool TryRequestDeleteVAO(int handle) => TryRequestDelete(static (service, h) => service.QueueVertexArrayDelete(h), handle);

    public static bool TryRequestDeleteVBO(int handle) => TryRequestDelete(static (service, h) => service.QueueVBODelete(h), handle);

    public static bool TryRequestDeleteFBO(int handle) => false; //todo: invent this

    public static bool TryRequestDeleteEBO(int handle) => TryRequestDelete(static (service, h) => service.QueueEBODelete(h), handle);

    public static void ProcessDeletes()
    {
        Implementation.ProcessDeletes();
    }
}

namespace BabyBearsEngine.Source.Platform;

public interface IRenderHost
{
    void Initialise();

    void BeginFrame();

    void EndFrame();

    void HandleScreenResize(int width, int height);
}

namespace BabyBearsEngine.PowerUsers;

public interface IGameLoop
{
    void Load();
    void Unload();
    void Update(double deltaSeconds);
    void Render(double deltaSeconds);
    void HandleScreenResize(int width, int height);
}

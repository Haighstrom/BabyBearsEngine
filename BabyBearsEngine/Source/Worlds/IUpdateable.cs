namespace BabyBearsEngine.Worlds;

public interface IUpdateable
{
    bool Active { get; set; }
    void Update(double elapsed);
}

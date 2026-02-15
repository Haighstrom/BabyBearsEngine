using System.Drawing;
namespace BabyBearsEngine.Worlds;

public interface IWorld : IContainer
{
    Colour BackgroundColour { get; }
    void Load();
    void Unload();
    void Update(double elapsed);
    void Draw();
}

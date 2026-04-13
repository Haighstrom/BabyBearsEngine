namespace BabyBearsEngine.Worlds;

public interface IAddable
{
    IContainer? Parent { get; }
    bool Exists { get; }
    void SetParent(IContainer? parent);
    void Remove();
}

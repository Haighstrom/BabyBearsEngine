namespace BabyBearsEngine.Worlds;

public abstract class AddableBase : IAddable
{
    public IContainer? Parent { get; private set; }

    public bool Exists => Parent is not null;

    public void SetParent(IContainer? container)
    {
        //only allow flipping between having a parent and not having a parent, not switching from one parent to another without first removing from the first
        if (container is null)
        {
            Ensure.NotNull(Parent);
        }
        else
        {
            Ensure.IsNull(Parent);
        }

        Parent = container;
    }

    public void Remove()
    {
        Ensure.NotNull(Parent);

        Parent.Remove(this);
    }
}

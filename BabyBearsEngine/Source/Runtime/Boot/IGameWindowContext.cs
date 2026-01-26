namespace BabyBearsEngine.Source.Runtime.Boot;

public interface IGameWindowContext : IDisposable
{
    object NativeWindow { get; }
}

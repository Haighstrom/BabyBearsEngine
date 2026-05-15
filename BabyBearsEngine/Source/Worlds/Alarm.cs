namespace BabyBearsEngine.Worlds;

/// <summary>
/// A timer that fires <see cref="Elapsed"/> after a set duration, then either removes itself
/// (one-shot) or resets and fires again (repeating).
/// </summary>
public class Alarm(double duration, bool repeating = false) : AddableBase, IUpdateable
{
    private double _elapsed = 0.0;

    public Alarm(double duration, Action onElapsed, bool repeating = false)
        : this(duration, repeating)
    {
        Elapsed += onElapsed;
    }

    public bool Active { get; set; } = true;

    public event Action? Elapsed;

    public void Update(double elapsed)
    {
        if (!Active)
        {
            return;
        }

        _elapsed += elapsed;

        if (_elapsed < duration)
        {
            return;
        }

        _elapsed -= duration;

        Elapsed?.Invoke();

        if (!repeating)
        {
            Remove();
        }
    }
}

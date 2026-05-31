namespace BabyBearsEngine.Tasks;

/// <summary>
/// A task that waits for a fixed duration then signals completion.
/// </summary>
public class WaitTask(double duration) : Task
{
    private double _elapsed = 0.0;

    /// <inheritdoc/>
    public override bool IsComplete => _elapsed >= duration;

    /// <inheritdoc/>
    protected override void OnReset()
    {
        _elapsed = 0.0;
    }

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        // Increment before base.Update: base checks IsComplete to fire completion this frame,
        // so _elapsed must reflect the current tick first or a single-frame wait takes two updates.
        _elapsed += elapsed;
        base.Update(elapsed);
    }
}

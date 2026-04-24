namespace BabyBearsEngine.Source.Worlds;

/// <summary>
/// Controller interface for mouse interactions with a clickable target.
///
/// <para>Implementations accept per-frame mouse-over information via
/// <see cref="SetMouseOver(bool)"/> and advance internal state via
/// <see cref="Update(double)"/>. The expected semantics follow the
/// behaviour of <c>ClickController</c>:</para>
/// <para>
/// - <see cref="SetMouseOver(bool)"/> must be called each frame before
///   <see cref="Update(double)"/> so the controller sees the current
///   mouse-over state.
/// - <see cref="Update(double)"/> advances the controller by the specified
///   elapsed time in seconds.
/// - The controller assumes mouse input exposes edge events (e.g.
///   <c>Mouse.LeftPressed</c> and <c>Mouse.LeftReleased</c> are true only on
///   the frame the action occurred).
/// - A <c>LeftReleased</c> should be forwarded to the target only when the
///   mouse was pressed while over the target and then released while still
///   over the target (a successful click). If the mouse is pressed over the
///   target, dragged outside while holding the button, and released outside,
///   the interaction is cancelled (for example the controller may emit a
///   mouse-exit/cancel event instead of a release).
/// </para>
/// </summary>
internal interface IClickController
{
    /// <summary>
    /// Inform the controller whether the mouse is currently over the target
    /// for this frame. This must be called before <see cref="Update(double)"/>.
    /// </summary>
    /// <param name="isMouseOver">True if the mouse is over the target this frame.</param>
    void SetMouseOver(bool isMouseOver);
}

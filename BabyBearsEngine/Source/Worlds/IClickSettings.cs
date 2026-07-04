namespace BabyBearsEngine.Worlds;

/// <summary>
/// The mouse-interaction settings for a clickable <see cref="Entity"/>. Obtained via
/// <see cref="Entity.ClickSettings"/>, which throws <see cref="System.InvalidOperationException"/>
/// unless the entity was constructed with <c>clickable: true</c> — so misuse on a non-clickable
/// entity fails loudly at the call site instead of silently no-opping.
/// </summary>
public interface IClickSettings
{
    /// <summary>
    /// When true, <see cref="Worlds.MouseSolver"/> continues propagating mouse-over state
    /// through this entity to overlapping clickable entities beneath it rather than stopping
    /// here.
    /// </summary>
    bool ClickThrough { get; set; }

    /// <summary>
    /// When true, a double-click also fires <see cref="Entity.LeftClicked"/> for the second
    /// click in addition to <see cref="Entity.LeftDoubleClicked"/>. Default is true.
    /// </summary>
    bool DoubleClickTriggersSingleClick { get; set; }

    /// <summary>
    /// Maximum time in seconds between two left-clicks for them to count as a double-click.
    /// Default is 0.5 seconds.
    /// </summary>
    double DoubleClickWindow { get; set; }

    /// <summary>
    /// Seconds the cursor must rest over this entity before <see cref="Entity.MouseHovered"/>
    /// fires. Default is 0.5. Set to 0 to fire immediately on mouse enter.
    /// </summary>
    double HoverDelay { get; set; }

    /// <summary>
    /// When true and the mouse is over this entity, scroll wheel movement fires
    /// <see cref="Entity.MouseScrolled"/> and sets <see cref="Worlds.MouseSolver.WheelScrollConsumed"/>,
    /// preventing world-level scroll handlers from also reacting to the wheel that frame.
    /// </summary>
    bool InterceptsMouseScroll { get; set; }
}

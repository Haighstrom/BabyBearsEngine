namespace BabyBearsEngine.Input;

/// <summary>
/// Identifies a mouse button. Mirrors OpenTK's <c>MouseButton</c> with the same integer values.
/// <see cref="Left"/>, <see cref="Right"/>, and <see cref="Middle"/> are aliases for the first three buttons;
/// <see cref="Button4"/>–<see cref="Button8"/> cover extra buttons such as side/thumb buttons.
/// </summary>
public enum MouseButton
{
    /// <summary>First mouse button. Aliased as <see cref="Left"/>.</summary>
    Button1 = 0,
    /// <summary>Second mouse button. Aliased as <see cref="Right"/>.</summary>
    Button2 = 1,
    /// <summary>Third mouse button. Aliased as <see cref="Middle"/>.</summary>
    Button3 = 2,
    /// <summary>Fourth mouse button (commonly a side / back button).</summary>
    Button4 = 3,
    /// <summary>Fifth mouse button (commonly a side / forward button).</summary>
    Button5 = 4,
    /// <summary>Sixth mouse button.</summary>
    Button6 = 5,
    /// <summary>Seventh mouse button.</summary>
    Button7 = 6,
    /// <summary>Eighth mouse button.</summary>
    Button8 = 7,
    /// <summary>Highest-numbered button supported (alias for <see cref="Button8"/>).</summary>
    Last = Button8,
    /// <summary>Left mouse button (alias for <see cref="Button1"/>).</summary>
    Left = Button1,
    /// <summary>Right mouse button (alias for <see cref="Button2"/>).</summary>
    Right = Button2,
    /// <summary>Middle mouse button (alias for <see cref="Button3"/>).</summary>
    Middle = Button3,
}

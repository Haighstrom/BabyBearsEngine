namespace BabyBearsEngine.Worlds.UI;

/// <summary>The unit a <see cref="ScrollStep"/>'s amount is measured in.</summary>
public enum ScrollStepUnit
{
    /// <summary>A fraction of the whole scroll range, where 1 is the full distance from top to bottom.</summary>
    Fraction,

    /// <summary>A fixed distance in pixels of content, however long the content is.</summary>
    Pixels,
}

/// <summary>
/// How far one scroll input (such as a mouse wheel notch) moves scrollable content: an amount
/// and the unit it is measured in. Build one with <see cref="Fraction"/> or <see cref="Pixels"/>.
/// </summary>
public readonly record struct ScrollStep
{
    private ScrollStep(float amount, ScrollStepUnit unit)
    {
        Amount = amount;
        Unit = unit;
    }

    /// <summary>The size of the step, in <see cref="Unit"/>.</summary>
    public float Amount { get; }

    /// <summary>What <see cref="Amount"/> is measured in.</summary>
    public ScrollStepUnit Unit { get; }

    /// <summary>
    /// A step that is a fraction of the whole scroll range (0.1 = 10%), so one step moves further
    /// the longer the content is.
    /// </summary>
    public static ScrollStep Fraction(float amount) => new(amount, ScrollStepUnit.Fraction);

    /// <summary>A step of a fixed number of content pixels, the same however long the content is.</summary>
    public static ScrollStep Pixels(float amount) => new(amount, ScrollStepUnit.Pixels);
}

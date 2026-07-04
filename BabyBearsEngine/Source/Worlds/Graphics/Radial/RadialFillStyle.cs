namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>How a <see cref="RadialProgressBar"/>'s fill sweeps outward from the centre.</summary>
public enum RadialFillStyle
{
    /// <summary>A filled sector growing from the centre out to the full radius, like a pie chart wedge.</summary>
    Pie,

    /// <summary>A filled annulus band of configurable thickness, leaving the centre hollow.</summary>
    Ring,
}

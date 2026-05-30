namespace BabyBearsEngine.Worlds;

/// <summary>
/// Anchor point used by <see cref="DockingController"/> to position a target rect
/// relative to a host rect. Names describe the location on the <em>host</em> that the
/// target's matching corner/edge midpoint aligns to.
/// </summary>
public enum DockPosition
{
    /// <summary>Target's top-left at host's top-left.</summary>
    TopLeft,

    /// <summary>Target horizontally centred on host, top edge on host's top.</summary>
    TopCentre,

    /// <summary>Target's top-right at host's top-right.</summary>
    TopRight,

    /// <summary>Target vertically centred on host, left edge on host's left.</summary>
    CentreLeft,

    /// <summary>Target centred on host.</summary>
    Centre,

    /// <summary>Target vertically centred on host, right edge on host's right.</summary>
    CentreRight,

    /// <summary>Target's bottom-left at host's bottom-left.</summary>
    BottomLeft,

    /// <summary>Target horizontally centred on host, bottom edge on host's bottom.</summary>
    BottomCentre,

    /// <summary>Target's bottom-right at host's bottom-right.</summary>
    BottomRight,

    /// <summary>
    /// Target hangs directly below the host — horizontally centred, top edge on the host's
    /// bottom. Useful for nameplates, health bars and similar attached widgets.
    /// </summary>
    HangBelow,

    /// <summary>
    /// Target sits directly above the host — horizontally centred, bottom edge on the host's
    /// top. Useful for floating labels and tooltips.
    /// </summary>
    HangAbove,
}

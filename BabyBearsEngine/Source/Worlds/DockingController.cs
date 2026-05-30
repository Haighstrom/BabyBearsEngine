using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Positions an <see cref="IRect"/> target relative to a host rect each frame. The host is
/// resolved via a <see cref="Func{IRect}"/> so it can be a moving entity, a UI cell, or a
/// changing reference. Covers docking (attached badges/icons), hanging widgets (nameplates,
/// health bars), and simple follow patterns via the <see cref="DockPosition"/> enum and a
/// pixel <see cref="Shift"/> offset.
/// </summary>
public class DockingController : UpdateableBase
{
    private readonly IRect _target;
    private readonly Func<IRect> _dockTo;

    /// <param name="target">The rect whose position is driven each frame.</param>
    /// <param name="dockTo">Resolves the host rect to dock onto. Called every frame.</param>
    /// <param name="position">Anchor describing how <paramref name="target"/> aligns to the host.</param>
    /// <param name="shift">Optional pixel offset applied after anchoring (positive X is right; positive Y is down).</param>
    public DockingController(IRect target, Func<IRect> dockTo, DockPosition position, Point shift = default)
    {
        _target = target;
        _dockTo = dockTo;
        DockPosition = position;
        Shift = shift;

        UpdatePosition();
    }

    /// <summary>Anchor describing how the target aligns to the host. Hot-swappable.</summary>
    public DockPosition DockPosition { get; set; }

    /// <summary>Pixel offset applied after anchoring (positive X is right; positive Y is down).</summary>
    public Point Shift { get; set; }

    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        IRect host = _dockTo();

        switch (DockPosition)
        {
            case DockPosition.TopLeft:
                _target.X = host.X + Shift.X;
                _target.Y = host.Y + Shift.Y;
                break;

            case DockPosition.TopCentre:
                _target.X = host.Centre.X - _target.Width * 0.5f + Shift.X;
                _target.Y = host.Y + Shift.Y;
                break;

            case DockPosition.TopRight:
                _target.X = host.Right - _target.Width + Shift.X;
                _target.Y = host.Y + Shift.Y;
                break;

            case DockPosition.CentreLeft:
                _target.X = host.X + Shift.X;
                _target.Y = host.Centre.Y - _target.Height * 0.5f + Shift.Y;
                break;

            case DockPosition.Centre:
                _target.X = host.Centre.X - _target.Width * 0.5f + Shift.X;
                _target.Y = host.Centre.Y - _target.Height * 0.5f + Shift.Y;
                break;

            case DockPosition.CentreRight:
                _target.X = host.Right - _target.Width + Shift.X;
                _target.Y = host.Centre.Y - _target.Height * 0.5f + Shift.Y;
                break;

            case DockPosition.BottomLeft:
                _target.X = host.X + Shift.X;
                _target.Y = host.Bottom - _target.Height + Shift.Y;
                break;

            case DockPosition.BottomCentre:
                _target.X = host.Centre.X - _target.Width * 0.5f + Shift.X;
                _target.Y = host.Bottom - _target.Height + Shift.Y;
                break;

            case DockPosition.BottomRight:
                _target.X = host.Right - _target.Width + Shift.X;
                _target.Y = host.Bottom - _target.Height + Shift.Y;
                break;

            case DockPosition.HangBelow:
                _target.X = host.Centre.X - _target.Width * 0.5f + Shift.X;
                _target.Y = host.Bottom + Shift.Y;
                break;

            case DockPosition.HangAbove:
                _target.X = host.Centre.X - _target.Width * 0.5f + Shift.X;
                _target.Y = host.Y - _target.Height + Shift.Y;
                break;

            default:
                throw new InvalidOperationException($"Unhandled {nameof(DockPosition)}: {DockPosition}.");
        }
    }
}

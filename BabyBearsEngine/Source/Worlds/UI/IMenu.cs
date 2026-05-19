namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A widget that can be shown and hidden as part of a <see cref="IMenuGroup"/>.
/// </summary>
public interface IMenu : IAddable
{
    /// <summary>Whether this menu is currently open.</summary>
    bool IsOpen { get; }

    /// <summary>Shows this menu.</summary>
    void Open();

    /// <summary>Hides this menu.</summary>
    void Close();
}

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// Coordinates a set of <see cref="IMenu"/> instances so that at most one is open at a time.
/// </summary>
public interface IMenuGroup
{
    /// <summary>Adds <paramref name="menus"/> to the group.</summary>
    void Register(params IMenu[] menus);

    /// <summary>Closes every other menu in the group, then opens <paramref name="menu"/>.</summary>
    void Open(IMenu menu);

    /// <summary>Opens <paramref name="menu"/> if it is closed; closes it if it is already open.</summary>
    void Toggle(IMenu menu);

    /// <summary>Closes every menu in the group.</summary>
    void CloseAll();
}

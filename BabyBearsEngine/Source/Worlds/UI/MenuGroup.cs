namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// Coordinates a set of <see cref="IMenu"/> instances so that at most one is open at a time.
/// </summary>
public class MenuGroup : IMenuGroup
{
    private readonly List<IMenu> _menus = [];

    /// <inheritdoc/>
    public void Register(params IMenu[] menus)
    {
        foreach (var menu in menus)
        {
            if (!_menus.Contains(menu))
            {
                _menus.Add(menu);
            }
        }
    }

    /// <inheritdoc/>
    public void Open(IMenu menu)
    {
        foreach (var m in _menus)
        {
            if (m != menu)
            {
                m.Close();
            }
        }

        menu.Open();
    }

    /// <inheritdoc/>
    public void Toggle(IMenu menu)
    {
        if (menu.IsOpen)
        {
            menu.Close();
        }
        else
        {
            Open(menu);
        }
    }

    /// <inheritdoc/>
    public void CloseAll()
    {
        foreach (var menu in _menus)
        {
            menu.Close();
        }
    }
}

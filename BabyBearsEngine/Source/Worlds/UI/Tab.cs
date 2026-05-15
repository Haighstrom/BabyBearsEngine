using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A clickable tab button that owns a list of content items shown in the parent
/// <see cref="TabbedPanel"/>'s content area while this tab is active.
/// </summary>
public class Tab : Entity
{
    private readonly IGraphic _activeGraphic;
    private readonly IGraphic _inactiveGraphic;
    private readonly TextGraphic _titleGraphic;
    private readonly List<IAddable> _content = [];
    private Entity? _contentPanel = null;
    private bool _isActive = false;

    /// <param name="width">Width of the tab button in pixels.</param>
    /// <param name="height">Height of the tab button in pixels.</param>
    /// <param name="title">Label text displayed on the tab.</param>
    /// <param name="theme">Visual styling. Factories are invoked once per tab.</param>
    public Tab(float width, float height, string title, TabbedPanelTheme theme)
        : base(0, 0, width, height, clickable: true)
    {
        _activeGraphic = theme.ActiveTabFactory(new Rect(0, 0, width, height));
        _activeGraphic.Visible = false;
        Add(_activeGraphic);

        _inactiveGraphic = theme.InactiveTabFactory(new Rect(0, 0, width, height));
        Add(_inactiveGraphic);

        _titleGraphic = new TextGraphic(theme.TabText.Font, title, theme.TabText.Colour, 0, 0, width, height)
        {
            HAlignment = theme.TabText.HAlignment,
            VAlignment = theme.TabText.VAlignment,
        };
        Add(_titleGraphic);
    }

    /// <summary>The content items owned by this tab.</summary>
    public ReadOnlyCollection<IAddable> Content => _content.AsReadOnly();

    /// <summary>Whether this tab is currently the active (selected) tab.</summary>
    public bool IsActive => _isActive;

    /// <summary>The tab's label text.</summary>
    public string Title
    {
        get => _titleGraphic.Text;
        set => _titleGraphic.Text = value;
    }

    /// <summary>
    /// Adds an item to this tab's content. If the tab has already been added to a
    /// <see cref="TabbedPanel"/>, the item is also added to the content panel immediately
    /// and shown or hidden according to the current active state.
    /// </summary>
    public void AddContent(IAddable item)
    {
        _content.Add(item);

        if (_contentPanel is not null)
        {
            _contentPanel.Add(item);

            if (!_isActive)
            {
                if (item is IRenderable r) { r.Visible = false; }
                if (item is IUpdateable u) { u.Active = false; }
            }
        }
    }

    internal void Attach(Entity contentPanel)
    {
        _contentPanel = contentPanel;

        foreach (IAddable item in _content)
        {
            contentPanel.Add(item);

            if (!_isActive)
            {
                if (item is IRenderable r) { r.Visible = false; }
                if (item is IUpdateable u) { u.Active = false; }
            }
        }
    }

    internal void Detach()
    {
        if (_contentPanel is not null)
        {
            foreach (IAddable item in _content.Where(i => i.Parent == _contentPanel))
            {
                _contentPanel.Remove(item);
            }

            _contentPanel = null;
        }
    }

    internal void Activate()
    {
        _isActive = true;
        _activeGraphic.Visible = true;
        _inactiveGraphic.Visible = false;

        foreach (IAddable item in _content)
        {
            if (item is IRenderable r) { r.Visible = true; }
            if (item is IUpdateable u) { u.Active = true; }
        }
    }

    internal void Deactivate()
    {
        _isActive = false;
        _activeGraphic.Visible = false;
        _inactiveGraphic.Visible = true;

        foreach (IAddable item in _content)
        {
            if (item is IRenderable r) { r.Visible = false; }
            if (item is IUpdateable u) { u.Active = false; }
        }
    }
}

using System;

namespace BabyBearsEngine.Demos.Source.Menu;

internal enum MenuEntryStyle { Demo, Submenu }

internal sealed record MenuEntry(string Name, Func<World> Factory, MenuEntryStyle Style = MenuEntryStyle.Demo);

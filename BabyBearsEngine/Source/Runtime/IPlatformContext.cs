using BabyBearsEngine.Input;

namespace BabyBearsEngine.Runtime;

//Platform Abstraction Layer
public interface IPlatformContext
{
    IWindow Window { get; }
    IKeyboard Keyboard { get; }
    IMouse Mouse { get; }
    IWorldSwitcher WorldSwitcher { get; }
}

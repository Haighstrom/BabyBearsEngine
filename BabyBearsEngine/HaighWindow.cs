using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine;

public class HaighWindow(int width, int height, string title) 
    : GameWindow(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
{
}

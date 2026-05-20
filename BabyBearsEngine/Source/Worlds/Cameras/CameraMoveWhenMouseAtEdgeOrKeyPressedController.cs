using System.Collections.Generic;
using BabyBearsEngine.Input;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Scrolls a camera's view when the mouse cursor is near the edge of the window or a bound key is
/// held. If the visible world region is wider (or taller) than the world bounds, the view is centred
/// on that axis instead.
/// Add this controller to the scene alongside its camera so it receives Update calls each frame.
/// </summary>
public class CameraMoveWhenMouseAtEdgeOrKeyPressedController(
    ICamera camera,
    float cameraMoveSpeed,
    int windowEdgeDistance,
    IEnumerable<Keys> upKeys,
    IEnumerable<Keys> downKeys,
    IEnumerable<Keys> leftKeys,
    IEnumerable<Keys> rightKeys) : UpdateableBase
{
    public override void Update(double elapsed)
    {
        if (camera.View.ViewWidth >= camera.MaxX)
        {
            camera.View.X = -(camera.View.ViewWidth - camera.MaxX) / 2;
        }
        else
        {
            if (Mouse.ClientX > Window.Width - windowEdgeDistance || Keyboard.AnyKeyDown(rightKeys))
            {
                camera.View.X = Math.Min(camera.MaxX - camera.View.ViewWidth, camera.View.X + cameraMoveSpeed * (float)elapsed);
            }

            if (Mouse.ClientX < windowEdgeDistance || Keyboard.AnyKeyDown(leftKeys))
            {
                camera.View.X = Math.Max(camera.MinX, camera.View.X - cameraMoveSpeed * (float)elapsed);
            }
        }

        if (camera.View.ViewHeight >= camera.MaxY)
        {
            camera.View.Y = -(camera.View.ViewHeight - camera.MaxY) / 2;
        }
        else
        {
            if (Mouse.ClientY < windowEdgeDistance || Keyboard.AnyKeyDown(upKeys))
            {
                camera.View.Y = Math.Max(camera.MinY, camera.View.Y - cameraMoveSpeed * (float)elapsed);
            }

            if (Mouse.ClientY > Window.Height - windowEdgeDistance || Keyboard.AnyKeyDown(downKeys))
            {
                camera.View.Y = Math.Min(camera.MaxY - camera.View.ViewHeight, camera.View.Y + cameraMoveSpeed * (float)elapsed);
            }
        }
    }
}

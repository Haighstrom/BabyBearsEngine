using BabyBearsEngine.Input;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Scrolls a camera's view when the mouse cursor is near the edge of the window.
/// Add this controller to the scene alongside its camera so it receives Update calls each frame.
/// </summary>
public class CameraMoveWhenMouseAtEdgeController(ICamera camera, float cameraMoveSpeed, int windowEdgeDistance) : UpdateableBase
{
    public override void Update(double elapsed)
    {
        if (Mouse.ClientY < windowEdgeDistance)
        {
            camera.View.Y = Math.Max(camera.MinY, camera.View.Y - cameraMoveSpeed * (float)elapsed);
        }

        if (Mouse.ClientX > Window.Width - windowEdgeDistance)
        {
            camera.View.X = Math.Min(camera.MaxX - camera.View.ViewWidth, camera.View.X + cameraMoveSpeed * (float)elapsed);
        }

        if (Mouse.ClientY > Window.Height - windowEdgeDistance)
        {
            camera.View.Y = Math.Min(camera.MaxY - camera.View.ViewHeight, camera.View.Y + cameraMoveSpeed * (float)elapsed);
        }

        if (Mouse.ClientX < windowEdgeDistance)
        {
            camera.View.X = Math.Max(camera.MinX, camera.View.X - cameraMoveSpeed * (float)elapsed);
        }
    }
}

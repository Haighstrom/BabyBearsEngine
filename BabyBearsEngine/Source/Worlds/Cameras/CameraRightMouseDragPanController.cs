using BabyBearsEngine.Input;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Pans a camera's view by holding the right mouse button and dragging. The view moves with the
/// mouse delta scaled by the camera's tile size, clamped to the world bounds.
/// Add this controller to the scene alongside its camera so it receives Update calls each frame.
/// </summary>
public class CameraRightMouseDragPanController(ICamera camera) : UpdateableBase
{
    private bool _dragging = false;

    public override void Update(double elapsed)
    {
        if (Mouse.RightUp)
        {
            _dragging = false;
        }

        if (_dragging && Mouse.RightDown)
        {
            float camMoveX = Mouse.XDelta / camera.View.TileWidth;
            float camMoveY = Mouse.YDelta / camera.View.TileHeight;

            if (camera.MinX < camera.MaxX - camera.View.ViewWidth)
            {
                camera.View.X = Math.Clamp(camera.View.X + camMoveX, camera.MinX, camera.MaxX - camera.View.ViewWidth);
            }

            if (camera.MinY < camera.MaxY - camera.View.ViewHeight)
            {
                camera.View.Y = Math.Clamp(camera.View.Y + camMoveY, camera.MinY, camera.MaxY - camera.View.ViewHeight);
            }
        }

        if (Mouse.RightPressed && camera.MouseIntersecting)
        {
            _dragging = true;
        }
    }
}

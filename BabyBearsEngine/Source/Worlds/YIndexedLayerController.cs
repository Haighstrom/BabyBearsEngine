namespace BabyBearsEngine.Worlds;

/// <summary>
/// Sets an <see cref="ILayered"/> target's <see cref="ILayered.Layer"/> from a Y-position
/// source each frame, producing correct depth-sorting for top-down views.
/// </summary>
/// <remarks>
/// <para>
/// The formula is <c>Layer = max(0, layerOffset − (int)Y)</c>. Think of
/// <paramref name="layerOffset"/> as the layer the object would occupy if its Y were zero —
/// its baseline depth in the scene. As Y increases (the object moves further down the
/// screen, i.e. closer to the viewer), the layer decreases, so the object renders in front
/// of objects that are higher up.
/// </para>
/// <para>
/// To control how a Y-indexed object sorts against a fixed-layer object, set the offset
/// relative to that fixed layer. For example, if a chair is at <c>Layer = 40</c>, giving
/// the person <c>layerOffset = 39</c> ensures they are always at or in front of the chair's
/// depth when their Y is 0, and slide behind it only once their Y reaches 39+.
/// </para>
/// </remarks>
/// <param name="getY">Delegate that returns the current Y position to read each frame.</param>
/// <param name="target">The layered object whose <see cref="ILayered.Layer"/> is written each frame.</param>
/// <param name="layerOffset">
/// The baseline layer when Y is zero. Defaults to 100.
/// Increase if the object needs to sit behind other deep-layered fixed objects.
/// </param>
public class YIndexedLayerController(Func<float> getY, ILayered target, int layerOffset = 100) : UpdateableBase
{
    /// <inheritdoc/>
    public override void Update(double elapsed)
    {
        target.Layer = Math.Max(0, layerOffset - (int)getY());
    }
}

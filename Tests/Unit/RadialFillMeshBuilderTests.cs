using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RadialFillMeshBuilderTests
{
    private const float Width = 100f;
    private const float Height = 100f;
    private const int Segments = 64;

    // Zero fill

    [TestMethod]
    public void Build_ZeroAmountFilled_ReturnsEmpty()
    {
        RadialMeshVertex[] vertices = RadialFillMeshBuilder.Build(Width, Height, 0f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);

        Assert.IsEmpty(vertices);
    }

    // Pie — vertex count / angle scaling

    [TestMethod]
    public void Build_Pie_FullSweep_FirstVertexIsCentre()
    {
        RadialMeshVertex[] vertices = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);

        Assert.AreEqual(Width / 2f, vertices[0].X);
        Assert.AreEqual(Height / 2f, vertices[0].Y);
    }

    [TestMethod]
    public void Build_Pie_PartialSweep_UsesFewerArcVerticesThanFullSweep()
    {
        RadialMeshVertex[] partial = RadialFillMeshBuilder.Build(Width, Height, 0.25f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);
        RadialMeshVertex[] full = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);

        Assert.IsLessThan(full.Length, partial.Length);
    }

    [TestMethod]
    public void Build_Pie_LastArcVertex_UAndAngleMatchAmountFilled()
    {
        const float amountFilled = 0.25f;
        RadialMeshVertex[] vertices = RadialFillMeshBuilder.Build(Width, Height, amountFilled, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);
        RadialMeshVertex lastArcVertex = vertices[^1];

        Assert.AreEqual(amountFilled, lastArcVertex.U, 1e-5f);

        // 0.25 sweep clockwise from 12 o'clock lands at 3 o'clock: +X from centre, same Y as centre.
        Assert.AreEqual(Width / 2f + Height / 2f, lastArcVertex.X, 1e-3f);
        Assert.AreEqual(Height / 2f, lastArcVertex.Y, 1e-3f);
    }

    [TestMethod]
    public void Build_Pie_AllArcVertices_AtOuterRadius()
    {
        RadialMeshVertex[] vertices = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);
        float centreX = Width / 2f;
        float centreY = Height / 2f;
        float expectedRadius = Math.Min(Width, Height) / 2f;

        for (int vertexIndex = 1; vertexIndex < vertices.Length; vertexIndex++)
        {
            float distanceFromCentre = MathF.Sqrt(MathF.Pow(vertices[vertexIndex].X - centreX, 2) + MathF.Pow(vertices[vertexIndex].Y - centreY, 2));
            Assert.AreEqual(expectedRadius, distanceFromCentre, 1e-3f);
        }
    }

    // Ring — hollow centre

    [TestMethod]
    public void Build_Ring_InnerVertices_LeaveCentreHollow()
    {
        const float ringThickness = 0.3f;
        RadialMeshVertex[] vertices = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Ring, ringThickness, 0f, RadialSweepDirection.Clockwise, Segments);
        float centreX = Width / 2f;
        float centreY = Height / 2f;
        float outerRadius = Math.Min(Width, Height) / 2f;
        float expectedInnerRadius = outerRadius * (1f - ringThickness);

        // Odd-indexed vertices are the inner-edge points (V = 0).
        for (int vertexIndex = 1; vertexIndex < vertices.Length; vertexIndex += 2)
        {
            Assert.AreEqual(0f, vertices[vertexIndex].V);
            float distanceFromCentre = MathF.Sqrt(MathF.Pow(vertices[vertexIndex].X - centreX, 2) + MathF.Pow(vertices[vertexIndex].Y - centreY, 2));
            Assert.AreEqual(expectedInnerRadius, distanceFromCentre, 1e-3f);
            Assert.IsGreaterThan(0f, distanceFromCentre);
        }
    }

    [TestMethod]
    public void Build_Ring_OuterVertices_AtOuterRadius()
    {
        RadialMeshVertex[] vertices = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Ring, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);
        float centreX = Width / 2f;
        float centreY = Height / 2f;
        float expectedOuterRadius = Math.Min(Width, Height) / 2f;

        // Even-indexed vertices are the outer-edge points (V = 1).
        for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex += 2)
        {
            Assert.AreEqual(1f, vertices[vertexIndex].V);
            float distanceFromCentre = MathF.Sqrt(MathF.Pow(vertices[vertexIndex].X - centreX, 2) + MathF.Pow(vertices[vertexIndex].Y - centreY, 2));
            Assert.AreEqual(expectedOuterRadius, distanceFromCentre, 1e-3f);
        }
    }

    [TestMethod]
    public void Build_Ring_ReturnsTwiceAsManyVerticesAsAngleSteps()
    {
        RadialMeshVertex[] partial = RadialFillMeshBuilder.Build(Width, Height, 0.5f, RadialFillStyle.Ring, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);
        RadialMeshVertex[] full = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Ring, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);

        Assert.AreEqual(0, partial.Length % 2);
        Assert.AreEqual(0, full.Length % 2);
        Assert.IsLessThan(full.Length, partial.Length);
    }

    // Direction

    [TestMethod]
    public void Build_Anticlockwise_SweepsOppositeWayFromClockwise()
    {
        const float amountFilled = 0.25f;
        RadialMeshVertex[] clockwise = RadialFillMeshBuilder.Build(Width, Height, amountFilled, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, Segments);
        RadialMeshVertex[] anticlockwise = RadialFillMeshBuilder.Build(Width, Height, amountFilled, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Anticlockwise, Segments);

        RadialMeshVertex clockwiseLast = clockwise[^1];
        RadialMeshVertex anticlockwiseLast = anticlockwise[^1];

        // Clockwise from 12 o'clock at 0.25 sweep lands at 3 o'clock (+X); anticlockwise lands at 9 o'clock (-X).
        Assert.IsGreaterThan(Width / 2f, clockwiseLast.X);
        Assert.IsLessThan(Width / 2f, anticlockwiseLast.X);
    }

    // Segments — quality knob

    [TestMethod]
    public void Build_MoreSegments_ProducesMoreVertices()
    {
        RadialMeshVertex[] coarse = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, segments: 8);
        RadialMeshVertex[] fine = RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, segments: 64);

        Assert.IsLessThan(fine.Length, coarse.Length);
    }

    [TestMethod]
    public void Build_SegmentsLessThanOne_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RadialFillMeshBuilder.Build(Width, Height, 1f, RadialFillStyle.Pie, 0.3f, 0f, RadialSweepDirection.Clockwise, segments: 0));
    }
}
